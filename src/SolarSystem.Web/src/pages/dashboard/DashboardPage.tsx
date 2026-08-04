import { Sun, Users, FileText, TrendingUp, Zap } from 'lucide-react';
import { useAuth } from '../../hooks/useAuth';
import { Card, CardContent, Badge } from '../../components/ui';

const quickStats = [
  { label: 'Leads ativos', value: '—', icon: Users, color: 'text-blue-400', bgColor: 'bg-blue-500/10' },
  { label: 'Propostas', value: '—', icon: FileText, color: 'text-emerald-400', bgColor: 'bg-emerald-500/10' },
  { label: 'Projetos', value: '—', icon: TrendingUp, color: 'text-purple-400', bgColor: 'bg-purple-500/10' },
  { label: 'Faturamento', value: '—', icon: Zap, color: 'text-solar-400', bgColor: 'bg-solar-500/10' },
];

export function DashboardPage() {
  const { user } = useAuth();

  return (
    <div className="space-y-8">
      {/* Welcome banner */}
      <div className="relative rounded-2xl overflow-hidden">
        <div className="absolute inset-0 gradient-solar opacity-10" />
        <div className="absolute inset-0 bg-gradient-to-r from-surface-900/90 to-surface-900/50" />
        <div className="relative p-8 flex items-center gap-6">
          <div className="w-14 h-14 rounded-2xl gradient-solar flex items-center justify-center shadow-xl animate-pulse-solar">
            <Sun className="w-8 h-8 text-surface-900" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-surface-100 mb-1">
              Olá, {user?.name?.split(' ')[0] || 'Usuário'}! ☀️
            </h1>
            <p className="text-surface-400">
              Bem-vindo ao Sistema Solar. Seu painel de gestão de projetos fotovoltaicos.
            </p>
          </div>
          <div className="ml-auto hidden md:block">
            <Badge variant="solar">
              {user?.tenantName || 'Sua Empresa'}
            </Badge>
          </div>
        </div>
      </div>

      {/* Quick stats */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {quickStats.map((stat) => (
          <Card key={stat.label} hover>
            <CardContent className="flex items-center gap-4">
              <div className={`w-12 h-12 rounded-xl ${stat.bgColor} flex items-center justify-center`}>
                <stat.icon className={`w-6 h-6 ${stat.color}`} />
              </div>
              <div>
                <p className="text-2xl font-bold text-surface-100">{stat.value}</p>
                <p className="text-sm text-surface-500">{stat.label}</p>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Getting started */}
      <Card>
        <CardContent className="py-12 text-center">
          <div className="w-16 h-16 rounded-2xl gradient-solar-subtle mx-auto mb-4 flex items-center justify-center">
            <Zap className="w-8 h-8 text-solar-400" />
          </div>
          <h3 className="text-lg font-semibold text-surface-200 mb-2">Comece por aqui</h3>
          <p className="text-surface-500 max-w-md mx-auto mb-6">
            Cadastre seus primeiros leads, configure seus equipamentos e comece a gerar propostas profissionais.
          </p>
          <div className="flex flex-wrap gap-3 justify-center">
            <div className="flex items-center gap-2 px-4 py-2 rounded-xl bg-surface-800/60 text-surface-400 text-sm">
              <span className="w-6 h-6 rounded-full bg-solar-500/20 text-solar-400 flex items-center justify-center text-xs font-bold">1</span>
              Cadastrar leads
            </div>
            <div className="flex items-center gap-2 px-4 py-2 rounded-xl bg-surface-800/60 text-surface-500 text-sm">
              <span className="w-6 h-6 rounded-full bg-surface-700 text-surface-400 flex items-center justify-center text-xs font-bold">2</span>
              Dimensionar sistema
            </div>
            <div className="flex items-center gap-2 px-4 py-2 rounded-xl bg-surface-800/60 text-surface-500 text-sm">
              <span className="w-6 h-6 rounded-full bg-surface-700 text-surface-400 flex items-center justify-center text-xs font-bold">3</span>
              Gerar proposta
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
