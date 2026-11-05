import { api } from './api.js';
import { CONFIG } from './config.js';

export const auth = {
    async login(email, password) {
        const response = await api.post('/Auth/login', { email, password });
        const token = response.token || response.accessToken || (typeof response === 'string' ? response : null);
        
        if (!token) throw new Error('ვერ მოიძებნა ავტორიზაციის ტოკენი პასუხში.');
        
        localStorage.setItem(CONFIG.TOKEN_KEY, token);
        const userInfo = this.decodeToken(token);
        localStorage.setItem(CONFIG.USER_KEY, JSON.stringify(userInfo));
        return userInfo;
    },

    logout() {
        localStorage.removeItem(CONFIG.TOKEN_KEY);
        localStorage.removeItem(CONFIG.USER_KEY);
        window.location.href = '/pages/login.html';
    },

    getToken() {
        return localStorage.getItem(CONFIG.TOKEN_KEY);
    },

    getUser() {
        const userStr = localStorage.getItem(CONFIG.USER_KEY);
        return userStr ? JSON.parse(userStr) : null;
    },

    isAuthenticated() {
        return !!this.getToken();
    },

    decodeToken(token) {
        try {
            const base64Url = token.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(atob(base64).split('').map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join(''));
            const payload = JSON.parse(jsonPayload);
            
            const roleKey = Object.keys(payload).find(k => k.endsWith('/role') || k === 'role' || k === 'roles');
            const role = roleKey ? payload[roleKey] : 'Student';

            return {
                email: payload.email || '',
                name: payload.unique_name || payload.name || 'User',
                role: Array.isArray(role) ? role[0] : role
            };
        } catch (e) {
            return { role: 'Student' };
        }
    }
};
