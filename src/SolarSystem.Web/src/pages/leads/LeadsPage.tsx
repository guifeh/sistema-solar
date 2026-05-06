import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Users, Plus, Search, Filter } from 'lucide-react';
import { Card, CardContent, Button, Badge, Spinner } from '../../components/ui';
import { useLeads } from '../../hooks/useLeads';
import { LeadFormModal } from '../../components/leads/LeadFormModal';
import type { LeadStatus } from '../../types/lead';

const statusColors: Record<LeadStatus, 'default' | 'success' | 'warning' | 'danger' | 'info' | 'solar'> = {
  new: 'info',
  contacting: 'warning',
  proposal_sent: 'solar',
  won: 'success',
  lost: 'danger',
};

const statusLabels: Record<LeadStatus, string> = {
  new: 'Novo',
  contacting: 'Em contato',
  proposal_sent: 'Proposta',
  won: 'Ganho',
  lost: 'Perdido',
};

export function LeadsPage() {
  const navigate = useNavigate();
  const [isModalOpen, setIsModalOpen] = useState(false);
  
  // Filters state
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');

  const { data, isLoading, isError } = useLeads({
    page: 1,
    pageSize: 20,
    search: search || undefined,
    status: statusFilter || undefined,
  });

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold text-surface-100">Leads</h1>
          <p className="text-surface-400 mt-1">Gerencie seus contatos e oportunidades</p>
        </div>
        <Button size="md" onClick={() => setIsModalOpen(true)}>
          <Plus className="w-5 h-5 mr-1" />
          Novo lead
        </Button>
      </div>

      {/* Search / filters bar */}
      <Card>
        <CardContent className="flex flex-wrap gap-4 items-center py-4">
          <div className="relative flex-1 min-w-[250px]">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-surface-500" />
            <input
              type="text"
              placeholder="Buscar por nome, telefone, cidade..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full pl-12 pr-4 py-3 rounded-xl bg-surface-900 border-2 border-surface-800 text-base text-surface-100 placeholder-surface-500 focus:outline-none focus:ring-4 focus:ring-solar-500/20 focus:border-solar-500 transition-all"
            />
          </div>
          <div className="flex items-center gap-2 overflow-x-auto pb-1 sm:pb-0">
            <Filter className="w-5 h-5 text-surface-500 mr-2" />
            <button
              onClick={() => setStatusFilter('')}
              className={`px-3 py-1.5 rounded-full text-sm font-medium transition-colors whitespace-nowrap ${
                statusFilter === '' 
                  ? 'bg-solar-500 text-surface-950' 
                  : 'bg-surface-800 text-surface-300 hover:bg-surface-700'
              }`}
            >
              Todos
            </button>
            {Object.entries(statusLabels).map(([key, label]) => (
              <button
                key={key}
                onClick={() => setStatusFilter(key)}
                className={`px-3 py-1.5 rounded-full text-sm font-medium transition-colors whitespace-nowrap ${
                  statusFilter === key 
                    ? 'bg-solar-500 text-surface-950' 
                    : 'bg-surface-800 text-surface-300 hover:bg-surface-700'
                }`}
              >
                {label}
              </button>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Data Table */}
      <Card className="overflow-hidden">
        {isLoading ? (
          <div className="py-24 flex flex-col items-center justify-center">
            <Spinner size="lg" className="mb-4" />
            <p className="text-surface-400">Carregando leads...</p>
          </div>
        ) : isError ? (
          <div className="py-16 text-center">
            <p className="text-red-400">Erro ao carregar leads. Tente novamente.</p>
          </div>
        ) : data?.items.length === 0 ? (
          <div className="py-20 text-center">
            <div className="w-20 h-20 rounded-full bg-surface-800/80 mx-auto mb-6 flex items-center justify-center">
              <Users className="w-10 h-10 text-surface-500" />
            </div>
            <h3 className="text-xl font-bold text-surface-200 mb-2">Nenhum lead encontrado</h3>
            <p className="text-surface-400 max-w-md mx-auto">
              {search || statusFilter 
                ? 'Nenhum contato corresponde aos filtros atuais.' 
                : 'Você ainda não possui leads. Clique em "Novo lead" para começar.'}
            </p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="border-b border-surface-800 bg-surface-900/50">
                  <th className="py-4 px-6 text-sm font-semibold text-surface-400">Nome / Empresa</th>
                  <th className="py-4 px-6 text-sm font-semibold text-surface-400">Contato</th>
                  <th className="py-4 px-6 text-sm font-semibold text-surface-400">Localização</th>
                  <th className="py-4 px-6 text-sm font-semibold text-surface-400">Status</th>
                  <th className="py-4 px-6 text-sm font-semibold text-surface-400 text-right">Data</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-surface-800">
                {data?.items.map((lead) => (
                  <tr 
                    key={lead.id} 
                    onClick={() => navigate(`/leads/${lead.id}`)}
                    className="hover:bg-surface-800/50 cursor-pointer transition-colors"
                  >
                    <td className="py-4 px-6">
                      <div className="font-semibold text-surface-100">{lead.name}</div>
                      <div className="text-sm text-surface-500 mt-1 capitalize">{lead.leadType === 'residential' ? 'Residencial' : lead.leadType === 'commercial' ? 'Comercial' : lead.leadType}</div>
                    </td>
                    <td className="py-4 px-6">
                      <div className="text-surface-200">{lead.phone}</div>
                      {lead.email && <div className="text-sm text-surface-500 mt-1">{lead.email}</div>}
                    </td>
                    <td className="py-4 px-6 text-surface-300">
                      {lead.city && lead.uf ? `${lead.city} - ${lead.uf.toUpperCase()}` : '-'}
                    </td>
                    <td className="py-4 px-6">
                      <Badge variant={statusColors[lead.status] || 'default'}>
                        {statusLabels[lead.status] || lead.status}
                      </Badge>
                    </td>
                    <td className="py-4 px-6 text-right text-sm text-surface-400">
                      {new Date(lead.createdAt).toLocaleDateString('pt-BR')}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      <LeadFormModal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} />
    </div>
  );
}

