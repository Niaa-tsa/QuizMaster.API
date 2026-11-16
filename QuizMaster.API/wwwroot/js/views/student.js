// სტუდენტი: ტესტების სია და მცდელობების ისტორია

import { api, errorMessage } from '../api.js';
import {
  esc, toast, badge, pageHeader, emptyState, fmtDate, fmtNumber,
  percentOf, statusBadge, setBusy
} from '../ui.js';

export async function studentView(ctx) {
  const [quizzes, history] = await Promise.all([
    api.get('/api/StudentQuizzes'),
    api.get('/api/StudentQuizzes/history')
  ]);
  if (!ctx.alive()) return;

  const attemptsByQuiz = new Map();
  for (const attempt of history) {
    if (!attemptsByQuiz.has(attempt.quizId)) attemptsByQuiz.set(attempt.quizId, []);
    attemptsByQuiz.get(attempt.quizId).push(attempt);
  }

  const cards = quizzes.map((quiz) => {
    const attempts = attemptsByQuiz.get(quiz.id) || [];
    const maxAttempts = quiz.maxAttempts ?? 1;
    const running = attempts.find((a) => a.status === 'InProgress');
    const finished = attempts.filter((a) => a.status === 'Finished' && a.maxScore > 0);
    const best = finished.length
      ? Math.max(...finished.map((a) => percentOf(a.score, a.maxScore)))
      : null;
    const exhausted = !running && attempts.length >= maxAttempts;

    let startButton;
    if (running) {
      startButton = `<button type="button" class="btn btn-primary" data-start="${quiz.id}">გაგრძელება</button>`;
    } else if (exhausted) {
      startButton = '<button type="button" class="btn btn-secondary" disabled>მცდელობები ამოიწურა</button>';
    } else {
      startButton = `<button type="button" class="btn btn-primary" data-start="${quiz.id}">ტესტის დაწყება</button>`;
    }

    return `
      <article class="card quiz-card">
        <div class="row-between">
          <h3>${esc(quiz.title)}</h3>
          ${running ? badge('მიმდინარე', 'warning') : ''}
        </div>
        <div class="meta">
          <span>⏱ ${esc(quiz.timeLimitMinutes)} წთ</span>
          <span>🔁 მცდელობა: ${attempts.length} / ${esc(maxAttempts)}</span>
          ${best !== null ? `<span>🏆 საუკეთესო: ${fmtNumber(best)}%</span>` : ''}
        </div>
        <div class="card-actions">
          ${startButton}
          <a class="btn btn-secondary" href="#/leaderboard/${quiz.id}">ლიდერბორდი</a>
        </div>
      </article>`;
  }).join('');

  ctx.main.innerHTML = `
    ${pageHeader('ხელმისაწვდომი ტესტები', 'აირჩიეთ ტესტი და დაიწყეთ გავლა.',
      '<a class="btn btn-secondary" href="#/student/history">ჩემი შედეგები</a>')}
    ${quizzes.length
      ? `<div class="grid">${cards}</div>`
      : emptyState('📭', 'ჯერ გამოქვეყნებული ტესტები არ არის', 'ტესტები გამოჩნდება მას შემდეგ, რაც ლექტორი გამოაქვეყნებს.')}`;

  ctx.main.addEventListener('click', async (e) => {
    const button = e.target.closest('[data-start]');
    if (!button) return;

    setBusy(button, true, 'იტვირთება...');
    try {
      const attempt = await api.post(`/api/StudentQuizzes/${button.dataset.start}/start`);
      ctx.go(`#/attempt/${attempt.id}`);
    } catch (err) {
      toast(errorMessage(err), 'error');
      setBusy(button, false);
    }
  });
}

export async function historyView(ctx) {
  const history = await api.get('/api/StudentQuizzes/history');
  if (!ctx.alive()) return;

  const finished = history.filter((a) => a.status === 'Finished' && a.maxScore > 0);
  const average = finished.length
    ? finished.reduce((sum, a) => sum + percentOf(a.score, a.maxScore), 0) / finished.length
    : null;

  const rows = [...history]
    .sort((a, b) => new Date(b.startedAt) - new Date(a.startedAt))
    .map((a) => {
      const hasScore = a.score !== null && a.score !== undefined && a.maxScore > 0;
      const percent = hasScore ? percentOf(a.score, a.maxScore) : null;
      return `
        <tr>
          <td><strong>${esc(a.quizTitle)}</strong></td>
          <td>${fmtDate(a.startedAt)}</td>
          <td>${a.finishedAt ? fmtDate(a.finishedAt) : '—'}</td>
          <td>${statusBadge(a.status)}</td>
          <td>${hasScore ? `${fmtNumber(a.score)} / ${fmtNumber(a.maxScore)}` : '—'}</td>
          <td style="min-width:130px">
            ${hasScore
              ? `<div class="progress ${percent >= 50 ? 'success' : ''}"><span style="width:${percent}%"></span></div>
                 <span class="small muted">${fmtNumber(percent)}%</span>`
              : '—'}
          </td>
          <td class="right">
            ${a.status === 'InProgress'
              ? `<a class="btn btn-primary btn-sm" href="#/attempt/${a.attemptId}">გაგრძელება</a>`
              : `<a class="btn btn-ghost btn-sm" href="#/leaderboard/${a.quizId}">ლიდერბორდი</a>`}
          </td>
        </tr>`;
    }).join('');

  ctx.main.innerHTML = `
    ${pageHeader('ჩემი შედეგები', 'თქვენი მცდელობების ისტორია.',
      '<a class="btn btn-secondary" href="#/student">ტესტებზე დაბრუნება</a>')}
    ${history.length ? `
      <div class="stat-grid mb">
        <div class="stat"><strong>${history.length}</strong><span>სულ მცდელობა</span></div>
        <div class="stat"><strong>${finished.length}</strong><span>დასრულებული</span></div>
        <div class="stat"><strong>${average !== null ? fmtNumber(average) + '%' : '—'}</strong><span>საშუალო შედეგი</span></div>
      </div>
      <div class="card table-wrap">
        <table>
          <thead>
            <tr><th>ტესტი</th><th>დაწყება</th><th>დასრულება</th><th>სტატუსი</th><th>ქულა</th><th>შედეგი</th><th></th></tr>
          </thead>
          <tbody>${rows}</tbody>
        </table>
      </div>`
      : emptyState('📊', 'მცდელობები ჯერ არ გაქვთ', 'დაიწყეთ ნებისმიერი ტესტი და შედეგები აქ გამოჩნდება.')}`;
}
