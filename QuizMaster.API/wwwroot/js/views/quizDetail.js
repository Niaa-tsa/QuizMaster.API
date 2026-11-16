// ლექტორი: ერთი ტესტის გვერდი (კითხვები, პასუხის ვარიანტები, სტატისტიკა)

import { api, errorMessage } from '../api.js';
import {
  esc, toast, badge, pageHeader, emptyState, fmtDate, fmtNumber, QUESTION_TYPES,
  openModal, confirmDialog, setBusy, showAlert, hideAlert
} from '../ui.js';
import { openQuizModal, runQuizAction, statsHtml } from './lecturer.js';

// ---------- პასუხის ვარიანტის მოდალი (POST / PUT /api/AnswerOption) ----------
function openOptionModal({ questionId, option = null, onSaved }) {
  const editing = !!option;

  const modal = openModal({
    title: editing ? 'ვარიანტის რედაქტირება' : 'ახალი ვარიანტი',
    html: `
      <form class="form" id="opt-form" novalidate>
        <div class="field">
          <label for="opt-text">ტექსტი</label>
          <input class="input" id="opt-text" maxlength="500" value="${esc(option ? option.text : '')}">
        </div>
        <label class="row">
          <input type="checkbox" id="opt-correct" ${option && option.isCorrect ? 'checked' : ''}>
          <span>სწორი პასუხია</span>
        </label>
        <div class="alert alert-error" id="opt-error" hidden></div>
        <div class="modal-actions">
          <button type="button" class="btn btn-ghost" data-cancel>გაუქმება</button>
          <button type="submit" class="btn btn-primary">შენახვა</button>
        </div>
      </form>`
  });

  const form = modal.el.querySelector('#opt-form');
  const errorBox = modal.el.querySelector('#opt-error');
  const submit = form.querySelector('button[type="submit"]');
  modal.el.querySelector('[data-cancel]').addEventListener('click', modal.close);

  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    hideAlert(errorBox);

    const text = form.querySelector('#opt-text').value.trim();
    const isCorrect = form.querySelector('#opt-correct').checked;

    if (!text || text.length > 500) {
      showAlert(errorBox, 'ტექსტი 1-დან 500 სიმბოლომდე უნდა იყოს.');
      return;
    }

    setBusy(submit, true, 'ინახება...');
    try {
      if (editing) await api.put(`/api/AnswerOption/${option.id}`, { id: option.id, text, isCorrect });
      else await api.post(`/api/AnswerOption/question/${questionId}`, { text, isCorrect });

      toast('ვარიანტი შენახულია.', 'success');
      modal.close();
      if (onSaved) await onSaved();
    } catch (err) {
      showAlert(errorBox, errorMessage(err));
      setBusy(submit, false);
    }
  });
}

