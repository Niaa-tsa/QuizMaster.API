import { auth } from './auth.js';

export function renderNavigation(activePage = '') {
    const user = auth.getUser();
    const role = user ? user.role.toLowerCase() : 'student';

    const navContainer = document.getElementById('sidebar-nav');
    if (!navContainer) return;

    navContainer.innerHTML = `
        <div class="sidebar-brand">
            <h2>QuizMaster</h2>
            <span class="badge ${role}">${role.toUpperCase()}</span>
        </div>
        <div class="sidebar-footer">
            <p class="user-name">${user ? user.name : 'User'}</p>
            <button id="logout-btn" class="btn btn-full" style="background:#ef4444;">გასვლა</button>
        </div>
    `;

    document.getElementById('logout-btn')?.addEventListener('click', () => {
        auth.logout();
    });
}
