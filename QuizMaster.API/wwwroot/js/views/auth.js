// შესვლა და რეგისტრაცია

import { api, session, errorMessage } from '../api.js';
import { toast, setBusy, showAlert, hideAlert } from '../ui.js';

export function loginView(ctx) {
  ctx.main.innerHTML = `
    <section class="auth-card card">
      <h2>შესვლა</h2>
      <p class="muted">შეიყვანეთ თქვენი მონაცემები სისტემაში შესასვლელად.</p>
      <form class="form" id="login-form" novalidate>
        <div class="field">
          <label for="email">ელფოსტა</label>
          <input class="input" id="email" type="email" autocomplete="email" required>
        </div>
        <div class="field">
          <label for="password">პაროლი</label>
          <input class="input" id="password" type="password" autocomplete="current-password" required>
        </div>
        <div class="alert alert-error" id="login-error" hidden></div>
        <button class="btn btn-primary btn-block" type="submit">შესვლა</button>
      </form>
      <p class="center muted mt">ანგარიში არ გაქვთ? <a href="#/register">რეგისტრაცია</a></p>
    </section>`;

  const form = ctx.main.querySelector('#login-form');
  const errorBox = ctx.main.querySelector('#login-error');
  const button = form.querySelector('button[type="submit"]');

  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    hideAlert(errorBox);

    const email = form.querySelector('#email').value.trim();
    const password = form.querySelector('#password').value;

    if (!email || !password) {
      showAlert(errorBox, 'შეიყვანეთ ელფოსტა და პაროლი.');
      return;
    }

    setBusy(button, true, 'შესვლა...');
    try {
      const auth = await api.post('/api/Auth/login', { email, password });
      session.save(auth);
      await ctx.refreshProfile();
      toast(`მოგესალმებით, ${auth.userName}!`, 'success');
      ctx.goHome();
    } catch (err) {
      showAlert(errorBox, errorMessage(err));
      setBusy(button, false);
    }
  });
}

export function registerView(ctx) {
  ctx.main.innerHTML = `
    <section class="auth-card card">
      <h2>რეგისტრაცია</h2>
      <p class="muted">შექმენით ახალი ანგარიში.</p>
      <form class="form" id="register-form" novalidate>
        <div class="field">
          <label for="userName">მომხმარებლის სახელი</label>
          <input class="input" id="userName" type="text" minlength="3" maxlength="100" autocomplete="username" required>
        </div>
        <div class="field">
          <label for="email">ელფოსტა</label>
          <input class="input" id="email" type="email" autocomplete="email" required>
        </div>
        <div class="field">
          <label for="password">პაროლი</label>
          <input class="input" id="password" type="password" minlength="8" autocomplete="new-password" required>
          <p class="hint">მინიმუმ 8 სიმბოლო.</p>
        </div>
        <div class="field">
          <label>რა როლით რეგისტრირდებით?</label>
          <div class="role-choice">
            <label>
              <input type="radio" name="role" value="Student" checked>
              <span>სტუდენტი<small>ტესტების გავლა და შედეგები</small></span>
            </label>
            <label>
              <input type="radio" name="role" value="Lecturer">
              <span>ლექტორი<small>საჭიროებს ადმინის დადასტურებას</small></span>
            </label>
          </div>
        </div>
        <div class="alert alert-error" id="register-error" hidden></div>
        <button class="btn btn-primary btn-block" type="submit">რეგისტრაცია</button>
      </form>
      <p class="center muted mt">უკვე გაქვთ ანგარიში? <a href="#/login">შესვლა</a></p>
    </section>`;

  const form = ctx.main.querySelector('#register-form');
  const errorBox = ctx.main.querySelector('#register-error');
  const button = form.querySelector('button[type="submit"]');

  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    hideAlert(errorBox);

    const userName = form.querySelector('#userName').value.trim();
    const email = form.querySelector('#email').value.trim();
    const password = form.querySelector('#password').value;
    const requestedRole = form.querySelector('input[name="role"]:checked').value;

    if (userName.length < 3) {
      showAlert(errorBox, 'მომხმარებლის სახელი მინიმუმ 3 სიმბოლოს უნდა შეიცავდეს.');
      return;
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      showAlert(errorBox, 'შეიყვანეთ სწორი ელფოსტა.');
      return;
    }
    if (password.length < 8) {
      showAlert(errorBox, 'პაროლი მინიმუმ 8 სიმბოლოს უნდა შეიცავდეს.');
      return;
    }

    setBusy(button, true, 'რეგისტრაცია...');
    try {
      const result = await api.post('/api/Auth/register', {
        userName,
        email,
        password,
        requestedRole
      });

      if (result && result.token) {
        session.save(result);
        await ctx.refreshProfile();
        toast('რეგისტრაცია წარმატებით დასრულდა!', 'success');
        ctx.goHome();
        return;
      }

      toast(
        requestedRole === 'Lecturer'
          ? 'ლექტორის მოთხოვნა გაიგზავნა. ადმინის დადასტურების შემდეგ შეძლებთ შესვლას.'
          : 'რეგისტრაცია დასრულდა. ახლა შეგიძლიათ შესვლა.',
        'success'
      );
      ctx.go('#/login');
    } catch (err) {
      showAlert(errorBox, errorMessage(err));
      setBusy(button, false);
    }
  });
}
