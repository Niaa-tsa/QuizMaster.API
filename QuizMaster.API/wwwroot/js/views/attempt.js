// ტესტის გავლა: კითხვები, პასუხების შენახვა, ტაიმერი, შედეგი

import { api, errorMessage } from '../api.js';
import {
  esc, toast, parseUtc, confirmDialog, fmtNumber, fmtDuration, percentOf,
  QUESTION_TYPES, emptyState, setBusy
} from '../ui.js';

function loadSaved(attemptId) {
  try {
    return JSON.parse(localStorage.getItem(`qm_answers_${attemptId}`)) || {};
  } catch {
    return {};
  }
}

function storeSaved(attemptId, map) {
  localStorage.setItem(`qm_answers_${attemptId}`, JSON.stringify(map));
}

function clearSaved(attemptId) {
  localStorage.removeItem(`qm_answers_${attemptId}`);
}

function resultHtml(title, score, maxScore, percentage, message) {
  const percent = Math.max(0, Math.min(100, Number(percentage) || 0));
  let verdict = 'სცადეთ ხელახლა — წარმატება აუცილებლად მოვა!';
  if (percent >= 90) verdict = 'შესანიშნავი შედეგია! 🎉';
  else if (percent >= 70) verdict = 'ძალიან კარგი შედეგია! 👍';
  else if (percent >= 50) verdict = 'დამაკმაყოფილებელი შედეგია.';

  return `
    <div class="card result">
      <h2>${esc(title)}</h2>
      ${message ? `<p class="muted">${esc(message)}</p>` : ''}
      <div class="result-ring" style="--p:${percent}">
        <div><strong>${fmtNumber(percent)}%</strong><span class="muted small">${fmtNumber(score)} / ${fmtNumber(maxScore)}</span></div>
      </div>
      <p>${verdict}</p>
      <div class="row" style="justify-content:center">
        <a class="btn btn-primary" href="#/student">ტესტებზე დაბრუნება</a>
        <a class="btn btn-secondary" href="#/student/history">ჩემი შედეგები</a>
      </div>
    </div>`;
}

