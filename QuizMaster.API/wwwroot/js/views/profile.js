// პროფილი და პროფილის ფოტოს ატვირთვა/წაშლა

import { api, errorMessage } from '../api.js';
import { esc, toast, avatarHtml, roleBadge, confirmDialog, setBusy, showAlert, hideAlert } from '../ui.js';

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
const MAX_BYTES = 2 * 1024 * 1024;

function validateFile(file) {
  if (!ALLOWED_TYPES.includes(file.type)) {
    return 'დასაშვებია მხოლოდ JPEG, PNG ან WebP ფორმატი.';
  }
  if (file.size > MAX_BYTES) {
    return 'ფოტო 2 MB-ზე დიდი ვერ იქნება.';
  }
  return null;
}

export async function profileView(ctx) {
  let profile = await api.get('/api/Profile/me');
  if (!ctx.alive()) return;

  function render() {
    ctx.main.innerHTML = `
      <div class="page-head"><div><h1>ჩემი პროფილი</h1></div></div>
      <div class="card" style="max-width:640px">
        <div class="row" style="gap:1.5rem;align-items:flex-start">
          ${avatarHtml(profile.userName, profile.profilePhotoUrl, true)}
          <div>
            <h2>${esc(profile.userName)}</h2>
            <p class="muted">${esc(profile.email)}</p>
            ${roleBadge(profile.role)}
          </div>
        </div>

        <hr style="border:none;border-top:1px solid var(--border);margin:1.5rem 0">

        <h3>პროფილის ფოტო</h3>
        <p class="hint mb">JPEG, PNG ან WebP, მაქსიმუმ 2 MB.</p>
        <div class="field">
          <input class="input" type="file" id="photo-input" accept="image/jpeg,image/png,image/webp">
          <div id="preview"></div>
        </div>
        <div class="alert alert-error mt" id="photo-error" hidden></div>
        <div class="row mt">
          <button type="button" class="btn btn-primary" id="upload-btn" disabled>ატვირთვა</button>
          ${profile.profilePhotoUrl
            ? '<button type="button" class="btn btn-danger" id="delete-btn">ფოტოს წაშლა</button>'
            : ''}
        </div>
      </div>`;

    const input = ctx.main.querySelector('#photo-input');
    const preview = ctx.main.querySelector('#preview');
    const errorBox = ctx.main.querySelector('#photo-error');
    const uploadBtn = ctx.main.querySelector('#upload-btn');
    const deleteBtn = ctx.main.querySelector('#delete-btn');

    input.addEventListener('change', () => {
      hideAlert(errorBox);
      preview.innerHTML = '';
      uploadBtn.disabled = true;

      const file = input.files[0];
      if (!file) return;

      const problem = validateFile(file);
      if (problem) {
        showAlert(errorBox, problem);
        input.value = '';
        return;
      }

      const img = document.createElement('img');
      img.className = 'photo-preview';
      img.alt = 'არჩეული ფოტო';
      img.src = URL.createObjectURL(file);
      preview.appendChild(img);
      uploadBtn.disabled = false;
    });

    uploadBtn.addEventListener('click', async () => {
      const file = input.files[0];
      if (!file) return;

      hideAlert(errorBox);
      setBusy(uploadBtn, true, 'იტვირთება...');
      try {
        const form = new FormData();
        form.append('file', file);
        profile = await api.upload('/api/Profile/photo', form);
        toast('ფოტო წარმატებით აიტვირთა.', 'success');
        await ctx.refreshProfile();
        if (ctx.alive()) render();
      } catch (err) {
        showAlert(errorBox, errorMessage(err));
        setBusy(uploadBtn, false);
      }
    });

    if (deleteBtn) {
      deleteBtn.addEventListener('click', async () => {
        const ok = await confirmDialog('გსურთ პროფილის ფოტოს წაშლა?', {
          title: 'ფოტოს წაშლა',
          confirmText: 'წაშლა',
          danger: true
        });
        if (!ok) return;

        try {
          await api.del('/api/Profile/photo');
          profile = { ...profile, profilePhotoUrl: null };
          toast('ფოტო წაიშალა.', 'success');
          await ctx.refreshProfile();
          if (ctx.alive()) render();
        } catch (err) {
          toast(errorMessage(err), 'error');
        }
      });
    }
  }

  render();
}
