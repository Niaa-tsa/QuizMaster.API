// API-სთან მუშაობა: ტოკენი, მოთხოვნები და შეცდომების გარჩევა (ProblemDetails)

const TOKEN_KEY = 'qm_token';
const USER_KEY = 'qm_user';

export const session = {
  get token() {
    return localStorage.getItem(TOKEN_KEY);
  },
  get user() {
    try {
      return JSON.parse(localStorage.getItem(USER_KEY));
    } catch {
      return null;
    }
  },
  save(auth) {
    localStorage.setItem(TOKEN_KEY, auth.token);
    localStorage.setItem(USER_KEY, JSON.stringify({
      userId: auth.userId,
      userName: auth.userName,
      email: auth.email,
      role: auth.role
    }));
  },
  clear() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }
};

let unauthorizedHandler = null;
export function setUnauthorizedHandler(fn) {
  unauthorizedHandler = fn;
}

const FALLBACK = {
  0: 'სერვერთან დაკავშირება ვერ მოხერხდა.',
  400: 'მონაცემები არასწორია.',
  401: 'ავტორიზაცია საჭიროა.',
  403: 'ამ მოქმედების შესრულების უფლება არ გაქვთ.',
  404: 'მონაცემი ვერ მოიძებნა.',
  409: 'მოქმედება ეწინააღმდეგება არსებულ მონაცემებს.'
};

function buildMessage(status, problem) {
  if (problem && typeof problem === 'object') {
    // ValidationProblemDetails: { errors: { Field: ["msg"] } }
    if (problem.errors && typeof problem.errors === 'object') {
      const messages = Object.values(problem.errors).flat().filter(Boolean);
      if (messages.length) return messages.join(' ');
    }
    if (problem.detail) return problem.detail;
    if (problem.title) return problem.title;
    if (problem.message) return problem.message;
  }
  if (status >= 500) return 'სერვერის შეცდომა. სცადეთ მოგვიანებით.';
  return FALLBACK[status] || `მოთხოვნა ვერ შესრულდა (${status}).`;
}

export class ApiError extends Error {
  constructor(status, problem) {
    super(buildMessage(status, problem));
    this.status = status;
    this.problem = problem;
  }
}

export function errorMessage(err) {
  return err && err.message ? err.message : 'დაფიქსირდა შეცდომა.';
}

async function request(method, url, { body, form } = {}) {
  const headers = {};

  if (session.token) {
    headers['Authorization'] = `Bearer ${session.token}`;
  }

  let payload;
  if (form) {
    payload = form; // multipart: Content-Type-ს ბრაუზერი თავად დააყენებს
  } else if (body !== undefined) {
    headers['Content-Type'] = 'application/json';
    payload = JSON.stringify(body);
  }

  let response;
  try {
    response = await fetch(url, { method, headers, body: payload });
  } catch {
    throw new ApiError(0, null);
  }

  if (response.status === 204) return null;

  const text = await response.text();
  let data = null;
  if (text) {
    try {
      data = JSON.parse(text);
    } catch {
      data = null;
    }
  }

  if (!response.ok) {
    // ტოკენი ჰქონდა და მაინც 401 → სესია ამოიწურა
    if (response.status === 401 && session.token) {
      session.clear();
      if (unauthorizedHandler) unauthorizedHandler();
    }
    throw new ApiError(response.status, data);
  }

  return data;
}

export const api = {
  get: (url) => request('GET', url),
  post: (url, body) => request('POST', url, { body }),
  put: (url, body) => request('PUT', url, { body }),
  patch: (url, body) => request('PATCH', url, { body }),
  del: (url) => request('DELETE', url),
  upload: (url, form) => request('POST', url, { form })
};
