namespace SolarSystem.Application.Dimensioning;

public record IrradiationDto(
    string Uf,
    string StateName,
    decimal AverageIrradiation,
    string Source,
    DateTime UpdatedAt);

public record ConsumptionRangeDto(int Min, int Max, int Average, string Unit = "kWh/mês");

public record ConsumptionEstimateDto(
    string PropertyType,
    int? NumRooms,
    bool HasAc,
    bool HasWaterHeater,
    bool HasPool,
    string StateGroup,
    ConsumptionRangeDto Consumption,
    bool IsApproximate,
    string? ApproximationNote);

public record ModulesDto(int Quantity, int PowerEachW, decimal TotalPowerKwp);

public record InverterDto(decimal SuggestedPowerKw, string? Brand = null, string? Model = null);

public record GenerationDto(decimal Monthly, decimal Yearly, string Unit = "kWh");

public record RoofAreaDto(decimal Required, string Unit = "m²");

public record DimensioningResultDto(
    int ConsumptionKwhMonth,
    string Uf,
    decimal AverageIrradiation,
    decimal EffectiveIrradiation,
    decimal LossFactor,
    string RoofOrientation,
    decimal SuggestedPowerKwp,
    ModulesDto Modules,
    InverterDto Inverter,
    GenerationDto EstimatedGeneration,
    RoofAreaDto RoofArea,
    bool IsManual,
    DateTime CalculatedAt);
