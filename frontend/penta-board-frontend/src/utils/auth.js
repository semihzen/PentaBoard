// src/utils/auth.js
export const TOKEN_KEY = 'pb_token';

export function saveToken(token, remember) {
  const target = remember ? window.localStorage : window.sessionStorage;
  target.setItem(TOKEN_KEY, token);
  // Diğer depoda aynı anahtar kalmasın
  (remember ? window.sessionStorage : window.localStorage).removeItem(TOKEN_KEY);
}

export function getToken() {
  return window.localStorage.getItem(TOKEN_KEY) || window.sessionStorage.getItem(TOKEN_KEY);
}

export function clearToken() {
  window.localStorage.removeItem(TOKEN_KEY);
  window.sessionStorage.removeItem(TOKEN_KEY);
}

// Ortak API_BASE (Login.js ile aynı şekilde çalışır)
export const API_BASE =
  (typeof import.meta !== 'undefined' && import.meta.env?.VITE_API_BASE_URL) ||
  process.env.REACT_APP_API_BASE_URL ||
  'http://localhost:5206';