// ---------- კითხვის მოდალი (POST / PUT /api/Question) ----------
function openQuestionModal({ quizId, question = null, options = [], onSaved }) {
  const editing = !!question;

  const model = {
    type: question ? Number(question.type) : 1,
    options: editing
      ? options.map((o) => ({ id: o.id, text: o.text, isCorrect: o.isCorrect }))
      : [{ text: '', isCorrect: true }, { text: '', isCorrect: false }]
  };

  const modal = openModal({
    title: editing ? 'კითხვის რედაქტირება' : 'ახალი კითხვა',
    wide: true,
    html: `
      <form class="form" id="q-form" novalidate>
        <div class="field-row">
          <div class="field">
            <label for="q-type">კითხვის ტიპი</label>
            <select id="q-type">
              ${[1, 2, 3].map((t) => `<option value="${t}" ${t === model.type ? 'selected' : ''}>${esc(QUESTION_TYPES[t])}</option>`).join('')}
            </select>
          </div>
          <div class="field">
            <label for="q-points">ქულა</label>
            <input class="input" id="q-points" type="number" min="0.01" max="999999.99" step="0.01" value="${esc(question ? question.points : 1)}">
          </div>
        </div>
        <div class="field">
          <label for="q-text">კითხვის ტექსტი</label>
          <textarea id="q-text" rows="3" maxlength="1000">${esc(question ? question.text : '')}</textarea>
        </div>
        <div class="field">
          <label>პასუხის ვარიანტები</label>
          <div id="q-options"></div>
          <div class="row">
            <button type="button" class="btn btn-ghost btn-sm" id="q-add">+ ვარიანტი</button>
            <span class="hint" id="q-hint"></span>
          </div>
        </div>
        <div class="alert alert-error" id="q-error" hidden></div>
        <div class="modal-actions">
          <button type="button" class="btn btn-ghost" data-cancel>გაუქმება</button>
          <button type="submit" class="btn btn-primary">შენახვა</button>
        </div>
      </form>`
  });

  const root = modal.el;
  const form = root.querySelector('#q-form');
  const typeSelect = root.querySelector('#q-type');
  const optionsBox = root.querySelector('#q-options');
  const addBtn = root.querySelector('#q-add');
  const hint = root.querySelector('#q-hint');
  const errorBox = root.querySelector('#q-error');
  const submit = form.querySelector('button[type="submit"]');
  root.querySelector('[data-cancel]').addEventListener('click', modal.close);

  function renderOptions() {
    const isTrueFalse = model.type === 3;

    optionsBox.innerHTML = model.options.map((o, i) => `
      <div class="opt-row">
        <input type="${model.type === 2 ? 'checkbox' : 'radio'}" name="correct" data-correct="${i}"
               ${o.isCorrect ? 'checked' : ''} title="სწორი პასუხი">
        <input class="input" data-text="${i}" maxlength="500" value="${esc(o.text)}"
               placeholder="ვარიანტი ${i + 1}" ${isTrueFalse ? 'readonly' : ''}>
        ${isTrueFalse ? '' : `<button type="button" class="icon-btn" data-remove="${i}" ${model.options.length <= 2 ? 'disabled' : ''}>✕</button>`}
      </div>`).join('');

    addBtn.hidden = isTrueFalse;
    hint.textContent = model.type === 1
      ? 'მონიშნეთ ზუსტად ერთი სწორი ვარიანტი.'
      : model.type === 2
        ? 'მონიშნეთ მინიმუმ ორი სწორი ვარიანტი.'
        : 'მონიშნეთ, რომელია სწორი: True თუ False.';
  }

  typeSelect.addEventListener('change', () => {
    const next = Number(typeSelect.value);

    if (next === 3) {
      const old = model.options;
      model.options = [
        { id: old[0] ? old[0].id : undefined, text: 'True', isCorrect: true },
        { id: old[1] ? old[1].id : undefined, text: 'False', isCorrect: false }
      ];
    } else if (next === 1) {
      let seen = false;
      model.options.forEach((o) => {
        if (o.isCorrect) {
          if (seen) o.isCorrect = false;
          seen = true;
        }
      });
    }
    model.type = next;
    renderOptions();
  });

  optionsBox.addEventListener('input', (e) => {
    const index = e.target.dataset.text;
    if (index !== undefined) model.options[Number(index)].text = e.target.value;
  });

  optionsBox.addEventListener('change', (e) => {
    const index = e.target.dataset.correct;
    if (index === undefined) return;

    if (model.type === 2) {
      model.options[Number(index)].isCorrect = e.target.checked;
    } else {
      model.options.forEach((o, i) => { o.isCorrect = i === Number(index); });
    }
  });

  optionsBox.addEventListener('click', (e) => {
    const button = e.target.closest('[data-remove]');
    if (!button || model.options.length <= 2) return;
    model.options.splice(Number(button.dataset.remove), 1);
    renderOptions();
  });

  addBtn.addEventListener('click', () => {
    if (model.options.length >= 10) {
      toast('ვარიანტების მაქსიმალური რაოდენობაა 10.', 'error');
      return;
    }
    model.options.push({ text: '', isCorrect: false });
    renderOptions();
  });

  function validate(text, points) {
    if (!text) return 'შეიყვანეთ კითხვის ტექსტი.';
    if (text.length > 1000) return 'კითხვის ტექსტი 1000 სიმბოლოზე მეტი ვერ იქნება.';
    if (!(points >= 0.01 && points <= 999999.99)) return 'ქულა 0.01-დან 999999.99-მდე უნდა იყოს.';

    const list = model.options;
    if (list.length < 2) return 'საჭიროა მინიმუმ 2 ვარიანტი.';
    if (list.some((o) => !o.text.trim())) return 'ყველა ვარიანტს უნდა ჰქონდეს ტექსტი.';

    const correct = list.filter((o) => o.isCorrect).length;
    if (model.type === 1 && correct !== 1) return 'ამ ტიპის კითხვას ზუსტად 1 სწორი ვარიანტი უნდა ჰქონდეს.';
    if (model.type === 2 && correct < 2) return 'ამ ტიპის კითხვას მინიმუმ 2 სწორი ვარიანტი უნდა ჰქონდეს.';
    if (model.type === 3 && (list.length !== 2 || correct !== 1)) {
      return 'True/False კითხვას ზუსტად 2 ვარიანტი და 1 სწორი პასუხი უნდა ჰქონდეს.';
    }
    return null;
  }

  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    hideAlert(errorBox);

    const text = root.querySelector('#q-text').value.trim();
    const points = Number(root.querySelector('#q-points').value);

    const problem = validate(text, points);
    if (problem) {
      showAlert(errorBox, problem);
      return;
    }

    const payload = {
      text,
      type: model.type,
      points,
      answerOptions: model.options.map((o) => ({
        id: o.id ?? null,
        text: o.text.trim(),
        isCorrect: o.isCorrect
      }))
    };

    setBusy(submit, true, 'ინახება...');
    try {
      if (editing) await api.put(`/api/Question/${question.id}`, payload);
      else await api.post(`/api/Question?quizId=${quizId}`, payload);

      toast(editing ? 'კითხვა განახლდა.' : 'კითხვა დაემატა.', 'success');
      modal.close();
      if (onSaved) await onSaved();
    } catch (err) {
      showAlert(errorBox, errorMessage(err));
      setBusy(submit, false);
    }
  });

  renderOptions();
}