export async function attemptView(ctx) {
  const attemptId = Number(ctx.params[0]);

  const [history, quizzes] = await Promise.all([
    api.get('/api/StudentQuizzes/history'),
    api.get('/api/StudentQuizzes')
  ]);
  if (!ctx.alive()) return;

  const attempt = history.find((a) => a.attemptId === attemptId);
  if (!attempt) {
    ctx.main.innerHTML = emptyState('🔍', 'მცდელობა ვერ მოიძებნა', 'შესაძლოა ის სხვა მომხმარებელს ეკუთვნის.');
    return;
  }

  // უკვე დასრულებული მცდელობა: მხოლოდ შედეგს ვაჩვენებთ
  if (attempt.status !== 'InProgress') {
    const percent = percentOf(attempt.score, attempt.maxScore);
    ctx.main.innerHTML = resultHtml(attempt.quizTitle, attempt.score, attempt.maxScore, percent, 'ეს მცდელობა უკვე დასრულებულია.');
    return;
  }

  const quiz = quizzes.find((q) => q.id === attempt.quizId);
  const startedAt = parseUtc(attempt.startedAt);
  const deadline = quiz && startedAt
    ? new Date(startedAt.getTime() + quiz.timeLimitMinutes * 60000)
    : null;

  const questions = await api.get(`/api/StudentQuizzes/attempt/${attemptId}/questions`);
  if (!ctx.alive()) return;

  // დრო უკვე გასულია: სერვერმა ტესტი ავტომატურად დაასრულა
  if (!Array.isArray(questions)) {
    ctx.main.innerHTML = resultHtml(
      attempt.quizTitle, questions.score, questions.maxScore, questions.percentage, questions.message
    );
    clearSaved(attemptId);
    return;
  }

  const saved = loadSaved(attemptId); // { questionId: [optionIds] }
  let finishing = false;
  let timerId = null;

  ctx.main.innerHTML = `
    <div class="card attempt-bar">
      <div>
        <h2 style="margin:0">${esc(attempt.quizTitle)}</h2>
        <span class="muted small" id="progress-text"></span>
      </div>
      <div class="row">
        ${deadline ? '<span class="timer" id="timer">--:--</span>' : ''}
        <button type="button" class="btn btn-success" id="finish-btn">ტესტის დასრულება</button>
      </div>
    </div>
    <div class="progress mb"><span id="progress-bar" style="width:0%"></span></div>
    <div class="stack" id="questions">
      ${questions.map((q, i) => `
        <article class="card question" data-qid="${q.id}">
          <header>
            <span class="q-num">${i + 1}</span>
            <div style="flex:1">
              <div class="q-text">${esc(q.text)}</div>
              <div class="muted small">${esc(QUESTION_TYPES[q.type] || '')} · ${fmtNumber(q.points)} ქულა</div>
            </div>
            <span class="badge badge-muted" data-state>უპასუხო</span>
          </header>
          <div class="options">
            ${q.answerOptions.map((o) => `
              <label class="option">
                <input type="${q.type === 2 ? 'checkbox' : 'radio'}" name="q${q.id}" value="${o.id}">
                <span>${esc(o.text)}</span>
              </label>`).join('')}
          </div>
          <footer>
            <button type="button" class="btn btn-secondary btn-sm" data-save>პასუხის შენახვა</button>
          </footer>
        </article>`).join('')}
    </div>`;

  const cards = [...ctx.main.querySelectorAll('.question')];
  const timerEl = ctx.main.querySelector('#timer');
  const finishBtn = ctx.main.querySelector('#finish-btn');

  const isSaved = (card) => card.dataset.saved === '1';
  const selectedOf = (card) =>
    [...card.querySelectorAll('input:checked')].map((input) => Number(input.value));

  function updateProgress() {
    const done = cards.filter(isSaved).length;
    ctx.main.querySelector('#progress-text').textContent =
      `შენახულია ${done} / ${cards.length} კითხვა`;
    ctx.main.querySelector('#progress-bar').style.width =
      `${cards.length ? (done / cards.length) * 100 : 0}%`;
  }

  function lockCard(card, ids) {
    card.dataset.saved = '1';
    card.querySelectorAll('input').forEach((input) => {
      input.disabled = true;
      if (ids) input.checked = ids.includes(Number(input.value));
    });
    const state = card.querySelector('[data-state]');
    state.textContent = 'შენახულია';
    state.className = 'badge badge-success';
    card.querySelector('[data-save]').hidden = true;
    updateProgress();
  }

  // წინა სესიიდან შენახული პასუხების აღდგენა
  for (const card of cards) {
    const ids = saved[card.dataset.qid];
    if (ids) lockCard(card, ids);
  }
  updateProgress();

  async function saveCard(card, silent = false) {
    const questionId = Number(card.dataset.qid);
    const ids = selectedOf(card);

    if (!ids.length) {
      if (!silent) toast('ჯერ აირჩიეთ პასუხი.', 'error');
      return false;
    }

    try {
      await api.post(`/api/StudentAnswers/attempt/${attemptId}/submit`, {
        questionId,
        answerOptionIds: ids
      });
      saved[questionId] = ids;
      storeSaved(attemptId, saved);
      lockCard(card, ids);
      return true;
    } catch (err) {
      if (err.status === 400) {
        // სავარაუდოდ ამ კითხვაზე პასუხი უკვე დაფიქსირებულია
        lockCard(card, null);
        if (!silent) toast('ამ კითხვაზე პასუხი უკვე დაფიქსირებულია და ვეღარ შეიცვლება.', 'error');
      } else if (!silent) {
        toast(errorMessage(err), 'error');
      }
      return false;
    }
  }

  async function finish(auto) {
    if (finishing) return;
    finishing = true;
    clearInterval(timerId);
    setBusy(finishBtn, true, 'მიმდინარეობს...');

    try {
      if (!auto) {
        // არჩეული, მაგრამ ჯერ შეუნახავი პასუხების ავტომატური შენახვა
        for (const card of cards) {
          if (!isSaved(card) && selectedOf(card).length) {
            await saveCard(card, true);
          }
        }
      }

      const result = await api.post(`/api/StudentQuizzes/attempt/${attemptId}/finish`);
      clearSaved(attemptId);
      if (!ctx.alive()) return;

      ctx.main.innerHTML = resultHtml(
        attempt.quizTitle, result.score, result.maxScore, result.percentage,
        auto ? 'დრო ამოიწურა. ტესტი ავტომატურად დასრულდა.' : 'ტესტი წარმატებით დასრულდა.'
      );
      window.scrollTo({ top: 0 });
    } catch (err) {
      finishing = false;
      setBusy(finishBtn, false);
      toast(errorMessage(err), 'error');
      // ხელით დასრულების წარუმატებლობისას ტაიმერი გრძელდება;
      // ავტომატურზე არა, რომ მოთხოვნების ციკლი არ შეიქმნას
      if (!auto && deadline && deadline.getTime() > Date.now()) startTimer();
    }
  }

  function tick() {
    const left = deadline.getTime() - Date.now();
    if (left <= 0) {
      timerEl.textContent = '00:00';
      clearInterval(timerId);
      toast('დრო ამოიწურა. ტესტი ავტომატურად სრულდება.', 'error');
      finish(true);
      return;
    }
    timerEl.textContent = fmtDuration(left);
    timerEl.classList.toggle('danger', left < 60000);
  }

  function startTimer() {
    clearInterval(timerId);
    tick();
    timerId = setInterval(tick, 1000);
  }

  if (deadline) startTimer();

  ctx.main.addEventListener('click', (e) => {
    const saveBtn = e.target.closest('[data-save]');
    if (saveBtn) saveCard(saveBtn.closest('.question'));
  });

  finishBtn.addEventListener('click', async () => {
    const unanswered = cards.filter((c) => !isSaved(c) && !selectedOf(c).length).length;
    const message = unanswered > 0
      ? `${unanswered} კითხვას პასუხი არ აქვს. დასრულების შემდეგ პასუხების შეცვლა შეუძლებელია. გსურთ დასრულება?`
      : 'დასრულების შემდეგ პასუხების შეცვლა შეუძლებელია. გსურთ ტესტის დასრულება?';

    const ok = await confirmDialog(message, { title: 'ტესტის დასრულება', confirmText: 'დასრულება' });
    if (ok) finish(false);
  });

  // გვერდიდან გასვლისას ტაიმერის გაჩერება
  return () => clearInterval(timerId);
}
