import type { User } from '../contexts/auth-context';

const ACCESS_TOKEN = 'access_token';
const REFRESH_TOKEN = 'refresh_token';
const USER = 'user';

export const authStorage = {
  getAccessToken: () => localStorage.getItem(ACCESS_TOKEN),
  getRefreshToken: () => localStorage.getItem(REFRESH_TOKEN),

  getUser(): User | null {
    const raw = localStorage.getItem(USER);
    if (!raw) return null;

    try {
      return JSON.parse(raw) as User;
    } catch {
      // JSON corrompido: descarta a sessao inteira em vez de deixar um estado meio valido.
      authStorage.clear();
      return null;
    }
  },

  setSession(accessToken: string, refreshToken: string, user?: User) {
    localStorage.setItem(ACCESS_TOKEN, accessToken);
    localStorage.setItem(REFRESH_TOKEN, refreshToken);
    if (user) localStorage.setItem(USER, JSON.stringify(user));
  },

  clear() {
    localStorage.removeItem(ACCESS_TOKEN);
    localStorage.removeItem(REFRESH_TOKEN);
    localStorage.removeItem(USER);
  },
};
