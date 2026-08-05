using FluentValidation;
using MediatR;
using SolarSystem.Application.Common;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Common;

namespace SolarSystem.Application.Dimensioning.Queries;

public record GetIrradiationQuery(string Uf) : IRequest<Result<IrradiationDto>>;

public class GetIrradiationQueryValidator : AbstractValidator<GetIrradiationQuery>
{
    public GetIrradiationQueryValidator()
    {
        RuleFor(x => x.Uf)
            .NotEmpty().WithMessage("UF é obrigatória.")
            .Must(BrazilianStates.IsValid).WithMessage("UF inválida.");
    }
}

public class GetIrradiationQueryHandler : IRequestHandler<GetIrradiationQuery, Result<IrradiationDto>>
{
    private readonly IIrradiationRepository _repository;

    public GetIrradiationQueryHandler(IIrradiationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IrradiationDto>> Handle(GetIrradiationQuery request, CancellationToken ct)
    {
        var irradiation = await _repository.GetByUfAsync(request.Uf, ct);
        if (irradiation is null)
            return Result.Failure<IrradiationDto>($"Não há dado de irradiação cadastrado para a UF '{request.Uf.ToUpper()}'.");

        return Result.Success(new IrradiationDto(
            irradiation.Uf,
            irradiation.StateName,
            irradiation.AverageIrradiation,
            irradiation.Source,
            irradiation.UpdatedAt));
    }
}

public record ListIrradiationsQuery : IRequest<IReadOnlyList<IrradiationDto>>;

public class ListIrradiationsQueryHandler : IRequestHandler<ListIrradiationsQuery, IReadOnlyList<IrradiationDto>>
{
    private readonly IIrradiationRepository _repository;

    public ListIrradiationsQueryHandler(IIrradiationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<IrradiationDto>> Handle(ListIrradiationsQuery request, CancellationToken ct)
    {
        var all = await _repository.GetAllAsync(ct);

        return all
            .Select(i => new IrradiationDto(i.Uf, i.StateName, i.AverageIrradiation, i.Source, i.UpdatedAt))
            .ToList();
    }
}
