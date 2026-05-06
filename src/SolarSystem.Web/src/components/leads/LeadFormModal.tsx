import { useState } from 'react';
import { useCreateLead } from '../../hooks/useLeads';
import { Modal, Input, Button } from '../ui';
import { User, Phone, Mail, MapPin, Building2, Link as LinkIcon, Battery, FileText } from 'lucide-react';

interface LeadFormModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function LeadFormModal({ isOpen, onClose }: LeadFormModalProps) {
  const [formData, setFormData] = useState({
    name: '',
    phone: '',
    email: '',
    city: '',
    uf: '',
    leadType: 'residential',
    leadSource: 'website',
    consumptionEstimate: '',
    notes: ''
  });

  const createLead = useCreateLead();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    createLead.mutate(
      {
        ...formData,
        consumptionEstimate: formData.consumptionEstimate ? Number(formData.consumptionEstimate) : undefined
      },
      {
        onSuccess: () => {
          setFormData({
            name: '', phone: '', email: '', city: '', uf: '',
            leadType: 'residential', leadSource: 'website', consumptionEstimate: '', notes: ''
          });
          onClose();
        }
      }
    );
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    setFormData(prev => ({ ...prev, [e.target.name]: e.target.value }));
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Novo Lead">
      <form onSubmit={handleSubmit} className="space-y-6">
        
        {createLead.isError && (
          <div className="p-3 rounded-lg bg-red-500/10 border border-red-500/20 text-sm text-red-400">
            Erro ao criar lead. Verifique os dados.
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input
            label="Nome do Lead / Empresa *"
            name="name"
            value={formData.name}
            onChange={handleChange}
            icon={<User className="w-4 h-4" />}
            required
            placeholder="Ex: João Silva"
          />
          <Input
            label="Telefone / WhatsApp *"
            name="phone"
            value={formData.phone}
            onChange={handleChange}
            icon={<Phone className="w-4 h-4" />}
            required
            placeholder="(11) 99999-9999"
          />
          <Input
            label="E-mail"
            name="email"
            type="email"
            value={formData.email}
            onChange={handleChange}
            icon={<Mail className="w-4 h-4" />}
            placeholder="joao@exemplo.com"
          />
          <div className="grid grid-cols-3 gap-2">
            <div className="col-span-2">
              <Input
                label="Cidade"
                name="city"
                value={formData.city}
                onChange={handleChange}
                icon={<MapPin className="w-4 h-4" />}
                placeholder="São Paulo"
              />
            </div>
            <Input
              label="UF"
              name="uf"
              value={formData.uf}
              onChange={handleChange}
              placeholder="SP"
              maxLength={2}
            />
          </div>

          <div className="flex flex-col gap-2">
            <label className="block text-sm font-semibold tracking-wide text-surface-300 uppercase ml-1">
              Tipo de Cliente
            </label>
            <div className="relative flex items-center">
              <div className="absolute left-4 flex items-center pointer-events-none text-surface-500">
                <Building2 className="w-4 h-4" />
              </div>
              <select
                name="leadType"
                value={formData.leadType}
                onChange={handleChange}
                className="w-full rounded-2xl bg-surface-900 border-2 border-surface-800 text-surface-100 focus:outline-none focus:ring-4 focus:ring-solar-500/20 focus:border-solar-500 transition-all duration-300 ease-out pr-5 py-3 text-base appearance-none"
                style={{ paddingLeft: '3.5rem' }}
              >
                <option value="residential">Residencial</option>
                <option value="commercial">Comercial</option>
                <option value="industrial">Industrial</option>
                <option value="rural">Rural</option>
              </select>
            </div>
          </div>

          <div className="flex flex-col gap-2">
            <label className="block text-sm font-semibold tracking-wide text-surface-300 uppercase ml-1">
              Origem
            </label>
            <div className="relative flex items-center">
              <div className="absolute left-4 flex items-center pointer-events-none text-surface-500">
                <LinkIcon className="w-4 h-4" />
              </div>
              <select
                name="leadSource"
                value={formData.leadSource}
                onChange={handleChange}
                className="w-full rounded-2xl bg-surface-900 border-2 border-surface-800 text-surface-100 focus:outline-none focus:ring-4 focus:ring-solar-500/20 focus:border-solar-500 transition-all duration-300 ease-out pr-5 py-3 text-base appearance-none"
                style={{ paddingLeft: '3.5rem' }}
              >
                <option value="website">Site</option>
                <option value="referral">Indicação</option>
                <option value="social_media">Redes Sociais</option>
                <option value="ads">Anúncios</option>
                <option value="other">Outros</option>
              </select>
            </div>
          </div>

          <Input
            label="Consumo Estimado (kWh)"
            name="consumptionEstimate"
            type="number"
            value={formData.consumptionEstimate}
            onChange={handleChange}
            icon={<Battery className="w-4 h-4" />}
            placeholder="Ex: 500"
          />
        </div>

        <div className="flex flex-col gap-2">
          <label className="block text-sm font-semibold tracking-wide text-surface-300 uppercase ml-1">
            Anotações Iniciais
          </label>
          <div className="relative flex">
            <div className="absolute left-4 top-4 flex items-center pointer-events-none text-surface-500">
              <FileText className="w-4 h-4" />
            </div>
            <textarea
              name="notes"
              value={formData.notes}
              onChange={handleChange}
              rows={3}
              className="w-full rounded-2xl bg-surface-900 border-2 border-surface-800 text-surface-100 placeholder-surface-600 focus:outline-none focus:ring-4 focus:ring-solar-500/20 focus:border-solar-500 transition-all duration-300 ease-out pr-5 py-3 text-base resize-none"
              style={{ paddingLeft: '3.5rem' }}
              placeholder="Adicione informações relevantes sobre este contato..."
            />
          </div>
        </div>

        <div className="flex justify-end gap-3 pt-4 border-t border-surface-800">
          <Button type="button" variant="ghost" onClick={onClose}>
            Cancelar
          </Button>
          <Button type="submit" isLoading={createLead.isPending}>
            Salvar Lead
          </Button>
        </div>
      </form>
    </Modal>
  );
}
