import { useQuery, useMutation } from '@tanstack/react-query';
import api from '../lib/api';
import type {
  CalculateDimensioningRequest,
  ConsumptionEstimate,
  ConsumptionEstimateParams,
  DimensioningResult,
  Irradiation,
} from '../types/dimensioning';

export const dimensioningKeys = {
  all: ['dimensioning'] as const,
  irradiations: () => [...dimensioningKeys.all, 'irradiations'] as const,
  estimate: (params: ConsumptionEstimateParams) =>
    [...dimensioningKeys.all, 'estimate', params] as const,
};

/** Lista das 27 UFs com irradiação — alimenta o dropdown e é praticamente imutável. */
export function useIrradiations() {
  return useQuery({
    queryKey: dimensioningKeys.irradiations(),
    queryFn: async () => {
      const { data } = await api.get<Irradiation[]>('/dimensionamento/irradiacao');
      return data;
    },
    staleTime: Infinity,
  });
}

export function useConsumptionEstimate(params: ConsumptionEstimateParams, enabled: boolean) {
  return useQuery({
    queryKey: dimensioningKeys.estimate(params),
    queryFn: async () => {
      const { data } = await api.get<ConsumptionEstimate>('/dimensionamento/estimativa-consumo', {
        params,
      });
      return data;
    },
    enabled,
  });
}

export function useCalculateDimensioning() {
  return useMutation({
    mutationFn: async (payload: CalculateDimensioningRequest) => {
      const { data } = await api.post<DimensioningResult>('/dimensionamento/calcular', payload);
      return data;
    },
  });
}
