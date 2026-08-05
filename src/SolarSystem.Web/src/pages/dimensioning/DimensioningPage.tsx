import { useEffect, useState } from 'react';
import { Calculator, Gauge, PanelTop, Ruler, Sun, Wand2, Zap } from 'lucide-react';
import { Button, Card, CardContent, CardHeader, Input, Spinner } from '../../components/ui';
import { ConsumptionEstimatorModal } from '../../components/dimensioning/ConsumptionEstimatorModal';
import { useCalculateDimensioning, useIrradiations } from '../../hooks/useDimensioning';
import {
  ROOF_ORIENTATION_LABELS,
  type DimensioningResult,
  type RoofOrientationValue,
} from '../../types/dimensioning';

type ManualMode = 'off' | 'modules' | 'power';

const DEFAULT_LOSS_FACTOR = 0.8;
const DEFAULT_MODULE_POWER_W = 550;

export function DimensioningPage() {
  const [consumption, setConsumption] = useState('500');
  const [uf, setUf] = useState('SP');
  const [lossFactor, setLossFactor] = useState(String(DEFAULT_LOSS_FACTOR));
  const [orientation, setOrientation] = useState<RoofOrientationValue>('north');
  const [modulePowerW, setModulePowerW] = useState(String(DEFAULT_MODULE_POWER_W));

  const [manualMode, setManualMode] = useState<ManualMode>('off');
  const [manualModules, setManualModules] = useState('');
  const [manualPower, setManualPower] = useState('');

  const [estimatorOpen, setEstimatorOpen] = useState(false);
  const [result, setResult] = useState<DimensioningResult | null>(null);

  const { data: irradiations, isLoading: loadingIrradiations } = useIrradiations();
  const { mutate: calculate, isPending, error } = useCalculateDimensioning();

  const buildPayload = () => ({
    consumptionKwhMonth: Number(consumption),
    uf,
    lossFactor: Number(lossFactor),
    roofOrientation: orientation,
    modulePowerW: Number(modulePowerW),
    manualModuleQuantity: manualMode === 'modules' && manualModules ? Number(manualModules) : null,
    manualPowerKwp: manualMode === 'power' && manualPower ? Number(manualPower) : null,
  });

  const handleCalculate = () => {
    calculate(buildPayload(), { onSuccess: setResult });
  };

  // US-022: com o ajuste manual ligado, mexer nos campos recalcula a geração na hora.
  // O debounce evita disparar uma chamada por tecla digitada.
  useEffect(() => {
    if (manualMode === 'off' || !result) return;
    if (manualMode === 'modules' && !manualModules) return;
    if (manualMode === 'power' && !manualPower) return;

    const timer = setTimeout(() => calculate(buildPayload(), { onSuccess: setResult }), 400);
    return () => clearTimeout(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [manualMode, manualModules, manualPower]);

  const toggleManual = (mode: ManualMode) => {
    if (manualMode === mode) {
      setManualMode('off');
      setManualModules('');
      setManualPower('');
      calculate({ ...buildPayload(), manualModuleQuantity: null, manualPowerKwp: null }, { onSuccess: setResult });
      return;
    }

    setManualMode(mode);
    if (mode === 'modules') setManualModules(String(result?.modules.quantity ?? ''));
    if (mode === 'power') setManualPower(String(result?.modules.totalPowerKwp ?? ''));
  };

  const coversConsumption =
    result !== null && result.estimatedGeneration.monthly >= result.consumptionKwhMonth;

  return (
    <div className="flex flex-col gap-6">
      <header className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-surface-100">Dimensionamento</h1>
          <p className="text-sm text-surface-500">
            Calcula potência, módulos e inversor a partir do consumo e da irradiação da UF.
          </p>
        </div>
        <Button variant="secondary" onClick={() => setEstimatorOpen(true)}>
          <Wand2 className="h-4 w-4" />
          Não tenho a conta de luz
        </Button>
      </header>

      <Card>
        <CardHeader>
          <h2 className="font-semibold text-surface-100">Dados de entrada</h2>
        </CardHeader>
        <CardContent className="flex flex-col gap-5">
          <div className="grid gap-5 md:grid-cols-2">
            <Input
              label="Consumo médio (kWh/mês)"
              type="number"
              min={1}
              value={consumption}
              onChange={(e) => setConsumption(e.target.value)}
              icon={<Zap className="h-5 w-5" />}
            />

            <div className="flex flex-col gap-2">
              <label
                htmlFor="uf"
                className="ml-1 block text-sm font-semibold uppercase tracking-wide text-surface-300"
              >
                UF da instalação
              </label>
              <select
                id="uf"
                value={uf}
                onChange={(e) => setUf(e.target.value)}
                disabled={loadingIrradiations}
                className="w-full rounded-2xl border-2 border-surface-800 bg-surface-900 px-5 py-3 text-base text-surface-100 transition-all duration-300 focus:border-solar-500 focus:outline-none focus:ring-4 focus:ring-solar-500/20"
              >
                {irradiations?.map((item) => (
                  <option key={item.uf} value={item.uf}>
                    {item.uf} — {item.stateName} ({item.averageIrradiation.toFixed(2)} kWh/m²/dia)
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="grid gap-5 md:grid-cols-3">
            <div className="flex flex-col gap-2">
              <label
                htmlFor="orientation"
                className="ml-1 block text-sm font-semibold uppercase tracking-wide text-surface-300"
              >
                Orientação do telhado
              </label>
              <select
                id="orientation"
                value={orientation}
                onChange={(e) => setOrientation(e.target.value as RoofOrientationValue)}
                className="w-full rounded-2xl border-2 border-surface-800 bg-surface-900 px-5 py-3 text-base text-surface-100 transition-all duration-300 focus:border-solar-500 focus:outline-none focus:ring-4 focus:ring-solar-500/20"
              >
                {(Object.keys(ROOF_ORIENTATION_LABELS) as RoofOrientationValue[]).map((value) => (
                  <option key={value} value={value}>
                    {ROOF_ORIENTATION_LABELS[value]}
                  </option>
                ))}
              </select>
            </div>

            <Input
              label="Fator de perda"
              type="number"
              step="0.01"
              min={0.5}
              max={1}
              value={lossFactor}
              onChange={(e) => setLossFactor(e.target.value)}
            />

            <Input
              label="Potência do módulo (W)"
              type="number"
              min={100}
              max={1000}
              value={modulePowerW}
              onChange={(e) => setModulePowerW(e.target.value)}
            />
          </div>

          {error && (
            <p className="text-sm text-red-400">
              {getErrorMessage(error)}
            </p>
          )}

          <div className="flex justify-end">
            <Button onClick={handleCalculate} isLoading={isPending} size="lg">
              <Calculator className="h-5 w-5" />
              Calcular
            </Button>
          </div>
        </CardContent>
      </Card>

      {isPending && !result && (
        <div className="flex justify-center py-10">
          <Spinner />
        </div>
      )}

      {result && (
        <>
          <Card glow>
            <CardHeader className="flex items-center justify-between">
              <h2 className="font-semibold text-surface-100">Resultado</h2>
              {result.isManual && (
                <span className="rounded-full bg-amber-500/15 px-3 py-1 text-xs font-semibold uppercase tracking-wide text-amber-400">
                  Ajuste manual
                </span>
              )}
            </CardHeader>
            <CardContent className="flex flex-col gap-6">
              <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                <Metric
                  icon={<Gauge className="h-5 w-5" />}
                  label="Potência instalada"
                  value={`${result.modules.totalPowerKwp.toFixed(2)} kWp`}
                  hint={`Necessária: ${result.suggestedPowerKwp.toFixed(2)} kWp`}
                />
                <Metric
                  icon={<PanelTop className="h-5 w-5" />}
                  label="Módulos"
                  value={`${result.modules.quantity} × ${result.modules.powerEachW} W`}
                />
                <Metric
                  icon={<Zap className="h-5 w-5" />}
                  label="Inversor sugerido"
                  value={`${result.inverter.suggestedPowerKw} kW`}
                  hint="Modelo virá do catálogo (EP-05)"
                />
                <Metric
                  icon={<Ruler className="h-5 w-5" />}
                  label="Área necessária"
                  value={`${result.roofArea.required.toFixed(2)} ${result.roofArea.unit}`}
                />
              </div>

              <div className="rounded-2xl border border-surface-800 bg-surface-950/60 px-5 py-4">
                <div className="flex flex-wrap items-baseline justify-between gap-4">
                  <div className="flex items-center gap-2 text-sm text-surface-400">
                    <Sun className="h-4 w-4 text-solar-400" />
                    Geração estimada
                  </div>
                  <div className="flex items-baseline gap-6">
                    <span className="text-2xl font-bold text-solar-400">
                      {result.estimatedGeneration.monthly.toLocaleString('pt-BR')}
                      <span className="ml-1 text-sm font-medium text-surface-400">kWh/mês</span>
                    </span>
                    <span className="text-lg font-semibold text-surface-200">
                      {result.estimatedGeneration.yearly.toLocaleString('pt-BR')}
                      <span className="ml-1 text-sm font-medium text-surface-500">kWh/ano</span>
                    </span>
                  </div>
                </div>

                <p className={`mt-3 text-sm ${coversConsumption ? 'text-emerald-400' : 'text-amber-400'}`}>
                  {coversConsumption
                    ? `Cobre os ${result.consumptionKwhMonth} kWh/mês informados.`
                    : `Fica abaixo dos ${result.consumptionKwhMonth} kWh/mês informados — aumente a potência para compensar.`}
                </p>
              </div>

              <p className="text-xs text-surface-500">
                Irradiação {result.uf}: {result.averageIrradiation.toFixed(2)} kWh/m²/dia · efetiva após perdas e
                orientação: {result.effectiveIrradiation.toFixed(2)} kWh/m²/dia
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <h2 className="font-semibold text-surface-100">Ajuste manual</h2>
              <p className="mt-1 text-sm text-surface-500">
                Para restrição de área, sombreamento ou pedido do cliente. A geração recalcula sozinha.
              </p>
            </CardHeader>
            <CardContent className="grid gap-5 md:grid-cols-2">
              <div className="flex flex-col gap-3">
                <ManualToggle
                  label="Fixar quantidade de módulos"
                  active={manualMode === 'modules'}
                  onClick={() => toggleManual('modules')}
                />
                {manualMode === 'modules' && (
                  <Input
                    type="number"
                    min={1}
                    value={manualModules}
                    onChange={(e) => setManualModules(e.target.value)}
                    aria-label="Quantidade de módulos"
                  />
                )}
              </div>

              <div className="flex flex-col gap-3">
                <ManualToggle
                  label="Fixar potência (kWp)"
                  active={manualMode === 'power'}
                  onClick={() => toggleManual('power')}
                />
                {manualMode === 'power' && (
                  <Input
                    type="number"
                    step="0.1"
                    min={0.1}
                    value={manualPower}
                    onChange={(e) => setManualPower(e.target.value)}
                    aria-label="Potência desejada em kWp"
                  />
                )}
              </div>
            </CardContent>
          </Card>
        </>
      )}

      {/* Atribuição exigida pela licença CC BY 4.0 da base de irradiação. */}
      <p className="text-xs text-surface-600">
        Irradiação média por UF derivada do{' '}
        <a
          href="https://power.larc.nasa.gov/"
          target="_blank"
          rel="noreferrer"
          className="underline hover:text-surface-400"
        >
          NASA POWER
        </a>{' '}
        (Prediction of Worldwide Energy Resources), licença CC BY 4.0. Valor médio do estado — para
        maior precisão, use a irradiação do local da instalação.
      </p>

      <ConsumptionEstimatorModal
        isOpen={estimatorOpen}
        uf={uf}
        onClose={() => setEstimatorOpen(false)}
        onApply={(value) => setConsumption(String(value))}
      />
    </div>
  );
}

function Metric({
  icon,
  label,
  value,
  hint,
}: {
  icon: React.ReactNode;
  label: string;
  value: string;
  hint?: string;
}) {
  return (
    <div className="rounded-2xl border border-surface-800 bg-surface-950/60 px-5 py-4">
      <div className="flex items-center gap-2 text-surface-500">
        {icon}
        <span className="text-xs font-semibold uppercase tracking-wide">{label}</span>
      </div>
      <p className="mt-2 text-xl font-bold text-surface-100">{value}</p>
      {hint && <p className="mt-1 text-xs text-surface-500">{hint}</p>}
    </div>
  );
}

function ManualToggle({
  label,
  active,
  onClick,
}: {
  label: string;
  active: boolean;
  onClick: () => void;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`flex items-center justify-between rounded-2xl border-2 px-5 py-3 text-sm font-medium transition-all duration-200 ${
        active
          ? 'border-solar-500 bg-solar-500/10 text-solar-400'
          : 'border-surface-800 bg-surface-900 text-surface-300 hover:border-surface-700'
      }`}
    >
      {label}
      <span className={`text-xs ${active ? 'text-solar-400' : 'text-surface-500'}`}>
        {active ? 'Ativo' : 'Desligado'}
      </span>
    </button>
  );
}

function getErrorMessage(error: unknown): string {
  const response = (error as { response?: { data?: { error?: string; title?: string } } }).response;
  return response?.data?.error ?? response?.data?.title ?? 'Não foi possível calcular o dimensionamento.';
}
