import { Sun } from 'lucide-react';

export function ProposalPublicPage() {
  return (
    <div className="max-w-4xl mx-auto px-6 py-12 animate-fade-in">
      <div className="text-center mb-12">
        <div className="w-16 h-16 rounded-2xl gradient-solar mx-auto mb-6 flex items-center justify-center shadow-xl">
          <Sun className="w-9 h-9 text-surface-900" />
        </div>
        <h1 className="text-3xl font-bold text-surface-100 mb-3">Sua Proposta de Energia Solar</h1>
        <p className="text-surface-400 text-lg">
          Veja os detalhes do seu sistema fotovoltaico e simule a economia em tempo real.
        </p>
      </div>

      <div className="rounded-2xl bg-surface-900/80 border border-surface-800 p-8 text-center">
        <p className="text-surface-500">
          A proposta interativa estará disponível quando o módulo de geração de propostas for implementado (EP-07).
        </p>
      </div>
    </div>
  );
}
