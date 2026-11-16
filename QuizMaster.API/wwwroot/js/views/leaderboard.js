// ტესტის ლიდერბორდი (გვერდებად)

import { api } from '../api.js';
import { esc, fmtDate, fmtNumber, percentOf, emptyState, renderError, pageHeader } from '../ui.js';

const MEDALS = { 1: '🥇', 2: '🥈', 3: '🥉' };

export async function leaderboardView(ctx) {
  const quizId = Number(ctx.params[0]);
  const me = ctx.user;
  let title = `ტესტი #${quizId}`;

  // სათაურს ვცდილობთ ამოვიღოთ ტესტების სიიდან (API ლიდერბორდში სათაურს არ აბრუნებს)
  try {
    let list = [];
    if (me.role === 'Student') list = await api.get('/api/StudentQuizzes');
    else if (me.role === 'Lecturer') list = await api.get('/api/Quiz');
    const quiz = list.find((q) => q.id === quizId);
    if (quiz) title = quiz.title;
  } catch {
    /* სათაური არ არის კრიტიკული */
  }
  if (!ctx.alive()) return;

  const backHref = me.role === 'Lecturer' ? `#/lecturer/quiz/${quizId}` : '#/student';
  let page = 1;
  let pageSize = 10;

  ctx.main.innerHTML = `
    ${pageHeader('ლიდერბორდი', title, `<a class="btn btn-secondary" href="${backHref}">უკან</a>`)}
    <div class="card">
      <div class="row-between mb">
        <span class="muted small">საუკეთესო შედეგების რეიტინგი</span>
        <label class="row small">გვერდზე:
          <select id="page-size" style="width:auto">
            ${[5, 10, 20, 50].map((n) => `<option value="${n}" ${n === pageSize ? 'selected' : ''}>${n}</option>`).join('')}
          </select>
        </label>
      </div>
      <div id="board"></div>
      <div class="pager">
        <button type="button" class="btn btn-secondary btn-sm" id="prev">← წინა</button>
        <span id="page-label" class="muted"></span>
        <button type="button" class="btn btn-secondary btn-sm" id="next">შემდეგი →</button>
      </div>
    </div>`;

  const board = ctx.main.querySelector('#board');
  const prev = ctx.main.querySelector('#prev');
  const next = ctx.main.querySelector('#next');
  const label = ctx.main.querySelector('#page-label');

  async function load() {
    board.innerHTML = '<div class="spinner"></div>';
    prev.disabled = true;
    next.disabled = true;

    try {
      const entries = await api.get(`/api/quizzes/${quizId}/leaderboard?page=${page}&pageSize=${pageSize}`);
      if (!ctx.alive()) return;

      if (!entries.length) {
        board.innerHTML = emptyState('🏁', page === 1 ? 'შედეგები ჯერ არ არის' : 'ამ გვერდზე ჩანაწერები არ არის');
      } else {
        board.innerHTML = `
          <div class="table-wrap">
            <table>
              <thead><tr><th>#</th><th>სტუდენტი</th><th>ქულა</th><th>შედეგი</th><th>დასრულდა</th></tr></thead>
              <tbody>
                ${entries.map((e) => `
                  <tr class="${e.userName === me.userName ? 'me' : ''}">
                    <td><strong>${MEDALS[e.rank] || esc(e.rank)}</strong></td>
                    <td>${esc(e.userName)}${e.userName === me.userName ? ' <span class="badge badge-primary">თქვენ</span>' : ''}</td>
                    <td>${fmtNumber(e.score)} / ${fmtNumber(e.maxScore)}</td>
                    <td>${fmtNumber(percentOf(e.score, e.maxScore))}%</td>
                    <td class="muted small">${fmtDate(e.finishedAt)}</td>
                  </tr>`).join('')}
              </tbody>
            </table>
          </div>`;
      }

      label.textContent = `გვერდი ${page}`;
      prev.disabled = page <= 1;
      next.disabled = entries.length < pageSize;
    } catch (err) {
      renderError(board, err, load);
    }
  }

  prev.addEventListener('click', () => { page -= 1; load(); });
  next.addEventListener('click', () => { page += 1; load(); });
  ctx.main.querySelector('#page-size').addEventListener('change', (e) => {
    pageSize = Number(e.target.value);
    page = 1;
    load();
  });

  await load();
}
