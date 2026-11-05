import { CONFIG } from './config.js';

export class ApiError extends Error {
    constructor(status, message, data = null) {
        super(message);
        this.status = status;
        this.data = data;
    }
}

async function request(endpoint, options = {}) {
    const url = `${CONFIG.API_BASE_URL}${endpoint}`;
    const token = localStorage.getItem(CONFIG.TOKEN_KEY);

    const headers = {
        'Content-Type': 'application/json',
        ...(options.headers || {})
    };

    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(url, { ...options, headers });

    if (response.status === 401) {
        localStorage.removeItem(CONFIG.TOKEN_KEY);
        localStorage.removeItem(CONFIG.USER_KEY);
        window.location.href = '/pages/login.html';
        throw new ApiError(401, 'სესია ამოიწურა. გთხოვთ თავიდან გაიაროთ ავტორიზაცია.');
    }

    if (response.status === 204) return null;

    let data = null;
    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
        data = await response.json();
    } else {
        data = await response.text();
    }

    if (!response.ok) {
        throw new ApiError(response.status, data.message || data.title || 'მოთხოვნა ვერ შესრულდა', data);
    }

    return data;
}

export const api = {
    get: (endpoint) => request(endpoint, { method: 'GET' }),
    post: (endpoint, body) => request(endpoint, { method: 'POST', body: JSON.stringify(body) }),
    put: (endpoint, body) => request(endpoint, { method: 'PUT', body: JSON.stringify(body) }),
    delete: (endpoint) => request(endpoint, { method: 'DELETE' })
};