// ---------- გვერდი ----------
export async function quizDetailView(ctx) {
  const quizId = Number(ctx.params[0]);

  let quiz = null;
  let questions = [];
  let optionsByQuestion = new Map();
  let stats = null;
  let missingQuizId = false;

  async function load() {
    const [quizResult, allQuestions] = await Promise.all([
      api.get(`/api/Quiz/${quizId}`),
      api.get('/api/Question')
    ]);

    quiz = quizResult;
    missingQuizId = allQuestions.length > 0 && allQuestions.some((q) => q.quizId === undefined);
    questions = allQuestions.filter((q) => q.quizId === quizId).sort((a, b) => a.id - b.id);

    const lists = await Promise.all(
      questions.map((q) => api.get(`/api/AnswerOption/question/${q.id}`))
    );
    optionsByQuestion = new Map(questions.map((q, i) => [q.id, lists[i]]));

    try {
      stats = await api.get(`/api/quizzes/${quizId}/statistics`);
    } catch {
      stats = null;
    }
  }

  await load();
  if (!ctx.alive()) return;

  function questionHtml(q, index, locked) {
    const options = optionsByQuestion.get(q.id) || [];
    return `
      <article class="card">
        <div class="row-between">
          <div class="row" style="align-items:flex-start;flex:1">
            <span class="q-num">${index + 1}</span>
            <div style="flex:1">
              <div class="q-text">${esc(q.text)}</div>
              <div class="muted small">${esc(QUESTION_TYPES[q.type] || '')} · ${fmtNumber(q.points)} ქულა</div>
            </div>
          </div>
          ${locked ? '' : `
            <div class="row">
              <button type="button" class="btn btn-secondary btn-sm" data-act="q-edit" data-id="${q.id}">რედაქტირება</button>
              <button type="button" class="btn btn-ghost btn-sm" data-act="q-del" data-id="${q.id}">წაშლა</button>
            </div>`}
        </div>
        <ul class="opt-list">
          ${options.map((o) => `
            <li class="opt-item ${o.isCorrect ? 'correct' : ''}">
              <span>${o.isCorrect ? '✔' : '○'}</span>
              <span class="grow">${esc(o.text)}</span>
              ${locked ? '' : `
                <button type="button" class="icon-btn" title="რედაქტირება" data-act="opt-edit" data-id="${o.id}" data-qid="${q.id}">✎</button>
                <button type="button" class="icon-btn" title="წაშლა" data-act="opt-del" data-id="${o.id}">✕</button>`}
            </li>`).join('')}
        </ul>
        ${locked ? '' : `<button type="button" class="btn btn-ghost btn-sm mt" data-act="opt-add" data-id="${q.id}">+ ვარიანტის დამატება</button>`}
      </article>`;
  }

  function render() {
    const attempts = stats ? Number(stats.attemptsCount) : 0;
    const locked = quiz.isPublished || attempts > 0;

    const lockReason = quiz.isPublished
      ? 'ტესტი გამოქვეყნებულია, ამიტომ კითხვების შეცვლა შეუძლებელია. ჯერ დახურეთ ტესტი.'
      : 'ამ ტესტზე სტუდენტებს უკვე აქვთ მცდელობები, ამიტომ კითხვების შეცვლა შეუძლებელია.';

    ctx.main.innerHTML = `
      ${pageHeader(quiz.title, '', `
        <a class="btn btn-ghost" href="#/lecturer">← ტესტების სია</a>
        <a class="btn btn-secondary" href="#/leaderboard/${quiz.id}">ლიდერბორდი</a>`)}

      <div class="card mb">
        <div class="row-between">
          <div class="row">
            ${quiz.isPublished ? badge('გამოქვეყნებული', 'success') : badge('დრაფტი', 'muted')}
            <span class="muted">⏱ ${esc(quiz.timeLimitMinutes)} წთ</span>
            <span class="muted">🔁 მცდელობა: ${esc(quiz.maxAttempts ?? 1)}</span>
            <span class="muted small">📅 ${fmtDate(quiz.createdAt)}</span>
          </div>
          <div class="row">
            ${quiz.isPublished
              ? '<button type="button" class="btn btn-secondary btn-sm" data-act="close">დახურვა</button>'
              : '<button type="button" class="btn btn-success btn-sm" data-act="publish">გამოქვეყნება</button>'}
            <button type="button" class="btn btn-secondary btn-sm" data-act="edit">რედაქტირება</button>
            <button type="button" class="btn btn-ghost btn-sm" data-act="delete">წაშლა</button>
          </div>
        </div>
        ${stats ? `<div class="mt">${statsHtml(stats)}</div>` : ''}
      </div>

      ${missingQuizId ? `
        <div class="alert alert-warning mb">
          სერვერის პასუხში კითხვებს <code>quizId</code> არ აქვს. დაამატეთ ველი <code>QuizId</code>
          <code>QuestionResponseDTO</code>-ში და <code>QuestionService.ToDto</code>-ში.
        </div>` : ''}

      <div class="row-between mb">
        <h2 style="margin:0">კითხვები (${questions.length})</h2>
        ${locked ? '' : '<button type="button" class="btn btn-primary" data-act="q-add">+ კითხვა</button>'}
      </div>
      ${locked ? `<div class="alert alert-info mb">${esc(lockReason)}</div>` : ''}

      ${questions.length
        ? `<div class="stack">${questions.map((q, i) => questionHtml(q, i, locked)).join('')}</div>`
        : emptyState('❓', 'კითხვები ჯერ არ არის', locked ? '' : 'დააჭირეთ „+ კითხვა“ და დაამატეთ პირველი კითხვა.')}`;
  }

  async function reload() {
    await load();
    if (ctx.alive()) render();
  }

  render();

  ctx.main.addEventListener('click', async (e) => {
    const button = e.target.closest('[data-act]');
    if (!button) return;

    const action = button.dataset.act;
    const id = Number(button.dataset.id);

    try {
      if (action === 'publish' || action === 'close') {
        await runQuizAction(action, quiz, reload);
      } else if (action === 'delete') {
        await runQuizAction('delete', quiz, () => ctx.go('#/lecturer'));
      } else if (action === 'edit') {
        openQuizModal({ quiz, onSaved: reload });
      } else if (action === 'q-add') {
        openQuestionModal({ quizId, onSaved: reload });
      } else if (action === 'q-edit') {
        const question = questions.find((q) => q.id === id);
        openQuestionModal({ quizId, question, options: optionsByQuestion.get(id) || [], onSaved: reload });
      } else if (action === 'q-del') {
        const ok = await confirmDialog('წაიშლება კითხვა და მისი ყველა ვარიანტი. გსურთ გაგრძელება?', {
          title: 'კითხვის წაშლა', confirmText: 'წაშლა', danger: true
        });
        if (!ok) return;
        await api.del(`/api/Question/${id}`);
        toast('კითხვა წაიშალა.', 'success');
        await reload();
      } else if (action === 'opt-add') {
        openOptionModal({ questionId: id, onSaved: reload });
      } else if (action === 'opt-edit') {
        const questionId = Number(button.dataset.qid);
        const option = (optionsByQuestion.get(questionId) || []).find((o) => o.id === id);
        openOptionModal({ questionId, option, onSaved: reload });
      } else if (action === 'opt-del') {
        const ok = await confirmDialog('გსურთ ვარიანტის წაშლა?', {
          title: 'ვარიანტის წაშლა', confirmText: 'წაშლა', danger: true
        });
        if (!ok) return;
        await api.del(`/api/AnswerOption/${id}`);
        toast('ვარიანტი წაიშალა.', 'success');
        await reload();
      }
    } catch (err) {
      toast(errorMessage(err), 'error');
    }
  });
}
