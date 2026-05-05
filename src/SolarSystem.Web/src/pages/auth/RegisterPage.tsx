import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Sun, Mail, Lock, User, Building2, ArrowRight } from 'lucide-react';
import { useAuth } from '../../contexts/AuthContext';
import { Button, Input } from '../../components/ui';

export function RegisterPage() {
  const [companyName, setCompanyName] = useState('');
  const [adminName, setAdminName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const { register } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');

    if (password !== confirmPassword) {
      setError('As senhas não coincidem.');
      return;
    }

    if (password.length < 6) {
      setError('A senha deve ter pelo menos 6 caracteres.');
      return;
    }

    setIsLoading(true);

    try {
      await register({ companyName, adminName, email, password });
      navigate('/dashboard');
    } catch (err: unknown) {
      const axiosError = err as { response?: { data?: { message?: string } } };
      setError(axiosError.response?.data?.message || 'Erro ao criar conta. Tente novamente.');
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
        <div className="relative z-10 flex flex-col justify-center px-16">
          <div className="flex items-center gap-3 mb-8">
            <div className="w-12 h-12 rounded-2xl gradient-solar flex items-center justify-center shadow-xl animate-pulse-solar">
              <Sun className="w-7 h-7 text-surface-900" />
            </div>
            <h1 className="text-3xl font-bold text-surface-100">Sistema Solar</h1>
          </div>
          <h2 className="text-5xl font-bold text-surface-100 leading-tight mb-6">
            Comece a gerar
            <br />
            <span className="text-solar-400">propostas profissionais</span>
            <br />
            em minutos.
          </h2>
          <p className="text-lg text-surface-400 max-w-md">
            Cadastre sua empresa e tenha acesso a todo o ecossistema de gestão para integradores solares.
          </p>
        </div>
      </div>

      {/* Right panel — register form */}
      <div className="flex-1 flex items-center justify-center p-8">
        <div className="w-full max-w-md animate-fade-in">
          {/* Mobile logo */}
          <div className="flex items-center gap-3 mb-8 lg:hidden">
            <div className="w-10 h-10 rounded-xl gradient-solar flex items-center justify-center shadow-lg">
              <Sun className="w-6 h-6 text-surface-900" />
            </div>
            <h1 className="text-2xl font-bold text-surface-100">Sistema Solar</h1>
          </div>

          <div>
            <h2 className="text-2xl font-bold text-surface-100 mb-2">Crie sua conta</h2>
            <p className="text-surface-400 mb-8">Cadastre sua empresa e comece hoje</p>
          </div>

          <form onSubmit={handleSubmit} className="space-y-4">
            {error && (
              <div className="p-3 rounded-xl bg-red-500/10 border border-red-500/20 text-sm text-red-400 animate-fade-in">
                {error}
              </div>
            )}

            <Input
              label="Nome da empresa"
              type="text"
              placeholder="Solar Tech Ltda"
              value={companyName}
              onChange={(e) => setCompanyName(e.target.value)}
              icon={<Building2 className="w-4 h-4" />}
              required
            />

            <Input
              label="Seu nome"
              type="text"
              placeholder="João Silva"
              value={adminName}
              onChange={(e) => setAdminName(e.target.value)}
              icon={<User className="w-4 h-4" />}
              required
            />

            <Input
              label="E-mail"
              type="email"
              placeholder="joao@solartech.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              icon={<Mail className="w-4 h-4" />}
              required
              autoComplete="email"
            />

            <Input
              label="Senha"
              type="password"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              icon={<Lock className="w-4 h-4" />}
              required
              autoComplete="new-password"
            />

            <Input
              label="Confirmar senha"
              type="password"
              placeholder="••••••••"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              icon={<Lock className="w-4 h-4" />}
              required
              autoComplete="new-password"
            />

            <Button
              type="submit"
              isLoading={isLoading}
              className="w-full"
              size="lg"
            >
              Criar conta
              <ArrowRight className="w-4 h-4" />
            </Button>
          </form>

          <p className="mt-8 text-center text-sm text-surface-500">
            Já tem uma conta?{' '}
            <Link
              to="/login"
              className="text-solar-400 hover:text-solar-300 font-medium transition-colors"
            >
              Faça login
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
