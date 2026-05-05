import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Sun, Mail, Lock, ArrowRight } from 'lucide-react';
import { useAuth } from '../../contexts/AuthContext';
import { Button, Input } from '../../components/ui';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      await login(email, password);
      navigate('/dashboard');
    } catch (err: unknown) {
      const axiosError = err as { response?: { data?: { message?: string } } };
      setError(axiosError.response?.data?.message || 'E-mail ou senha inválidos.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex bg-surface-950">
      {/* Left panel — branding */}
      <div className="hidden lg:flex lg:w-1/2 relative overflow-hidden">
        <div className="absolute inset-0 gradient-solar opacity-10" />
        <div className="absolute inset-0 bg-gradient-to-br from-surface-950/80 via-surface-950/60 to-transparent" />
        <div className="relative z-10 flex flex-col justify-center w-full px-12 lg:px-24 xl:px-32 2xl:px-40">
          <div className="flex items-center gap-4 mb-10">
            <div className="w-14 h-14 rounded-2xl gradient-solar flex items-center justify-center shadow-xl animate-pulse-solar">
              <Sun className="w-8 h-8 text-surface-900" />
            </div>
            <h1 className="text-4xl font-bold text-surface-100">Sistema Solar</h1>
          </div>
          <h2 className="text-5xl xl:text-6xl font-bold text-surface-100 leading-tight mb-8">
            Gerencie seus
            <br />
            <span className="text-solar-400">projetos solares</span>
            <br />
            com inteligência.
          </h2>
          <p className="text-xl text-surface-400 max-w-lg leading-relaxed">
            Dimensionamento, precificação, propostas e acompanhamento financeiro — tudo em um só lugar.
          </p>

          {/* Decorative elements */}
          <div className="absolute bottom-16 left-12 lg:left-24 xl:left-32 flex gap-4">
            <div className="w-3 h-3 rounded-full bg-solar-500/60" />
            <div className="w-3 h-3 rounded-full bg-solar-500/40" />
            <div className="w-3 h-3 rounded-full bg-solar-500/20" />
          </div>
        </div>
      </div>

      {/* Right panel — login form */}
      <div className="flex-1 flex items-center justify-center p-8 lg:p-16 xl:p-24">
        <div className="w-full max-w-xl animate-fade-in">
          {/* Mobile logo */}
          <div className="flex items-center gap-3 mb-8 lg:hidden">
            <div className="w-12 h-12 rounded-xl gradient-solar flex items-center justify-center shadow-lg">
              <Sun className="w-7 h-7 text-surface-900" />
            </div>
            <h1 className="text-2xl font-bold text-surface-100">Sistema Solar</h1>
          </div>

          <div className="mb-10">
            <h2 className="text-4xl font-bold text-surface-100 mb-3 tracking-tight">Bem-vindo de volta</h2>
            <p className="text-xl text-surface-400">Entre na sua conta para continuar</p>
          </div>

          <form onSubmit={handleSubmit} className="space-y-8">
            {error && (
              <div className="p-3 rounded-xl bg-red-500/10 border border-red-500/20 text-sm text-red-400 animate-fade-in">
                {error}
              </div>
            )}

            <Input
              label="E-mail"
              type="email"
              placeholder="seu@email.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              icon={<Mail className="w-5 h-5" />}
              required
              autoComplete="email"
            />

            <Input
              label="Senha"
              type="password"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              icon={<Lock className="w-5 h-5" />}
              required
              autoComplete="current-password"
            />

            <Button
              type="submit"
              isLoading={isLoading}
              className="w-full"
              size="lg"
            >
              Entrar
              <ArrowRight className="w-5 h-5" />
            </Button>
          </form>

          <p className="mt-10 text-center text-base text-surface-500">
            Não tem uma conta?{' '}
            <Link
              to="/register"
              className="text-solar-400 hover:text-solar-300 font-semibold transition-colors"
            >
              Cadastre-se gratuitamente
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
