using MediatR;
using SolarSystem.Application.Common;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Leads;

namespace SolarSystem.Application.Leads.Queries;

public record GetLeadsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    string? Uf = null,
    string? Type = null,
    string? Search = null
) : IRequest<PaginatedResult<LeadDto>>;

public class GetLeadsQueryHandler : IRequestHandler<GetLeadsQuery, PaginatedResult<LeadDto>>
{
    private readonly ILeadRepository _leadRepository;
    private readonly ICurrentUserService _currentUser;

    public GetLeadsQueryHandler(ILeadRepository leadRepository, ICurrentUserService currentUser)
    {
        _leadRepository = leadRepository;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<LeadDto>> Handle(GetLeadsQuery request, CancellationToken ct)
    {
        var filter = new LeadFilter
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Uf = request.Uf,
            Search = request.Search
        };

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<LeadStatus>(request.Status, true, out var status))
            filter.Status = status;

        if (!string.IsNullOrEmpty(request.Type) && Enum.TryParse<LeadType>(request.Type, true, out var type))
            filter.Type = type;

        var (leads, total) = await _leadRepository.GetByTenantAsync(_currentUser.TenantId, filter, ct);

        return new PaginatedResult<LeadDto>
        {
            Items = leads.Select(LeadDto.FromEntity).ToList(),
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        };
    }
}

public record GetLeadByIdQuery(Guid LeadId) : IRequest<Result<LeadDto>>;

public class GetLeadByIdQueryHandler : IRequestHandler<GetLeadByIdQuery, Result<LeadDto>>
{
    private readonly ILeadRepository _leadRepository;

    public GetLeadByIdQueryHandler(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<Result<LeadDto>> Handle(GetLeadByIdQuery request, CancellationToken ct)
    {
        var lead = await _leadRepository.GetByIdAsync(request.LeadId, ct);
        if (lead == null)
            return Result.Failure<LeadDto>("Lead não encontrado.");

        return Result.Success(LeadDto.FromEntity(lead));
    }
}

public record AddLeadNoteCommand(Guid LeadId, string Note) : IRequest<Result>;

public class AddLeadNoteCommandHandler : IRequestHandler<AddLeadNoteCommand, Result>
{
    private readonly ILeadRepository _leadRepository;

    public AddLeadNoteCommandHandler(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<Result> Handle(AddLeadNoteCommand request, CancellationToken ct)
    {
        var lead = await _leadRepository.GetByIdAsync(request.LeadId, ct);
        if (lead == null)
            return Result.Failure("Lead não encontrado.");

        lead.AddNote(request.Note);
        await _leadRepository.UpdateAsync(lead, ct);

        return Result.Success();
    }
}

public record ChangeLeadStatusCommand(Guid LeadId, string Status) : IRequest<Result>;

public class ChangeLeadStatusCommandHandler : IRequestHandler<ChangeLeadStatusCommand, Result>
{
    private readonly ILeadRepository _leadRepository;

    public ChangeLeadStatusCommandHandler(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<Result> Handle(ChangeLeadStatusCommand request, CancellationToken ct)
    {
        var lead = await _leadRepository.GetByIdAsync(request.LeadId, ct);
        if (lead == null)
            return Result.Failure("Lead não encontrado.");

        if (!Enum.TryParse<LeadStatus>(request.Status, true, out var status))
            return Result.Failure($"Status '{request.Status}' inválido. Valores aceitos: new, contacting, proposal_sent, won, lost.");

        lead.ChangeStatus(status);
        await _leadRepository.UpdateAsync(lead, ct);

        return Result.Success();
    }
}
