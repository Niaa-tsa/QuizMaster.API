// ადმინი: მომხმარებლები და ლექტორების დადასტურება; სუპერ ადმინი: ადმინების მართვა

import { api, errorMessage } from '../api.js';
import {
  esc, toast, pageHeader, emptyState, roleBadge, approvalBadge, badge,
  openModal, confirmDialog, setBusy, showAlert, hideAlert
} from '../ui.js';

// ======================= ADMIN =======================
export async function adminView(ctx) {
  let users = [];
  let pending = [];
  let tab = 'pending';
  let filter = '';

  async function load() {
    [users, pending] = await Promise.all([
      api.get('/api/Admin/users'),
      api.get('/api/Admin/pending-lecturers')
    ]);
  }

  await load();
  if (!ctx.alive()) return;

  ctx.main.innerHTML = `
    ${pageHeader('მომხმარებლების მართვა', 'ლექტორების დადასტურება და როლების შეცვლა.')}
    <div class="tabs">
      <button type="button" class="tab" data-tab="pending"></button>
      <button type="button" class="tab" data-tab="users"></button>
    </div>
    <div id="tab-content"></div>`;

  const content = ctx.main.querySelector('#tab-content');

  function renderTabs() {
    const pendingTab = ctx.main.querySelector('[data-tab="pending"]');
    const usersTab = ctx.main.querySelector('[data-tab="users"]');
    pendingTab.textContent = `მოლოდინში (${pending.length})`;
    usersTab.textContent = `ყველა მომხმარებელი (${users.length})`;
    pendingTab.classList.toggle('active', tab === 'pending');
    usersTab.classList.toggle('active', tab === 'users');
  }

  function pendingHtml() {
    if (!pending.length) {
      return emptyState('✅', 'დასადასტურებელი მოთხოვნები არ არის');
    }
    return `
      <div class="card table-wrap">
        <table>
          <thead><tr><th>სახელი</th><th>ელფოსტა</th><th>მოთხოვნილი როლი</th><th></th></tr></thead>
          <tbody>
            ${pending.map((u) => `
              <tr>
                <td><strong>${esc(u.userName)}</strong></td>
                <td>${esc(u.email)}</td>
                <td>${roleBadge(u.requestedRole || 'Lecturer')}</td>
                <td class="right">
                  <button type="button" class="btn btn-success btn-sm" data-approve="${u.id}">დადასტურება</button>
                  <button type="button" class="btn btn-danger btn-sm" data-reject="${u.id}">უარყოფა</button>
                </td>
              </tr>`).join('')}
          </tbody>
        </table>
      </div>`;
  }

  function usersTableHtml() {
    const term = filter.trim().toLowerCase();
    const list = users.filter((u) =>
      !term || u.userName.toLowerCase().includes(term) || u.email.toLowerCase().includes(term));

    if (!list.length) return emptyState('🔍', 'მომხმარებელი ვერ მოიძებნა');

    return `
      <div class="card table-wrap">
        <table>
          <thead><tr><th>#</th><th>სახელი</th><th>ელფოსტა</th><th>როლი</th><th>სტატუსი</th><th>მოთხოვნილი</th><th>როლის შეცვლა</th></tr></thead>
          <tbody>
            ${list.map((u) => {
              const changeable = u.role === 'Student' || u.role === 'Lecturer';
              return `
                <tr>
                  <td class="muted">${esc(u.id)}</td>
                  <td><strong>${esc(u.userName)}</strong></td>
                  <td>${esc(u.email)}</td>
                  <td>${roleBadge(u.role)}</td>
                  <td>${approvalBadge(u.approvalStatus)}</td>
                  <td>${u.requestedRole ? roleBadge(u.requestedRole) : '—'}</td>
                  <td>
                    ${changeable ? `
                      <select data-role-user="${u.id}" data-current="${esc(u.role)}" style="width:auto">
                        <option value="Student" ${u.role === 'Student' ? 'selected' : ''}>სტუდენტი</option>
                        <option value="Lecturer" ${u.role === 'Lecturer' ? 'selected' : ''}>ლექტორი</option>
                      </select>` : '<span class="muted small">შეუცვლელი</span>'}
                  </td>
                </tr>`;
            }).join('')}
          </tbody>
        </table>
      </div>`;
  }

  function renderContent() {
    renderTabs();
    if (tab === 'pending') {
      content.innerHTML = pendingHtml();
    } else {
      content.innerHTML = `
        <div class="field mb">
          <input class="input" id="user-search" placeholder="ძებნა სახელით ან ელფოსტით..." value="${esc(filter)}">
        </div>
        <div id="users-table">${usersTableHtml()}</div>`;
    }
  }

  async function reload() {
    await load();
    if (ctx.alive()) renderContent();
  }

  renderContent();

  ctx.main.addEventListener('input', (e) => {
    if (e.target.id !== 'user-search') return;
    filter = e.target.value;
    ctx.main.querySelector('#users-table').innerHTML = usersTableHtml();
  });

  ctx.main.addEventListener('change', async (e) => {
    const select = e.target.closest('[data-role-user]');
    if (!select) return;

    const user = users.find((u) => u.id === Number(select.dataset.roleUser));
    const newRole = select.value;
    if (!user || newRole === user.role) return;

    const ok = await confirmDialog(
      `${user.userName}-ის როლი შეიცვლება: ${newRole === 'Lecturer' ? 'ლექტორი' : 'სტუდენტი'}. გსურთ გაგრძელება?`,
      { title: 'როლის შეცვლა', confirmText: 'შეცვლა' }
    );
    if (!ok) {
      select.value = user.role;
      return;
    }

    try {
      await api.put(`/api/Admin/change-role/${user.id}`, { role: newRole });
      toast('როლი შეიცვალა.', 'success');
      await reload();
    } catch (err) {
      toast(errorMessage(err), 'error');
      select.value = user.role;
    }
  });

  ctx.main.addEventListener('click', async (e) => {
    const tabButton = e.target.closest('[data-tab]');
    if (tabButton) {
      tab = tabButton.dataset.tab;
      renderContent();
      return;
    }

    const approve = e.target.closest('[data-approve]');
    const reject = e.target.closest('[data-reject]');
    if (!approve && !reject) return;

    const button = approve || reject;
    const userId = Number(button.dataset.approve || button.dataset.reject);

    if (reject) {
      const ok = await confirmDialog('გსურთ ლექტორის მოთხოვნის უარყოფა?', {
        title: 'მოთხოვნის უარყოფა', confirmText: 'უარყოფა', danger: true
      });
      if (!ok) return;
    }

    setBusy(button, true, '...');
    try {
      await api.post(`/api/Admin/${approve ? 'approve' : 'reject'}-lecturer/${userId}`);
      toast(approve ? 'ლექტორი დადასტურდა.' : 'მოთხოვნა უარყოფილია.', 'success');
      await reload();
    } catch (err) {
      toast(errorMessage(err), 'error');
      setBusy(button, false);
    }
  });
}

