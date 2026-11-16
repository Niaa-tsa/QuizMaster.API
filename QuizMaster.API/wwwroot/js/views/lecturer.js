// ლექტორი: ტესტების სია, შექმნა/რედაქტირება, გამოქვეყნება, დახურვა, წაშლა, სტატისტიკა

import { api, errorMessage } from '../api.js';
import {
  esc, toast, badge, pageHeader, emptyState, fmtDate, fmtNumber, percentOf,
  openModal, confirmDialog, setBusy, showAlert, hideAlert
} from '../ui.js';

// ---------- ტესტის შექმნის / რედაქტირების მოდალი ----------
export function openQuizModal({ quiz = null, onSaved }) {
  const editing = !!quiz;

  const modal = openModal({
    title: editing ? 'ტესტის რედაქტირება' : 'ახალი ტესტი',
    html: `
      <form class="form" id="quiz-form" novalidate>
        <div class="field">
          <label for="qz-title">სათაური</label>
          <input class="input" id="qz-title" maxlength="200" value="${esc(quiz ? quiz.title : '')}">
        </div>
        <div class="field-row">
          <div class="field">
            <label for="qz-time">დრო (წუთი)</label>
            <input class="input" id="qz-time" type="number" min="1" max="300" value="${esc(quiz ? quiz.timeLimitMinutes : 15)}">
          </div>
          <div class="field">
            <label for="qz-attempts">მაქს. მცდელობა</label>
            <input class="input" id="qz-attempts" type="number" min="1" max="10" value="${esc(quiz ? (quiz.maxAttempts ?? 1) : 1)}">
          </div>
        </div>
        <div class="alert alert-error" id="qz-error" hidden></div>
        <div class="modal-actions">
          <button type="button" class="btn btn-ghost" data-cancel>გაუქმება</button>
          <button type="submit" class="btn btn-primary">შენახვა</button>
        </div>
      </form>`
  });

  const form = modal.el.querySelector('#quiz-form');
  const errorBox = modal.el.querySelector('#qz-error');
  const submit = form.querySelector('button[type="submit"]');
  modal.el.querySelector('[data-cancel]').addEventListener('click', modal.close);

  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    hideAlert(errorBox);

    const title = form.querySelector('#qz-title').value.trim();
    const timeLimitMinutes = Number(form.querySelector('#qz-time').value);
    const maxAttempts = Number(form.querySelector('#qz-attempts').value);

    if (title.length < 3 || title.length > 200) {
      showAlert(errorBox, 'სათაური 3-დან 200 სიმბოლომდე უნდა იყოს.');
      return;
    }
    if (!Number.isInteger(timeLimitMinutes) || timeLimitMinutes < 1 || timeLimitMinutes > 300) {
      showAlert(errorBox, 'დრო 1-დან 300 წუთამდე უნდა იყოს.');
      return;
    }
    if (!Number.isInteger(maxAttempts) || maxAttempts < 1 || maxAttempts > 10) {
      showAlert(errorBox, 'მცდელობების რაოდენობა 1-დან 10-მდე უნდა იყოს.');
      return;
    }

    const payload = { title, timeLimitMinutes, maxAttempts };

    setBusy(submit, true, 'ინახება...');
    try {
      if (editing) await api.put(`/api/Quiz/${quiz.id}`, payload);
      else await api.post('/api/Quiz', payload);

      toast(editing ? 'ტესტი განახლდა.' : 'ტესტი შეიქმნა.', 'success');
      modal.close();
      if (onSaved) await onSaved();
    } catch (err) {
      showAlert(errorBox, errorMessage(err));
      setBusy(submit, false);
    }
  });
}

// ---------- სტატისტიკა ----------
export function statsHtml(stats) {
  const avgPercent = percentOf(stats.averageScore, stats.maxScore);
  return `
    <div class="stat-grid">
      <div class="stat"><strong>${esc(stats.attemptsCount)}</strong><span>მცდელობა</span></div>
      <div class="stat"><strong>${fmtNumber(stats.averageScore)}</strong><span>საშუალო ქულა (${fmtNumber(avgPercent)}%)</span></div>
      <div class="stat"><strong>${fmtNumber(stats.highestScore)}</strong><span>უმაღლესი</span></div>
      <div class="stat"><strong>${fmtNumber(stats.lowestScore)}</strong><span>უმდაბლესი</span></div>
      <div class="stat"><strong>${fmtNumber(stats.maxScore)}</strong><span>მაქსიმალური ქულა</span></div>
    </div>`;
}

