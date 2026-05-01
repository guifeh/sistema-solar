using FluentValidation;
using MediatR;
using SolarSystem.Application.Common;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Leads;

namespace SolarSystem.Application.Leads.Commands;

public record UpdateLeadCommand(
    Guid Id,
    string? Name,
    string? Phone,
    string? Email,
    string? City,
    string? Uf,
    string? LeadType,
    string? LeadSource,
    int? ConsumptionEstimate
) : IRequest<Result<LeadDto>>;

public class UpdateLeadCommandValidator : AbstractValidator<UpdateLeadCommand>
{
    public UpdateLeadCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).MaximumLength(200).When(x => x.Name != null);
        RuleFor(x => x.Phone).MaximumLength(20).When(x => x.Phone != null);
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email != null);
        RuleFor(x => x.Uf).Length(2).When(x => x.Uf != null);
        RuleFor(x => x.City).MaximumLength(100).When(x => x.City != null);
    }
}

public class UpdateLeadCommandHandler : IRequestHandler<UpdateLeadCommand, Result<LeadDto>>
{
    private readonly ILeadRepository _leadRepository;

    public UpdateLeadCommandHandler(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<Result<LeadDto>> Handle(UpdateLeadCommand request, CancellationToken ct)
    {
        var lead = await _leadRepository.GetByIdAsync(request.Id, ct);
        if (lead == null)
            return Result.Failure<LeadDto>("Lead não encontrado.");

        LeadType? lt = null;
        if (!string.IsNullOrEmpty(request.LeadType) && Enum.TryParse<LeadType>(request.LeadType, true, out var ltParsed))
            lt = ltParsed;

        LeadSource? ls = null;
        if (!string.IsNullOrEmpty(request.LeadSource) && Enum.TryParse<LeadSource>(request.LeadSource, true, out var lsParsed))
            ls = lsParsed;

        lead.Update(
            name: request.Name,
            phone: request.Phone,
            email: request.Email,
            city: request.City,
            uf: request.Uf,
            leadType: lt,
            leadSource: ls,
            consumptionEstimate: request.ConsumptionEstimate);

        await _leadRepository.UpdateAsync(lead, ct);

        return Result.Success(LeadDto.FromEntity(lead));
    }
}
