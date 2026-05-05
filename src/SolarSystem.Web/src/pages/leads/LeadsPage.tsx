import { Users, Plus, Search } from 'lucide-react';
import { Card, CardContent, Button, Badge } from '../../components/ui';

export function LeadsPage() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-surface-100">Leads</h1>
          <p className="text-surface-500 mt-1">Gerencie seus contatos e oportunidades</p>
        </div>
        <Button size="md" disabled>
          <Plus className="w-4 h-4" />
          Novo lead
        </Button>
      </div>

      {/* Search / filters bar */}
      <Card>
        <CardContent className="flex flex-wrap gap-3 items-center py-3">
          <div className="relative flex-1 min-w-[200px]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-surface-500" />
            <input
              type="text"
              placeholder="Buscar por nome, telefone, cidade..."
              className="w-full pl-9 pr-4 py-2 rounded-lg bg-surface-800/60 border border-surface-700 text-sm text-surface-200 placeholder-surface-500 focus:outline-none focus:ring-1 focus:ring-solar-500/50 focus:border-solar-500 transition-all"
              disabled
            />
          </div>
          <div className="flex gap-2">
            <Badge variant="solar">Todos</Badge>
            <Badge variant="default">Novo</Badge>
            <Badge variant="default">Em contato</Badge>
            <Badge variant="default">Proposta</Badge>
          </div>
        </CardContent>
      </Card>

      {/* Empty state */}
      <Card>
        <CardContent className="py-16 text-center">
          <div className="w-16 h-16 rounded-2xl bg-surface-800/60 mx-auto mb-4 flex items-center justify-center">
            <Users className="w-8 h-8 text-surface-600" />
          </div>
          <h3 className="text-lg font-semibold text-surface-300 mb-2">Nenhum lead cadastrado</h3>
          <p className="text-surface-500 max-w-sm mx-auto mb-6">
            Comece cadastrando seus primeiros leads para acompanhar o funil de vendas.
          </p>
          <Badge variant="info">Funcionalidade disponível em breve</Badge>
        </CardContent>
      </Card>
    </div>
  );
}
