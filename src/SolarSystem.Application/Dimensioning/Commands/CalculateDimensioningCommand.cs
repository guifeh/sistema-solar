using FluentValidation;
using MediatR;
using SolarSystem.Application.Common;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Common;
using SolarSystem.Domain.Dimensioning;

namespace SolarSystem.Application.Dimensioning.Commands;

/// <summary>
/// US-021 (calculo) e US-022 (ajuste manual). O calculo e sem estado: informar
/// ManualModuleQuantity ou ManualPowerKwp devolve o mesmo resultado recalculado, que e o
/// que permite ao frontend atualizar a geracao em tempo real enquanto o tecnico ajusta.
/// </summary>
public record CalculateDimensioningCommand(
    int ConsumptionKwhMonth,
    string Uf,
    decimal? LossFactor = null,
    string? RoofOrientation = null,
    int? ModulePowerW = null,
    int? ManualModuleQuantity = null,
    decimal? ManualPowerKwp = null
) : IRequest<Result<DimensioningResultDto>>;

public class CalculateDimensioningCommandValidator : AbstractValidator<CalculateDimensioningCommand>
{
    public CalculateDimensioningCommandValidator()
    {
        RuleFor(x => x.ConsumptionKwhMonth)
            .GreaterThan(0).WithMessage("Consumo deve ser maior que zero.")
            .LessThanOrEqualTo(1_000_000).WithMessage("Consumo acima do limite suportado.");

        RuleFor(x => x.Uf)
            .NotEmpty().WithMessage("UF é obrigatória.")
            .Must(BrazilianStates.IsValid).WithMessage("UF inválida.");

        RuleFor(x => x.LossFactor)
            .InclusiveBetween(DimensioningCalculator.MinLossFactor, DimensioningCalculator.MaxLossFactor)
            .When(x => x.LossFactor.HasValue)
            .WithMessage($"Fator de perda deve estar entre {DimensioningCalculator.MinLossFactor} e {DimensioningCalculator.MaxLossFactor}.");

        RuleFor(x => x.RoofOrientation)
            .Must(v => Enum.TryParse<RoofOrientation>(NormalizeOrientation(v), true, out _))
            .When(x => !string.IsNullOrEmpty(x.RoofOrientation))
            .WithMessage("Orientação inválida. Valores aceitos: north, northEast, northWest, east, west, south, flat.");

        RuleFor(x => x.ModulePowerW)
            .InclusiveBetween(100, 1000).When(x => x.ModulePowerW.HasValue)
            .WithMessage("Potência do módulo deve estar entre 100 W e 1000 W.");

        RuleFor(x => x.ManualModuleQuantity)
            .InclusiveBetween(1, 10_000).When(x => x.ManualModuleQuantity.HasValue)
            .WithMessage("Quantidade de módulos deve estar entre 1 e 10000.");

        RuleFor(x => x.ManualPowerKwp)
            .GreaterThan(0).When(x => x.ManualPowerKwp.HasValue)
            .WithMessage("Potência informada deve ser maior que zero.");

        RuleFor(x => x)
            .Must(x => !(x.ManualModuleQuantity.HasValue && x.ManualPowerKwp.HasValue))
            .WithMessage("Informe a quantidade de módulos ou a potência desejada, não os dois.")
            .WithName(nameof(CalculateDimensioningCommand.ManualModuleQuantity));
    }

    /// <summary>Aceita "north-east" alem de "northEast", como aparece na documentacao do épico.</summary>
    internal static string NormalizeOrientation(string? value)
        => value?.Replace("-", string.Empty).Replace("_", string.Empty) ?? string.Empty;
}

public class CalculateDimensioningCommandHandler
    : IRequestHandler<CalculateDimensioningCommand, Result<DimensioningResultDto>>
{
    private readonly IIrradiationRepository _irradiationRepository;

    public CalculateDimensioningCommandHandler(IIrradiationRepository irradiationRepository)
    {
        _irradiationRepository = irradiationRepository;
    }

    public async Task<Result<DimensioningResultDto>> Handle(
        CalculateDimensioningCommand request, CancellationToken ct)
    {
        var irradiation = await _irradiationRepository.GetByUfAsync(request.Uf, ct);
        if (irradiation is null)
            return Result.Failure<DimensioningResultDto>(
                $"Não há dado de irradiação cadastrado para a UF '{request.Uf.ToUpper()}'.");

        var orientation = ParseOrientation(request.RoofOrientation);
        var lossFactor = request.LossFactor ?? DimensioningCalculator.DefaultLossFactor;
        var modulePowerW = request.ModulePowerW ?? DimensioningCalculator.DefaultModulePowerW;

        var result = DimensioningCalculator.Calculate(new DimensioningInput(
            ConsumptionKwhMonth: request.ConsumptionKwhMonth,
            AverageIrradiation: irradiation.AverageIrradiation,
            LossFactor: lossFactor,
            Orientation: orientation,
            ModulePowerW: modulePowerW,
            ManualModuleQuantity: request.ManualModuleQuantity,
            ManualPowerKwp: request.ManualPowerKwp));

        return Result.Success(new DimensioningResultDto(
            ConsumptionKwhMonth: request.ConsumptionKwhMonth,
            Uf: irradiation.Uf,
            AverageIrradiation: irradiation.AverageIrradiation,
            EffectiveIrradiation: result.EffectiveIrradiation,
            LossFactor: lossFactor,
            RoofOrientation: orientation.ToString(),
            SuggestedPowerKwp: result.RequiredPowerKwp,
            Modules: new ModulesDto(result.ModuleQuantity, result.ModulePowerW, result.InstalledPowerKwp),
            Inverter: new InverterDto(result.InverterPowerKw),
            EstimatedGeneration: new GenerationDto(result.GenerationMonthlyKwh, result.GenerationYearlyKwh),
            RoofArea: new RoofAreaDto(result.RoofAreaM2),
            IsManual: result.IsManual,
            CalculatedAt: DateTime.UtcNow));
    }

    private static RoofOrientation ParseOrientation(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return RoofOrientation.North;

        return Enum.Parse<RoofOrientation>(
            CalculateDimensioningCommandValidator.NormalizeOrientation(value), true);
    }
}
