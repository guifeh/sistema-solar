using SolarSystem.Domain.Leads;

namespace SolarSystem.Application.Common.Interfaces;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Lead> Leads, int Total)> GetByTenantAsync(
        Guid tenantId, LeadFilter filter, CancellationToken ct = default);
    Task<Lead> CreateAsync(Lead lead, CancellationToken ct = default);
    Task<Lead> UpdateAsync(Lead lead, CancellationToken ct = default);
}

public class LeadFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public LeadStatus? Status { get; set; }
    public string? Uf { get; set; }
    public LeadType? Type { get; set; }
    public string? Search { get; set; }
}
