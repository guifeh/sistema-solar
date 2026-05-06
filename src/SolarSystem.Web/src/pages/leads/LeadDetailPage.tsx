import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, MapPin, Phone, Mail, Clock, Building2, Link as LinkIcon, Battery, Plus } from 'lucide-react';
import { useLead, useAddLeadNote, useChangeLeadStatus } from '../../hooks/useLeads';
import { Card, CardHeader, CardContent, Button, Badge, Spinner } from '../../components/ui';
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

export function LeadDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [newNote, setNewNote] = useState('');

  const { data: lead, isLoading, isError } = useLead(id!);
  const addNote = useAddLeadNote();
  const changeStatus = useChangeLeadStatus();

  const handleAddNote = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newNote.trim() || !id) return;
    addNote.mutate({ id, note: newNote }, {
      onSuccess: () => setNewNote('')
    });
  };

  const handleStatusChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    if (!id) return;
    changeStatus.mutate({ id, status: e.target.value as LeadStatus });
  };

  if (isLoading) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[50vh]">
        <Spinner size="lg" className="mb-4" />
        <p className="text-surface-400">Carregando detalhes do lead...</p>
      </div>
    );
  }

  if (isError || !lead) {
    return (
      <div className="text-center py-16">
        <p className="text-red-400 text-lg mb-4">Erro ao carregar informações do lead.</p>
        <Button onClick={() => navigate('/leads')} variant="ghost">Voltar para Leads</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-5xl mx-auto">
      {/* Header Actions */}
      <div className="flex items-center justify-between">
        <button 
          onClick={() => navigate('/leads')}
          className="flex items-center text-surface-400 hover:text-surface-100 transition-colors"
        >
          <ArrowLeft className="w-4 h-4 mr-2" />
          Voltar para Leads
        </button>
        <div className="flex items-center gap-4">
          <select 
            value={lead.status}
            onChange={handleStatusChange}
            disabled={changeStatus.isPending}
            className="bg-surface-800 border border-surface-700 text-surface-100 text-sm rounded-lg focus:ring-solar-500 focus:border-solar-500 block p-2.5"
          >
            {Object.entries(statusLabels).map(([key, label]) => (
              <option key={key} value={key}>{label}</option>
            ))}
          </select>
        </div>
      </div>

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left Column: Details */}
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardContent className="p-6">
              <div className="flex justify-between items-start mb-6">
                <div>
                  <h1 className="text-3xl font-bold text-surface-100 mb-2">{lead.name}</h1>
                  <Badge variant={statusColors[lead.status]}>
                    {statusLabels[lead.status]}
                  </Badge>
                </div>
                <div className="text-right text-sm text-surface-500">
                  <p>Criado em: {new Date(lead.createdAt).toLocaleDateString('pt-BR')}</p>
                  {lead.updatedAt && <p>Atualizado: {new Date(lead.updatedAt).toLocaleDateString('pt-BR')}</p>}
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-8">
                <div className="flex items-start gap-3">
                  <div className="p-2 rounded-lg bg-surface-800 text-surface-400">
                    <Phone className="w-5 h-5" />
                  </div>
                  <div>
                    <p className="text-sm text-surface-500 font-medium">Telefone</p>
                    <p className="text-surface-100">{lead.phone}</p>
                  </div>
                </div>

                <div className="flex items-start gap-3">
                  <div className="p-2 rounded-lg bg-surface-800 text-surface-400">
                    <Mail className="w-5 h-5" />
                  </div>
                  <div>
                    <p className="text-sm text-surface-500 font-medium">E-mail</p>
                    <p className="text-surface-100">{lead.email || 'Não informado'}</p>
                  </div>
                </div>

                <div className="flex items-start gap-3">
                  <div className="p-2 rounded-lg bg-surface-800 text-surface-400">
                    <MapPin className="w-5 h-5" />
                  </div>
                  <div>
                    <p className="text-sm text-surface-500 font-medium">Localização</p>
                    <p className="text-surface-100">
                      {lead.city && lead.uf ? `${lead.city} - ${lead.uf.toUpperCase()}` : 'Não informada'}
                    </p>
                  </div>
                </div>

                <div className="flex items-start gap-3">
                  <div className="p-2 rounded-lg bg-surface-800 text-surface-400">
                    <Building2 className="w-5 h-5" />
                  </div>
                  <div>
                    <p className="text-sm text-surface-500 font-medium">Tipo</p>
                    <p className="text-surface-100 capitalize">
                      {lead.leadType === 'residential' ? 'Residencial' : lead.leadType === 'commercial' ? 'Comercial' : lead.leadType}
                    </p>
                  </div>
                </div>

                <div className="flex items-start gap-3">
                  <div className="p-2 rounded-lg bg-surface-800 text-surface-400">
                    <LinkIcon className="w-5 h-5" />
                  </div>
                  <div>
                    <p className="text-sm text-surface-500 font-medium">Origem</p>
                    <p className="text-surface-100 capitalize">
                      {lead.leadSource}
                    </p>
                  </div>
                </div>

                <div className="flex items-start gap-3">
                  <div className="p-2 rounded-lg bg-surface-800 text-solar-400">
                    <Battery className="w-5 h-5" />
                  </div>
                  <div>
                    <p className="text-sm text-surface-500 font-medium">Consumo Estimado</p>
                    <p className="text-surface-100 font-semibold">
                      {lead.consumptionEstimate ? `${lead.consumptionEstimate} kWh/mês` : 'Não informado'}
                    </p>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Right Column: Notes & History */}
        <div className="lg:col-span-1 space-y-6">
          <Card className="h-full flex flex-col">
            <CardHeader className="border-b border-surface-800 pb-4">
              <h3 className="text-lg font-bold text-surface-100 flex items-center gap-2">
                <Clock className="w-5 h-5 text-solar-500" />
                Anotações
              </h3>
            </CardHeader>
            <CardContent className="flex-1 flex flex-col pt-4">
              
              <div className="flex-1 min-h-[200px] mb-4 overflow-y-auto pr-2 space-y-4">
                {lead.notes ? (
                  <div className="p-4 rounded-xl bg-surface-800/50 text-surface-200 text-sm whitespace-pre-wrap">
                    {lead.notes}
                  </div>
                ) : (
                  <p className="text-surface-500 text-sm text-center py-8">
                    Nenhuma anotação registrada ainda.
                  </p>
                )}
              </div>

              <form onSubmit={handleAddNote} className="mt-auto pt-4 border-t border-surface-800">
                <textarea
                  value={newNote}
                  onChange={(e) => setNewNote(e.target.value)}
                  placeholder="Adicionar nova anotação..."
                  className="w-full rounded-xl bg-surface-900 border border-surface-700 text-surface-100 p-3 text-sm focus:ring-1 focus:ring-solar-500 resize-none h-20 mb-3"
                  required
                />
                <Button 
                  type="submit" 
                  className="w-full" 
                  size="sm"
                  isLoading={addNote.isPending}
                >
                  <Plus className="w-4 h-4 mr-2" />
                  Adicionar Nota
                </Button>
              </form>

            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
