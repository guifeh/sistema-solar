using FluentValidation;
using MediatR;
using SolarSystem.Application.Common;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Leads;

namespace SolarSystem.Application.Leads.Commands;

public record CreateLeadCommand(
    string Name,
    string Phone,
    string? Email,
    string? City,
    string? Uf,
    string? LeadType,
    string? LeadSource,
    int? ConsumptionEstimate,
    string? Notes
) : IRequest<Result<LeadDto>>;

public class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefone é obrigatório.")
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("E-mail inválido.");

        RuleFor(x => x.Uf)
            .Length(2).When(x => !string.IsNullOrEmpty(x.Uf))
            .WithMessage("UF deve ter 2 caracteres.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("Cidade deve ter no máximo 100 caracteres.");
    }
}

public class CreateLeadCommandHandler : IRequestHandler<CreateLeadCommand, Result<LeadDto>>
{
    private readonly ILeadRepository _leadRepository;
    private readonly ICurrentUserService _currentUser;

    public CreateLeadCommandHandler(ILeadRepository leadRepository, ICurrentUserService currentUser)
    {
        _leadRepository = leadRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<LeadDto>> Handle(CreateLeadCommand request, CancellationToken ct)
    {
        var lead = Lead.Create(
            _currentUser.TenantId,
            _currentUser.UserId,
            request.Name,
            request.Phone,
            request.Email,
            request.City,
            request.Uf);

        if (!string.IsNullOrEmpty(request.LeadType) && Enum.TryParse<LeadType>(request.LeadType, true, out var lt))
            lead.Update(leadType: lt);

        if (!string.IsNullOrEmpty(request.LeadSource) && Enum.TryParse<LeadSource>(request.LeadSource, true, out var ls))
            lead.Update(leadSource: ls);

        if (request.ConsumptionEstimate.HasValue)
            lead.Update(consumptionEstimate: request.ConsumptionEstimate.Value);

        if (!string.IsNullOrEmpty(request.Notes))
            lead.AddNote(request.Notes);

        await _leadRepository.CreateAsync(lead, ct);

        return Result.Success(LeadDto.FromEntity(lead));
    }
}
