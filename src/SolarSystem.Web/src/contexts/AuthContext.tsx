import { useState, useCallback, type ReactNode } from 'react';
import api from '../lib/api';
import { authStorage } from '../lib/authStorage';
import { AuthContext, type RegisterData, type User } from './auth-context';

function readStoredSession(): User | null {
  return authStorage.getAccessToken() ? authStorage.getUser() : null;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  // Sessao lida no inicializador do useState: nao ha efeito nem render intermediario
  // com o usuario deslogado antes do localStorage ser consultado.
  const [user, setUser] = useState<User | null>(readStoredSession);

  const login = useCallback(async (email: string, password: string) => {
    const { data } = await api.post('/auth/login', { email, password });
    authStorage.setSession(data.accessToken, data.refreshToken, data.user);
    setUser(data.user);
  }, []);

  const register = useCallback(async (payload: RegisterData) => {
    const { data } = await api.post('/auth/register', {
      companyName: payload.companyName,
      name: payload.adminName,
      email: payload.email,
      password: payload.password,
    });
    authStorage.setSession(data.accessToken, data.refreshToken, data.user);
    setUser(data.user);
  }, []);

  const logout = useCallback(async () => {
    try {
      // O backend revoga o refresh token; sem ele o logout seria so no cliente.
      await api.post('/auth/logout', { refreshToken: authStorage.getRefreshToken() ?? '' });
    } catch {
      // Falha no servidor nao pode impedir a limpeza local da sessao.
    }
    authStorage.clear();
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading: false,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}
