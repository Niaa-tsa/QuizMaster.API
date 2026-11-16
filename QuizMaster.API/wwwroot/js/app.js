// აპლიკაციის შესასვლელი: როუტერი, ნავიგაცია, სესიის მართვა

import { session, api, setUnauthorizedHandler } from './api.js';
import { esc, toast, avatarHtml, spinnerHtml, renderError } from './ui.js';
import { loginView, registerView } from './views/auth.js';
import { profileView } from './views/profile.js';
import { studentView, historyView } from './views/student.js';
import { attemptView } from './views/attempt.js';
import { leaderboardView } from './views/leaderboard.js';
import { lecturerView } from './views/lecturer.js';
import { quizDetailView } from './views/quizDetail.js';
import { adminView, superAdminView } from './views/admin.js';

const HOME = {
  Student: '#/student',
  Lecturer: '#/lecturer',
  Admin: '#/admin',
  SuperAdmin: '#/superadmin'
};

const NAV = {
  Student: [['#/student', 'ტესტები'], ['#/student/history', 'ჩემი შედეგები']],
  Lecturer: [['#/lecturer', 'ჩემი ტესტები']],
  Admin: [['#/admin', 'მომხმარებლები']],
  SuperAdmin: [['#/superadmin', 'ადმინისტრატორები']]
};

// roles არ არის მითითებული → საკმარისია ავტორიზაცია
const routes = [
  { re: /^#\/login$/, public: true, view: loginView },
  { re: /^#\/register$/, public: true, view: registerView },
  { re: /^#\/profile$/, view: profileView },
  { re: /^#\/leaderboard\/(\d+)$/, roles: ['Student', 'Lecturer'], view: leaderboardView },
  { re: /^#\/student$/, roles: ['Student'], view: studentView },
  { re: /^#\/student\/history$/, roles: ['Student'], view: historyView },
  { re: /^#\/attempt\/(\d+)$/, roles: ['Student'], view: attemptView },
  { re: /^#\/lecturer$/, roles: ['Lecturer'], view: lecturerView },
  { re: /^#\/lecturer\/quiz\/(\d+)$/, roles: ['Lecturer'], view: quizDetailView },
  { re: /^#\/admin$/, roles: ['Admin'], view: adminView },
  { re: /^#\/superadmin$/, roles: ['SuperAdmin'], view: superAdminView }
];

const state = { profile: null };
let cleanup = null;
let renderId = 0;

function homeFor(user) {
  return (user && HOME[user.role]) || '#/profile';
}

function isActive(href, hash) {
  if (hash === href) return true;
  if (href === '#/lecturer') return hash.startsWith('#/lecturer/');
  if (href === '#/student') return hash.startsWith('#/attempt/') || hash.startsWith('#/leaderboard/');
  return false;
}

function renderNav() {
  const nav = document.getElementById('nav');
  const area = document.getElementById('user-area');
  const user = session.user;
  const loggedIn = !!(session.token && user);

  if (!loggedIn) {
    nav.innerHTML = '';
    area.innerHTML = `
      <a class="btn btn-ghost btn-sm" href="#/login">შესვლა</a>
      <a class="btn btn-primary btn-sm" href="#/register">რეგისტრაცია</a>`;
    return;
  }

  const hash = location.hash;
  const links = NAV[user.role] || [];
  nav.innerHTML = links
    .map(([href, label]) => `<a href="${href}" class="${isActive(href, hash) ? 'active' : ''}">${esc(label)}</a>`)
    .join('');

  const photo = state.profile ? state.profile.profilePhotoUrl : null;
  area.innerHTML = `
    <a class="user-chip" href="#/profile" title="პროფილი">
      ${avatarHtml(user.userName, photo)}
      <span>${esc(user.userName)}</span>
    </a>
    <button type="button" class="btn btn-ghost btn-sm" id="logout-btn">გასვლა</button>`;
  document.getElementById('logout-btn').addEventListener('click', logout);
}

function logout() {
  session.clear();
  state.profile = null;
  redirect('#/login');
}

async function refreshProfile() {
  if (!session.token) {
    state.profile = null;
    renderNav();
    return;
  }
  try {
    state.profile = await api.get('/api/Profile/me');
  } catch {
    state.profile = null;
  }
  renderNav();
}

function redirect(hash) {
  if (location.hash === hash) {
    navigate();
  } else {
    location.hash = hash;
  }
}

async function navigate() {
  if (cleanup) {
    try { cleanup(); } catch { /* ignore */ }
    cleanup = null;
  }

  const id = ++renderId;
  const hash = location.hash;
  const user = session.user;
  const loggedIn = !!(session.token && user);

  if (!hash || hash === '#' || hash === '#/') {
    redirect(loggedIn ? homeFor(user) : '#/login');
    return;
  }

  let route = null;
  let match = null;
  for (const r of routes) {
    const m = hash.match(r.re);
    if (m) {
      route = r;
      match = m;
      break;
    }
  }

  const host = document.getElementById('app');

  if (!route) {
    renderNav();
    host.innerHTML = `
      <div class="empty card">
        <div class="icon">🧭</div>
        <h3>გვერდი ვერ მოიძებნა</h3>
        <p><a href="#/">მთავარ გვერდზე დაბრუნება</a></p>
      </div>`;
    return;
  }

  if (!route.public && !loggedIn) {
    redirect('#/login');
    return;
  }
  if (route.public && loggedIn) {
    redirect(homeFor(user));
    return;
  }
  if (route.roles && !route.roles.includes(user.role)) {
    toast('ამ გვერდზე წვდომა არ გაქვთ.', 'error');
    redirect(homeFor(user));
    return;
  }

  renderNav();

  // ყოველი გვერდისთვის ახალი კონტეინერი, რომ event listener-ები არ დაგროვდეს
  const container = document.createElement('div');
  container.innerHTML = spinnerHtml;
  host.replaceChildren(container);

  const ctx = {
    main: container,
    params: match.slice(1),
    user,
    alive: () => id === renderId,
    go: (target) => redirect(target),
    goHome: () => redirect(homeFor(session.user)),
    refreshProfile
  };

  try {
    const result = await route.view(ctx);
    if (typeof result === 'function') {
      if (ctx.alive()) cleanup = result;
      else result();
    }
  } catch (err) {
    if (ctx.alive()) {
      renderError(container, err, () => navigate());
    }
  }
}

setUnauthorizedHandler(() => {
  state.profile = null;
  toast('სესია ამოიწურა. გთხოვთ, ხელახლა შეხვიდეთ.', 'error');
  redirect('#/login');
});

window.addEventListener('hashchange', navigate);

(async function start() {
  if (session.token) await refreshProfile();
  navigate();
})();
