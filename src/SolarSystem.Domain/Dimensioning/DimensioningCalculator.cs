namespace SolarSystem.Domain.Dimensioning;

public record DimensioningInput(
    int ConsumptionKwhMonth,
    decimal AverageIrradiation,
    decimal LossFactor,
    RoofOrientation Orientation,
    int ModulePowerW,
    int? ManualModuleQuantity = null,
    decimal? ManualPowerKwp = null);

public record DimensioningResult(
    decimal RequiredPowerKwp,
    int ModuleQuantity,
    int ModulePowerW,
    decimal InstalledPowerKwp,
    decimal InverterPowerKw,
    decimal EffectiveIrradiation,
    decimal GenerationMonthlyKwh,
    decimal GenerationYearlyKwh,
    decimal RoofAreaM2,
    bool IsManual);

/// <summary>
/// Nucleo de calculo do EP-03. Puro de proposito: nao toca banco nem serviço externo,
/// entao a formula pode ser verificada direto por teste.
///
/// Potencia (kWp) = Consumo (kWh/mes) / (Irradiacao x 30 x Fator de perda x Fator de orientacao)
/// </summary>
public static class DimensioningCalculator
{
    public const int DefaultModulePowerW = 550;
    public const decimal DefaultLossFactor = 0.80m;
    public const decimal MinLossFactor = 0.50m;
    public const decimal MaxLossFactor = 1.00m;

    private const decimal DaysPerMonth = 30m;

    /// <summary>
    /// Densidade de potencia tipica de modulo fotovoltaico atual, em W/m². Um modulo de
    /// 550 W ocupa ~2,3 m². A area escala com a potencia do modulo em vez de usar constante
    /// fixa, para continuar valendo quando o catalogo do EP-05 trouxer outras potencias.
    /// </summary>
    private const decimal ModulePowerDensityWPerM2 = 240m;

    /// <summary>
    /// Potencias de inversor disponiveis no mercado. Substituir pelo catalogo real no EP-05.
    /// </summary>
    private static readonly decimal[] StandardInverterPowersKw =
        { 3m, 5m, 6m, 8m, 10m, 12m, 15m, 20m, 25m, 30m, 36m, 50m, 75m, 100m };

    /// <summary>
    /// Sobredimensionamento maximo do arranjo em relacao ao inversor. 1.2 significa que um
    /// inversor de 5 kW aceita ate 6 kWp de modulos — pratica usual, porque o arranjo
    /// raramente entrega a potencia de pico.
    /// </summary>
    private const decimal MaxArrayToInverterRatio = 1.2m;

    public static DimensioningResult Calculate(DimensioningInput input)
    {
        if (input.ConsumptionKwhMonth <= 0)
            throw new Common.DomainException("Consumo deve ser maior que zero.");
        if (input.AverageIrradiation <= 0)
            throw new Common.DomainException("Irradiação deve ser maior que zero.");
        if (input.LossFactor < MinLossFactor || input.LossFactor > MaxLossFactor)
            throw new Common.DomainException($"Fator de perda deve estar entre {MinLossFactor} e {MaxLossFactor}.");
        if (input.ModulePowerW <= 0)
            throw new Common.DomainException("Potência do módulo deve ser maior que zero.");

        var orientationFactor = RoofOrientationFactors.FactorFor(input.Orientation);
        var effectiveIrradiation = input.AverageIrradiation * input.LossFactor * orientationFactor;

        var requiredPowerKwp = input.ConsumptionKwhMonth / (effectiveIrradiation * DaysPerMonth);

        var (moduleQuantity, isManual) = ResolveModuleQuantity(input, requiredPowerKwp);

        var installedPowerKwp = moduleQuantity * input.ModulePowerW / 1000m;

        // O anual sai do mensal ja arredondado: o numero vai para a proposta, e o cliente
        // que multiplicar por 12 tem que chegar no mesmo valor exibido.
        var generationMonthly = Round(installedPowerKwp * effectiveIrradiation * DaysPerMonth);

        return new DimensioningResult(
            RequiredPowerKwp: Round(requiredPowerKwp),
            ModuleQuantity: moduleQuantity,
            ModulePowerW: input.ModulePowerW,
            InstalledPowerKwp: Round(installedPowerKwp),
            InverterPowerKw: SuggestInverterKw(installedPowerKwp),
            EffectiveIrradiation: Round(effectiveIrradiation),
            GenerationMonthlyKwh: generationMonthly,
            GenerationYearlyKwh: generationMonthly * 12,
            RoofAreaM2: Round(moduleQuantity * input.ModulePowerW / ModulePowerDensityWPerM2),
            IsManual: isManual);
    }

    /// <summary>
    /// US-022: o tecnico pode fixar a quantidade de modulos ou a potencia desejada; o resto
    /// do calculo continua valendo, so a origem da quantidade muda.
    /// </summary>
    private static (int Quantity, bool IsManual) ResolveModuleQuantity(
        DimensioningInput input, decimal requiredPowerKwp)
    {
        if (input.ManualModuleQuantity is { } quantity)
        {
            if (quantity <= 0)
                throw new Common.DomainException("Quantidade de módulos deve ser maior que zero.");
            return (quantity, true);
        }

        if (input.ManualPowerKwp is { } powerKwp)
        {
            if (powerKwp <= 0)
                throw new Common.DomainException("Potência informada deve ser maior que zero.");
            return (ModulesFor(powerKwp, input.ModulePowerW), true);
        }

        return (ModulesFor(requiredPowerKwp, input.ModulePowerW), false);
    }

    private static int ModulesFor(decimal powerKwp, int modulePowerW)
        => Math.Max(1, (int)Math.Ceiling(powerKwp * 1000m / modulePowerW));

    private static decimal SuggestInverterKw(decimal installedPowerKwp)
    {
        var minimumInverterKw = installedPowerKwp / MaxArrayToInverterRatio;

        foreach (var power in StandardInverterPowersKw)
        {
            if (power >= minimumInverterKw)
                return power;
        }

        return StandardInverterPowersKw[^1];
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