// ======================= SUPER ADMIN =======================
function openCreateAdminModal(onSaved) {
  const modal = openModal({
    title: 'ახალი ადმინისტრატორი',
    html: `
      <form class="form" id="admin-form" novalidate>
        <div class="field">
          <label for="ad-name">მომხმარებლის სახელი</label>
          <input class="input" id="ad-name" maxlength="100" autocomplete="off">
        </div>
        <div class="field">
          <label for="ad-email">ელფოსტა</label>
          <input class="input" id="ad-email" type="email" autocomplete="off">
        </div>
        <div class="field">
          <label for="ad-pass">პაროლი</label>
          <input class="input" id="ad-pass" type="password" autocomplete="new-password">
          <p class="hint">მინიმუმ 8 სიმბოლო.</p>
        </div>
        <div class="alert alert-error" id="ad-error" hidden></div>
        <div class="modal-actions">
          <button type="button" class="btn btn-ghost" data-cancel>გაუქმება</button>
          <button type="submit" class="btn btn-primary">შექმნა</button>
        </div>
      </form>`
  });

  const form = modal.el.querySelector('#admin-form');
  const errorBox = modal.el.querySelector('#ad-error');
  const submit = form.querySelector('button[type="submit"]');
  modal.el.querySelector('[data-cancel]').addEventListener('click', modal.close);

  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    hideAlert(errorBox);

    const userName = form.querySelector('#ad-name').value.trim();
    const email = form.querySelector('#ad-email').value.trim();
    const password = form.querySelector('#ad-pass').value;

    if (userName.length < 3) return showAlert(errorBox, 'სახელი მინიმუმ 3 სიმბოლო უნდა იყოს.');
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return showAlert(errorBox, 'შეიყვანეთ სწორი ელფოსტა.');
    if (password.length < 8) return showAlert(errorBox, 'პაროლი მინიმუმ 8 სიმბოლო უნდა იყოს.');

    setBusy(submit, true, 'იქმნება...');
    try {
      await api.post('/api/SuperAdmin/create-admin', { userName, email, password });
      toast('ადმინისტრატორი შეიქმნა.', 'success');
      modal.close();
      await onSaved();
    } catch (err) {
      showAlert(errorBox, errorMessage(err));
      setBusy(submit, false);
    }
  });
}

