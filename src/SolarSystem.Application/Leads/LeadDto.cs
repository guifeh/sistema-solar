using SolarSystem.Domain.Leads;

namespace SolarSystem.Application.Leads;

public record LeadDto(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    string? City,
    string? Uf,
    string LeadType,
    string LeadSource,
    string Status,
    string? Notes,
    int? ConsumptionEstimate,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public static LeadDto FromEntity(Lead lead) => new(
        lead.Id,
        lead.Name,
        lead.Phone,
        lead.Email,
        lead.City,
        lead.Uf,
        lead.LeadType.ToString().ToLowerInvariant(),
        lead.LeadSource.ToString().ToLowerInvariant(),
        lead.Status.ToString().ToLowerInvariant(),
        lead.Notes,
        lead.ConsumptionEstimate,
        lead.CreatedAt,
        lead.UpdatedAt
    );
}
