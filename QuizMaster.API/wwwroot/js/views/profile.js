
// პროფილი, პროფილის ფოტოს მართვა და პაროლის შეცვლა

import { api, errorMessage, session } from '../api.js';
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
      <div class="page-head">
        <div><h1>ჩემი პროფილი</h1></div>
      </div>

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
          <input class="input" type="file" id="photo-input"
            accept="image/jpeg,image/png,image/webp">
          <div id="preview"></div>
        </div>

        <div class="alert alert-error mt" id="photo-error" hidden></div>

        <div class="row mt">
          <button type="button" class="btn btn-primary" id="upload-btn" disabled>
            ატვირთვა
          </button>
          ${profile.profilePhotoUrl
                ? '<button type="button" class="btn btn-danger" id="delete-btn">ფოტოს წაშლა</button>'
                : ''}
        </div>

        <hr style="border:none;border-top:1px solid var(--border);margin:1.5rem 0">

        <h3>პაროლის შეცვლა</h3>
        <p class="hint mb">
          ახალი პაროლი უნდა შეიცავდეს მინიმუმ 8 სიმბოლოს.
          პაროლის შეცვლის შემდეგ ხელახლა შესვლა მოგიწევს.
        </p>

        <form id="password-form">
          <div class="field">
            <label for="current-password">მიმდინარე პაროლი</label>
            <input class="input" type="password" id="current-password"
              autocomplete="current-password" required>
          </div>

          <div class="field">
            <label for="new-password">ახალი პაროლი</label>
            <input class="input" type="password" id="new-password"
              autocomplete="new-password" minlength="8" required>
          </div>

          <div class="field">
            <label for="confirm-password">გაიმეორე ახალი პაროლი</label>
            <input class="input" type="password" id="confirm-password"
              autocomplete="new-password" minlength="8" required>
          </div>

          <div class="alert alert-error mt" id="password-error" hidden></div>

          <button type="submit" class="btn btn-primary mt" id="password-btn">
            პაროლის შეცვლა
          </button>
        </form>
      </div>`;

        // ფოტოს ელემენტები
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

        // პაროლის შეცვლა
        const passwordForm = ctx.main.querySelector('#password-form');
        const currentPassword = ctx.main.querySelector('#current-password');
        const newPassword = ctx.main.querySelector('#new-password');
        const confirmPassword = ctx.main.querySelector('#confirm-password');
        const passwordError = ctx.main.querySelector('#password-error');
        const passwordBtn = ctx.main.querySelector('#password-btn');

        passwordForm.addEventListener('submit', async (event) => {
            event.preventDefault();
            hideAlert(passwordError);

            if (newPassword.value.length < 8) {
                showAlert(passwordError, 'ახალი პაროლი მინიმუმ 8 სიმბოლოს უნდა შეიცავდეს.');
                return;
            }

            if (newPassword.value !== confirmPassword.value) {
                showAlert(passwordError, 'ახალი პაროლები ერთმანეთს არ ემთხვევა.');
                return;
            }

            if (currentPassword.value === newPassword.value) {
                showAlert(passwordError, 'ახალი პაროლი მიმდინარე პაროლისგან განსხვავებული უნდა იყოს.');
                return;
            }

            setBusy(passwordBtn, true, 'იცვლება...');

            try {
                await api.post('/api/Auth/change-password', {
                    currentPassword: currentPassword.value,
                    newPassword: newPassword.value
                });

                toast('პაროლი წარმატებით შეიცვალა. ხელახლა შედით სისტემაში.', 'success');

                // ძველი JWT უკვე გაუქმებულია
                session.clear();

                // გვერდის განახლებისას აპლიკაციამ Login გვერდი უნდა აჩვენოს
                setTimeout(() => {
                    window.location.reload();
                }, 800);

            } catch (err) {
                showAlert(passwordError, errorMessage(err));
                setBusy(passwordBtn, false);
            }
        });
    }

    render();
    console.log('PROFILE JS LOADED');
    console.log(document.querySelector('#password-form'));
}