export async function superAdminView(ctx) {
  let admins = await api.get('/api/SuperAdmin/admins');
  if (!ctx.alive()) return;

  function render() {
    ctx.main.innerHTML = `
      ${pageHeader('ადმინისტრატორები', 'სისტემის ადმინისტრატორების მართვა.',
        '<button type="button" class="btn btn-primary" data-create>+ ახალი ადმინი</button>')}
      ${admins.length ? `
        <div class="card table-wrap">
          <table>
            <thead><tr><th>#</th><th>სახელი</th><th>ელფოსტა</th><th>სტატუსი</th><th></th></tr></thead>
            <tbody>
              ${admins.map((a) => `
                <tr>
                  <td class="muted">${esc(a.id)}</td>
                  <td><strong>${esc(a.userName)}</strong></td>
                  <td>${esc(a.email)}</td>
                  <td>${a.approvalStatus ? approvalBadge(a.approvalStatus) : badge('აქტიური', 'success')}</td>
                  <td class="right">
                    <button type="button" class="btn btn-secondary btn-sm" data-view="${a.id}">დეტალები</button>
                    <button type="button" class="btn btn-ghost btn-sm" data-delete="${a.id}">წაშლა</button>
                  </td>
                </tr>`).join('')}
            </tbody>
          </table>
        </div>`
        : emptyState('🛡️', 'ადმინისტრატორები ჯერ არ არის', 'დააჭირეთ „+ ახალი ადმინი“.')}`;
  }

  async function reload() {
    admins = await api.get('/api/SuperAdmin/admins');
    if (ctx.alive()) render();
  }

  render();

  ctx.main.addEventListener('click', async (e) => {
    if (e.target.closest('[data-create]')) {
      openCreateAdminModal(reload);
      return;
    }

    const view = e.target.closest('[data-view]');
    if (view) {
      try {
        const admin = await api.get(`/api/SuperAdmin/admins/${view.dataset.view}`);
        openModal({
          title: 'ადმინისტრატორის დეტალები',
          html: `
            <dl style="display:grid;grid-template-columns:auto 1fr;gap:.5rem 1rem;margin:0">
              <dt class="muted">ID</dt><dd style="margin:0">${esc(admin.id)}</dd>
              <dt class="muted">სახელი</dt><dd style="margin:0">${esc(admin.userName)}</dd>
              <dt class="muted">ელფოსტა</dt><dd style="margin:0">${esc(admin.email)}</dd>
              <dt class="muted">როლი</dt><dd style="margin:0">${roleBadge(admin.role)}</dd>
            </dl>`
        });
      } catch (err) {
        toast(errorMessage(err), 'error');
      }
      return;
    }

    const del = e.target.closest('[data-delete]');
    if (del) {
      const admin = admins.find((a) => a.id === Number(del.dataset.delete));
      const ok = await confirmDialog(`გსურთ ადმინისტრატორის „${admin ? admin.userName : ''}“ წაშლა?`, {
        title: 'ადმინისტრატორის წაშლა', confirmText: 'წაშლა', danger: true
      });
      if (!ok) return;

      try {
        await api.del(`/api/SuperAdmin/admins/${del.dataset.delete}`);
        toast('ადმინისტრატორი წაიშალა.', 'success');
        await reload();
      } catch (err) {
        toast(errorMessage(err), 'error');
      }
    }
  });
}
