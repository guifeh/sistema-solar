import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL?.endsWith('/api') ? import.meta.env.VITE_API_URL : `${import.meta.env.VITE_API_URL || 'http://localhost:5268'}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor — add JWT token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('access_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor — handle 401 with refresh
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = localStorage.getItem('refresh_token');
        if (!refreshToken) {
          throw new Error('No refresh token');
        }

        const response = await axios.post(
          `${import.meta.env.VITE_API_URL?.endsWith('/api') ? import.meta.env.VITE_API_URL : `${import.meta.env.VITE_API_URL || 'http://localhost:5268'}/api`}/auth/refresh`,
          { refreshToken }
        );

        const { accessToken, refreshToken: newRefreshToken } = response.data;
        localStorage.setItem('access_token', accessToken);
        localStorage.setItem('refresh_token', newRefreshToken);

        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return api(originalRequest);
      } catch {
        localStorage.removeItem('access_token');
        localStorage.removeItem('refresh_token');
        localStorage.removeItem('user');
        window.location.href = '/login';
        return Promise.reject(error);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
