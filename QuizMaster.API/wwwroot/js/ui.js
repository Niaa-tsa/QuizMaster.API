// საერთო დამხმარე ფუნქციები: ესკეიპი, ტოსტები, მოდალები, თარიღები, ბეჯები

import { errorMessage } from './api.js';

// ---------- უსაფრთხოება ----------
export function esc(value) {
  return String(value ?? '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

// ---------- ტოსტი ----------
export function toast(message, type = 'info') {
  const root = document.getElementById('toasts');
  if (!root) return;
  const el = document.createElement('div');
  el.className = `toast toast-${type}`;
  el.textContent = message;
  root.appendChild(el);
  setTimeout(() => el.classList.add('hide'), 3800);
  setTimeout(() => el.remove(), 4300);
}

// ---------- მოდალი ----------
export function openModal({ title, html, wide = false, onClose = null }) {
  const root = document.getElementById('modal-root');
  const overlay = document.createElement('div');
  overlay.className = 'modal-overlay';
  overlay.innerHTML = `
    <div class="modal ${wide ? 'modal-wide' : ''}" role="dialog" aria-modal="true">
      <div class="modal-head">
        <h3>${esc(title)}</h3>
        <button type="button" class="icon-btn" data-close aria-label="დახურვა">✕</button>
      </div>
      <div class="modal-body">${html}</div>
    </div>`;
  root.appendChild(overlay);

  let closed = false;
  const onKey = (e) => {
    if (e.key === 'Escape') close();
  };
  function close() {
    if (closed) return;
    closed = true;
    overlay.remove();
    document.removeEventListener('keydown', onKey);
    if (onClose) onClose();
  }

  document.addEventListener('keydown', onKey);
  overlay.addEventListener('mousedown', (e) => {
    if (e.target === overlay) close();
  });
  overlay.querySelector('[data-close]').addEventListener('click', close);

  return { el: overlay.querySelector('.modal-body'), close };
}

export function confirmDialog(message, { title = 'დადასტურება', confirmText = 'დადასტურება', danger = false } = {}) {
  return new Promise((resolve) => {
    let decided = false;
    const decide = (value) => {
      if (decided) return;
      decided = true;
      resolve(value);
    };

    const modal = openModal({
      title,
      html: `
        <p>${esc(message)}</p>
        <div class="modal-actions">
          <button type="button" class="btn btn-ghost" data-no>გაუქმება</button>
          <button type="button" class="btn ${danger ? 'btn-danger' : 'btn-primary'}" data-yes>${esc(confirmText)}</button>
        </div>`,
      onClose: () => decide(false)
    });

    modal.el.querySelector('[data-yes]').addEventListener('click', () => {
      decide(true);
      modal.close();
    });
    modal.el.querySelector('[data-no]').addEventListener('click', () => modal.close());
  });
}

// ---------- თარიღები და რიცხვები ----------
// SQL Server-იდან DateTime ზოგჯერ მოდის "Z"-ს გარეშე; ის UTC-ა და ისე უნდა წავიკითხოთ
export function parseUtc(value) {
  if (!value) return null;
  let s = String(value).replace(/(\.\d{3})\d+/, '$1');
  if (!/([zZ]|[+-]\d{2}:?\d{2})$/.test(s)) s += 'Z';
  const d = new Date(s);
  return Number.isNaN(d.getTime()) ? null : d;
}

export function fmtDate(value) {
  const d = parseUtc(value);
  if (!d) return '—';
  try {
    return d.toLocaleString('ka-GE', { dateStyle: 'medium', timeStyle: 'short' });
  } catch {
    return d.toLocaleString();
  }
}

export function fmtNumber(n) {
  const x = Number(n);
  return Number.isFinite(x) ? String(Math.round(x * 100) / 100) : '—';
}

export function percentOf(score, max) {
  const s = Number(score);
  const m = Number(max);
  return m > 0 ? Math.round((s / m) * 10000) / 100 : 0;
}

export function fmtDuration(ms) {
  const total = Math.max(0, Math.floor(ms / 1000));
  const h = Math.floor(total / 3600);
  const m = Math.floor((total % 3600) / 60);
  const s = total % 60;
  const mm = String(m).padStart(2, '0');
  const ss = String(s).padStart(2, '0');
  return h > 0 ? `${h}:${mm}:${ss}` : `${mm}:${ss}`;
}

// ---------- ლეიბლები და ბეჯები ----------
export const QUESTION_TYPES = {
  1: 'ერთი სწორი პასუხი',
  2: 'რამდენიმე სწორი პასუხი',
  3: 'True / False'
};

const ROLE_LABELS = {
  Student: 'სტუდენტი',
  Lecturer: 'ლექტორი',
  Admin: 'ადმინი',
  SuperAdmin: 'სუპერ ადმინი'
};

export function badge(text, kind = 'muted') {
  return `<span class="badge badge-${kind}">${esc(text)}</span>`;
}

export function statusBadge(status) {
  if (status === 'InProgress') return badge('მიმდინარე', 'warning');
  if (status === 'Finished') return badge('დასრულებული', 'success');
  return badge(status || '—');
}

export function approvalBadge(status) {
  if (status === 'Approved') return badge('დადასტურებული', 'success');
  if (status === 'Pending') return badge('მოლოდინში', 'warning');
  if (status === 'Rejected') return badge('უარყოფილი', 'danger');
  return badge(status || '—');
}

export function roleBadge(role) {
  return badge(ROLE_LABELS[role] || role || '—', 'primary');
}

// ---------- ავატარი ----------
export function initials(name) {
  const parts = String(name || '?').trim().split(/[\s_.-]+/).filter(Boolean);
  const letters = parts.slice(0, 2).map((p) => p[0]).join('');
  return (letters || '?').toUpperCase();
}

export function avatarHtml(name, photoUrl, large = false) {
  const cls = large ? 'avatar avatar-lg' : 'avatar';
  if (photoUrl) {
    return `<img class="${cls}" src="${esc(photoUrl)}" alt="${esc(name)}">`;
  }
  return `<span class="${cls}">${esc(initials(name))}</span>`;
}

// ---------- გვერდის ბლოკები ----------
export const spinnerHtml = '<div class="spinner" role="status" aria-label="იტვირთება"></div>';

export function pageHeader(title, subtitle = '', actionsHtml = '') {
  return `
    <div class="page-head">
      <div>
        <h1>${esc(title)}</h1>
        ${subtitle ? `<p>${esc(subtitle)}</p>` : ''}
      </div>
      <div class="row">${actionsHtml}</div>
    </div>`;
}

export function emptyState(icon, title, text = '') {
  return `
    <div class="empty card">
      <div class="icon">${icon}</div>
      <h3>${esc(title)}</h3>
      ${text ? `<p>${esc(text)}</p>` : ''}
    </div>`;
}

export function renderError(container, err, onRetry = null) {
  container.innerHTML = `
    <div class="card center">
      <div class="alert alert-error mb">${esc(errorMessage(err))}</div>
      <div class="row" style="justify-content:center">
        ${onRetry ? '<button type="button" class="btn btn-primary" data-retry>ხელახლა ცდა</button>' : ''}
        <a class="btn btn-secondary" href="#/">მთავარი გვერდი</a>
      </div>
    </div>`;
  if (onRetry) {
    container.querySelector('[data-retry]').addEventListener('click', onRetry);
  }
}

// ---------- ფორმის დამხმარეები ----------
export function setBusy(button, busy, busyText = 'მიმდინარეობს...') {
  if (!button) return;
  if (busy) {
    button.dataset.label = button.textContent;
    button.textContent = busyText;
    button.disabled = true;
  } else {
    if (button.dataset.label) button.textContent = button.dataset.label;
    button.disabled = false;
  }
}

export function showAlert(el, message, kind = 'error') {
  if (!el) return;
  el.className = `alert alert-${kind}`;
  el.textContent = message;
  el.hidden = false;
}

export function hideAlert(el) {
  if (el) el.hidden = true;
}
