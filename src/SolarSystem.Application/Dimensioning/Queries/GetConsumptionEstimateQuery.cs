using FluentValidation;
using MediatR;
using SolarSystem.Application.Common;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Dimensioning;

namespace SolarSystem.Application.Dimensioning.Queries;

/// <summary>
/// US-020 — estimativa de consumo quando o cliente nao tem a conta de luz em maos.
/// A regiao pode vir pela UF do lead ou explicitamente.
/// </summary>
public record GetConsumptionEstimateQuery(
    string PropertyType,
    int? NumRooms = null,
    bool HasAc = false,
    bool HasWaterHeater = false,
    bool HasPool = false,
    string? Uf = null,
    string? StateGroup = null
) : IRequest<Result<ConsumptionEstimateDto>>;

public class GetConsumptionEstimateQueryValidator : AbstractValidator<GetConsumptionEstimateQuery>
{
    public GetConsumptionEstimateQueryValidator()
    {
        RuleFor(x => x.PropertyType)
            .NotEmpty().WithMessage("Tipo de imóvel é obrigatório.")
            .Must(v => Enum.TryParse<PropertyType>(v, true, out _))
            .WithMessage("Tipo de imóvel inválido. Valores aceitos: apartment, house, commercial.");

        RuleFor(x => x.NumRooms)
            .InclusiveBetween(1, 20).When(x => x.NumRooms.HasValue)
            .WithMessage("Número de cômodos deve estar entre 1 e 20.");

        RuleFor(x => x.Uf)
            .Must(Domain.Common.BrazilianStates.IsValid).When(x => !string.IsNullOrEmpty(x.Uf))
            .WithMessage("UF inválida.");

        RuleFor(x => x.StateGroup)
            .Must(v => Enum.TryParse<StateGroup>(v, true, out _)).When(x => !string.IsNullOrEmpty(x.StateGroup))
            .WithMessage("Região inválida. Valores aceitos: north, northeast, centerWest, southeast, south.");
    }
}

public class GetConsumptionEstimateQueryHandler
    : IRequestHandler<GetConsumptionEstimateQuery, Result<ConsumptionEstimateDto>>
{
    /// <summary>
    /// Regiao com dataset completo. Serve de base quando a regiao pedida ainda nao tem
    /// perfis cadastrados.
    /// </summary>
    private const StateGroup FallbackGroup = StateGroup.Southeast;

    private readonly IConsumptionProfileRepository _repository;

    public GetConsumptionEstimateQueryHandler(IConsumptionProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ConsumptionEstimateDto>> Handle(
        GetConsumptionEstimateQuery request, CancellationToken ct)
    {
        var propertyType = Enum.Parse<PropertyType>(request.PropertyType, true);
        var requestedGroup = ResolveStateGroup(request);

        var candidates = await _repository.GetCandidatesAsync(propertyType, requestedGroup, ct);
        var usedGroup = requestedGroup;
        string? note = null;

        if (candidates.Count == 0 && requestedGroup != FallbackGroup)
        {
            candidates = await _repository.GetCandidatesAsync(propertyType, FallbackGroup, ct);
            usedGroup = FallbackGroup;
            note = $"Ainda não há perfis cadastrados para a região {requestedGroup}. " +
                   $"Estimativa baseada no perfil {FallbackGroup}.";
        }

        if (candidates.Count == 0)
            return Result.Failure<ConsumptionEstimateDto>(
                $"Nenhum perfil de consumo cadastrado para '{request.PropertyType}'.");

        var best = PickBest(candidates, request);
        var exactMatch = Score(best, request) == 0;

        if (!exactMatch && note is null)
            note = "Não há perfil exato para essa combinação; usado o mais próximo cadastrado.";

        return Result.Success(new ConsumptionEstimateDto(
            PropertyType: propertyType.ToString(),
            NumRooms: request.NumRooms,
            HasAc: request.HasAc,
            HasWaterHeater: request.HasWaterHeater,
            HasPool: request.HasPool,
            StateGroup: usedGroup.ToString(),
            Consumption: new ConsumptionRangeDto(best.ConsumptionMin, best.ConsumptionMax, best.ConsumptionAvg),
            IsApproximate: note is not null,
            ApproximationNote: note));
    }

    private static StateGroup ResolveStateGroup(GetConsumptionEstimateQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.StateGroup))
            return Enum.Parse<StateGroup>(request.StateGroup, true);

        return StateGroups.ForUf(request.Uf) ?? FallbackGroup;
    }

    private static ConsumptionProfile PickBest(
        IReadOnlyList<ConsumptionProfile> candidates, GetConsumptionEstimateQuery request)
        => candidates.OrderBy(p => Score(p, request)).ThenBy(p => p.ConsumptionAvg).First();

    /// <summary>
    /// Distancia do perfil ao que foi pedido: quanto menor, melhor. Os equipamentos pesam
    /// mais que o numero de comodos porque impactam muito mais o consumo.
    /// </summary>
    private static int Score(ConsumptionProfile profile, GetConsumptionEstimateQuery request)
    {
        var score = 0;

        if (request.NumRooms.HasValue && profile.NumRooms.HasValue)
            score += Math.Abs(profile.NumRooms.Value - request.NumRooms.Value);
        else if (request.NumRooms.HasValue != profile.NumRooms.HasValue)
            score += 2;

        if (profile.HasAc != request.HasAc) score += 4;
        if (profile.HasWaterHeater != request.HasWaterHeater) score += 4;
        if (profile.HasPool != request.HasPool) score += 4;

        return score;
    }
}
