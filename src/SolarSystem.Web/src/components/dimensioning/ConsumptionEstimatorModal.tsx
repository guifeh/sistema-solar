import { useState } from 'react';
import { Lightbulb } from 'lucide-react';
import { Button, Modal, Spinner } from '../ui';
import { useConsumptionEstimate } from '../../hooks/useDimensioning';
import {
  PROPERTY_TYPE_LABELS,
  type ConsumptionEstimateParams,
  type PropertyType,
} from '../../types/dimensioning';

interface Props {
  isOpen: boolean;
  uf?: string;
  onClose: () => void;
  onApply: (consumptionKwhMonth: number) => void;
}

const ROOM_OPTIONS = [1, 2, 3, 4, 5, 6];

/**
 * US-020 — o vendedor monta o perfil do imóvel e o sistema devolve a faixa estimada,
 * para o orçamento não travar quando o cliente não tem a conta de luz em mãos.
 */
export function ConsumptionEstimatorModal({ isOpen, uf, onClose, onApply }: Props) {
  const [propertyType, setPropertyType] = useState<PropertyType>('house');
  const [numRooms, setNumRooms] = useState<number | null>(3);
  const [hasAc, setHasAc] = useState(false);
  const [hasWaterHeater, setHasWaterHeater] = useState(false);
  const [hasPool, setHasPool] = useState(false);

  const isCommercial = propertyType === 'commercial';

  const params: ConsumptionEstimateParams = {
    propertyType,
    numRooms: isCommercial ? null : numRooms,
    hasAc,
    hasWaterHeater,
    hasPool,
    uf,
  };

  const { data: estimate, isLoading, isError } = useConsumptionEstimate(params, isOpen);

  const handlePropertyTypeChange = (value: PropertyType) => {
    setPropertyType(value);
    setNumRooms(value === 'commercial' ? null : 3);
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Estimar consumo pelo perfil do imóvel">
      <div className="flex flex-col gap-6">
        <div className="flex flex-col gap-2">
          <span className="block text-sm font-semibold tracking-wide text-surface-300 uppercase ml-1">
            Tipo de imóvel
          </span>
          <div className="grid grid-cols-3 gap-2">
            {(Object.keys(PROPERTY_TYPE_LABELS) as PropertyType[]).map((type) => (
              <button
                key={type}
                type="button"
                onClick={() => handlePropertyTypeChange(type)}
                className={`rounded-2xl border-2 px-4 py-3 text-sm font-medium transition-all duration-200 ${
                  propertyType === type
                    ? 'border-solar-500 bg-solar-500/10 text-solar-400'
                    : 'border-surface-800 bg-surface-900 text-surface-300 hover:border-surface-700'
                }`}
              >
                {PROPERTY_TYPE_LABELS[type]}
              </button>
            ))}
          </div>
        </div>

        {!isCommercial && (
          <div className="flex flex-col gap-2">
            <span className="block text-sm font-semibold tracking-wide text-surface-300 uppercase ml-1">
              Cômodos
            </span>
            <div className="grid grid-cols-6 gap-2">
              {ROOM_OPTIONS.map((rooms) => (
                <button
                  key={rooms}
                  type="button"
                  onClick={() => setNumRooms(rooms)}
                  className={`rounded-2xl border-2 py-3 text-sm font-medium transition-all duration-200 ${
                    numRooms === rooms
                      ? 'border-solar-500 bg-solar-500/10 text-solar-400'
                      : 'border-surface-800 bg-surface-900 text-surface-300 hover:border-surface-700'
                  }`}
                >
                  {rooms}
                </button>
              ))}
            </div>
          </div>
        )}

        <div className="flex flex-col gap-2">
          <span className="block text-sm font-semibold tracking-wide text-surface-300 uppercase ml-1">
            Equipamentos
          </span>
          <div className="flex flex-col gap-2">
            <ToggleRow label="Ar-condicionado" checked={hasAc} onChange={setHasAc} />
            <ToggleRow label="Aquecedor / chuveiro elétrico" checked={hasWaterHeater} onChange={setHasWaterHeater} />
            <ToggleRow label="Piscina aquecida" checked={hasPool} onChange={setHasPool} />
          </div>
        </div>

        <div className="rounded-2xl border border-surface-800 bg-surface-950/60 px-5 py-4">
          {isLoading && (
            <div className="flex items-center gap-3 text-surface-400">
              <Spinner />
              <span className="text-sm">Calculando estimativa…</span>
            </div>
          )}

          {isError && (
            <p className="text-sm text-red-400">
              Não foi possível estimar o consumo para esse perfil.
            </p>
          )}

          {estimate && !isLoading && (
            <div className="flex flex-col gap-3">
              <div className="flex items-baseline justify-between gap-4">
                <span className="text-sm text-surface-400">Consumo estimado</span>
                <span className="text-3xl font-bold text-solar-400">
                  {estimate.consumption.average}
                  <span className="ml-1 text-base font-medium text-surface-400">kWh/mês</span>
                </span>
              </div>

              <div className="flex items-center justify-between text-sm text-surface-500">
                <span>Faixa típica</span>
                <span>
                  {estimate.consumption.min} – {estimate.consumption.max} kWh/mês
                </span>
              </div>

              {estimate.isApproximate && estimate.approximationNote && (
                <p className="flex items-start gap-2 text-xs text-amber-400/90">
                  <Lightbulb className="mt-0.5 h-4 w-4 shrink-0" />
                  {estimate.approximationNote}
                </p>
              )}
            </div>
          )}
        </div>

        <div className="flex justify-end gap-3">
          <Button variant="ghost" onClick={onClose}>
            Cancelar
          </Button>
          <Button
            disabled={!estimate}
            onClick={() => {
              if (estimate) {
                onApply(estimate.consumption.average);
                onClose();
              }
            }}
          >
            Usar {estimate?.consumption.average ?? ''} kWh/mês
          </Button>
        </div>
      </div>
    </Modal>
  );
}

function ToggleRow({
  label,
  checked,
  onChange,
}: {
  label: string;
  checked: boolean;
  onChange: (value: boolean) => void;
}) {
  return (
    <label className="flex cursor-pointer items-center justify-between rounded-2xl border-2 border-surface-800 bg-surface-900 px-5 py-3 transition-colors hover:border-surface-700">
      <span className="text-sm text-surface-200">{label}</span>
      <input
        type="checkbox"
        checked={checked}
        onChange={(e) => onChange(e.target.checked)}
        className="h-5 w-5 accent-solar-500"
      />
    </label>
  );
}