export async function openStatsModal(quiz) {
  try {
    const stats = await api.get(`/api/quizzes/${quiz.id}/statistics`);
    openModal({ title: `სტატისტიკა: ${quiz.title}`, html: statsHtml(stats), wide: true });
  } catch (err) {
    toast(errorMessage(err), 'error');
  }
}

// ---------- ტესტზე მოქმედებები (საერთოა სიისა და დეტალური გვერდისთვის) ----------
export async function runQuizAction(action, quiz, onDone) {
  try {
    if (action === 'publish') {
      await api.patch(`/api/Quiz/${quiz.id}/publish`);
      toast('ტესტი გამოქვეყნდა და სტუდენტებისთვის ხელმისაწვდომია.', 'success');
    } else if (action === 'close') {
      const ok = await confirmDialog(
        'დახურვის შემდეგ სტუდენტები ამ ტესტს ვეღარ დაიწყებენ. გსურთ ტესტის დახურვა?',
        { title: 'ტესტის დახურვა', confirmText: 'დახურვა' }
      );
      if (!ok) return;
      await api.patch(`/api/Quiz/${quiz.id}/close`);
      toast('ტესტი დაიხურა.', 'success');
    } else if (action === 'delete') {
      const ok = await confirmDialog(
        `წაიშლება ტესტი „${quiz.title}“ და მისი ყველა კითხვა. გსურთ გაგრძელება?`,
        { title: 'ტესტის წაშლა', confirmText: 'წაშლა', danger: true }
      );
      if (!ok) return;
      await api.del(`/api/Quiz/${quiz.id}`);
      toast('ტესტი წაიშალა.', 'success');
    }
    if (onDone) await onDone(action);
  } catch (err) {
    toast(errorMessage(err), 'error');
  }
}

// ---------- ტესტების სია ----------
export async function lecturerView(ctx) {
  let quizzes = await api.get('/api/Quiz');
  if (!ctx.alive()) return;

  function render() {
    const cards = quizzes.map((quiz) => `
      <article class="card quiz-card">
        <div class="row-between">
          <h3>${esc(quiz.title)}</h3>
          ${quiz.isPublished ? badge('გამოქვეყნებული', 'success') : badge('დრაფტი', 'muted')}
        </div>
        <div class="meta">
          <span>⏱ ${esc(quiz.timeLimitMinutes)} წთ</span>
          <span>🔁 მცდელობა: ${esc(quiz.maxAttempts ?? 1)}</span>
          <span>📅 ${fmtDate(quiz.createdAt)}</span>
        </div>
        <div class="card-actions">
          <a class="btn btn-primary btn-sm" href="#/lecturer/quiz/${quiz.id}">კითხვები</a>
          ${quiz.isPublished
            ? `<button type="button" class="btn btn-secondary btn-sm" data-act="close" data-id="${quiz.id}">დახურვა</button>`
            : `<button type="button" class="btn btn-success btn-sm" data-act="publish" data-id="${quiz.id}">გამოქვეყნება</button>`}
          <button type="button" class="btn btn-secondary btn-sm" data-act="stats" data-id="${quiz.id}">სტატისტიკა</button>
          <a class="btn btn-secondary btn-sm" href="#/leaderboard/${quiz.id}">ლიდერბორდი</a>
          <button type="button" class="btn btn-secondary btn-sm" data-act="edit" data-id="${quiz.id}">რედაქტირება</button>
          <button type="button" class="btn btn-ghost btn-sm" data-act="delete" data-id="${quiz.id}">წაშლა</button>
        </div>
      </article>`).join('');

    ctx.main.innerHTML = `
      ${pageHeader('ჩემი ტესტები', 'შექმენით ტესტები, დაამატეთ კითხვები და გამოაქვეყნეთ.',
        '<button type="button" class="btn btn-primary" data-act="create">+ ახალი ტესტი</button>')}
      ${quizzes.length
        ? `<div class="grid">${cards}</div>`
        : emptyState('📝', 'ტესტები ჯერ არ გაქვთ', 'დააჭირეთ „ახალი ტესტი“ და შექმენით პირველი.')}`;
  }

  async function reload() {
    quizzes = await api.get('/api/Quiz');
    if (ctx.alive()) render();
  }

  render();

  ctx.main.addEventListener('click', (e) => {
    const button = e.target.closest('[data-act]');
    if (!button) return;

    const action = button.dataset.act;
    if (action === 'create') {
      openQuizModal({ onSaved: reload });
      return;
    }

    const quiz = quizzes.find((q) => q.id === Number(button.dataset.id));
    if (!quiz) return;

    if (action === 'edit') openQuizModal({ quiz, onSaved: reload });
    else if (action === 'stats') openStatsModal(quiz);
    else runQuizAction(action, quiz, reload);
  });
}
