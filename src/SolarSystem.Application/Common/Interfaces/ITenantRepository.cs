using SolarSystem.Domain.Identity;

namespace SolarSystem.Application.Common.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Tenant> CreateAsync(Tenant tenant, CancellationToken ct = default);
}
