using Microsoft.EntityFrameworkCore;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Identity;

namespace SolarSystem.Infrastructure.Persistence.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly SolarDbContext _context;

    public TenantRepository(SolarDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<Tenant> CreateAsync(Tenant tenant, CancellationToken ct = default)
    {
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(ct);
        return tenant;
    }
}
