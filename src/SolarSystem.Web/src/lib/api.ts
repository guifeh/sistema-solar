import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { authStorage } from './authStorage';

const rawBaseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5268';
const baseURL = rawBaseUrl.endsWith('/api') ? rawBaseUrl : `${rawBaseUrl}/api`;

const api = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
});

type RetriableRequest = InternalAxiosRequestConfig & { _retry?: boolean };

// O backend rotaciona o refresh token a cada uso: o antigo é revogado na hora. Se dois
// requests tomarem 401 juntos e cada um disparar seu próprio refresh, o segundo usa um
// token já morto e derruba a sessão. Por isso o refresh é single-flight — todos os
// requests em espera compartilham a mesma promise.
let refreshInFlight: Promise<string> | null = null;

function refreshAccessToken(): Promise<string> {
  if (refreshInFlight) return refreshInFlight;

  refreshInFlight = (async () => {
    const refreshToken = authStorage.getRefreshToken();
    if (!refreshToken) throw new Error('Sem refresh token.');

    // Cliente separado: usar `api` aqui reentraria no próprio interceptor.
    const { data } = await axios.post(`${baseURL}/auth/refresh`, { refreshToken });
    authStorage.setSession(data.accessToken, data.refreshToken, data.user);
    return data.accessToken as string;
  })();

  refreshInFlight.finally(() => {
    refreshInFlight = null;
  });

  return refreshInFlight;
}

api.interceptors.request.use(
  (config) => {
    const token = authStorage.getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetriableRequest | undefined;

    const isAuthEndpoint = originalRequest?.url?.includes('/auth/');
    if (error.response?.status !== 401 || !originalRequest || originalRequest._retry || isAuthEndpoint) {
      return Promise.reject(error);
    }

    originalRequest._retry = true;

    try {
      const accessToken = await refreshAccessToken();
      originalRequest.headers.Authorization = `Bearer ${accessToken}`;
      return api(originalRequest);
    } catch {
      authStorage.clear();
      window.location.href = '/login';
      return Promise.reject(error);
    }
  }
);

export default api;
