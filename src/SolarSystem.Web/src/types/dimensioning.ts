export type PropertyType = 'apartment' | 'house' | 'commercial';

export type RoofOrientationValue =
  | 'north'
  | 'northEast'
  | 'northWest'
  | 'east'
  | 'west'
  | 'south'
  | 'flat';

export interface Irradiation {
  uf: string;
  stateName: string;
  averageIrradiation: number;
  source: string;
  updatedAt: string;
}

export interface ConsumptionRange {
  min: number;
  max: number;
  average: number;
  unit: string;
}

export interface ConsumptionEstimate {
  propertyType: string;
  numRooms: number | null;
  hasAc: boolean;
  hasWaterHeater: boolean;
  hasPool: boolean;
  stateGroup: string;
  consumption: ConsumptionRange;
  isApproximate: boolean;
  approximationNote: string | null;
}

export interface ConsumptionEstimateParams {
  propertyType: PropertyType;
  numRooms?: number | null;
  hasAc?: boolean;
  hasWaterHeater?: boolean;
  hasPool?: boolean;
  uf?: string;
}

export interface CalculateDimensioningRequest {
  consumptionKwhMonth: number;
  uf: string;
  lossFactor?: number;
  roofOrientation?: RoofOrientationValue;
  modulePowerW?: number;
  /** US-022: informar um destes dois fixa o sistema e recalcula a geração. */
  manualModuleQuantity?: number | null;
  manualPowerKwp?: number | null;
}

export interface DimensioningResult {
  consumptionKwhMonth: number;
  uf: string;
  averageIrradiation: number;
  effectiveIrradiation: number;
  lossFactor: number;
  roofOrientation: string;
  suggestedPowerKwp: number;
  modules: {
    quantity: number;
    powerEachW: number;
    totalPowerKwp: number;
  };
  inverter: {
    suggestedPowerKw: number;
    brand: string | null;
    model: string | null;
  };
  estimatedGeneration: {
    monthly: number;
    yearly: number;
    unit: string;
  };
  roofArea: {
    required: number;
    unit: string;
  };
  isManual: boolean;
  calculatedAt: string;
}

export const PROPERTY_TYPE_LABELS: Record<PropertyType, string> = {
  apartment: 'Apartamento',
  house: 'Casa',
  commercial: 'Comercial',
};

export const ROOF_ORIENTATION_LABELS: Record<RoofOrientationValue, string> = {
  north: 'Norte (ideal)',
  northEast: 'Nordeste',
  northWest: 'Noroeste',
  east: 'Leste',
  west: 'Oeste',
  south: 'Sul',
  flat: 'Laje / plano',
};
