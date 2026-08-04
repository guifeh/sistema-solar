import { createContext } from 'react';

export interface User {
  id: string;
  email: string;
  name: string;
  role: string;
  tenantId: string;
  tenantName: string;
}

export interface RegisterData {
  companyName: string;
  adminName: string;
  email: string;
  password: string;
}

export interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  logout: () => Promise<void>;
}

// Contexto vive fora do .tsx do provider para nao quebrar o fast refresh,
// que exige que um arquivo de componente exporte apenas componentes.
export const AuthContext = createContext<AuthContextType | undefined>(undefined);
