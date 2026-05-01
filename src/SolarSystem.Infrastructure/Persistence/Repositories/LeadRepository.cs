using Microsoft.EntityFrameworkCore;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Leads;

namespace SolarSystem.Infrastructure.Persistence.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly SolarDbContext _context;

    public LeadRepository(SolarDbContext context)
    {
        _context = context;
    }

    public async Task<Lead?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    public async Task<(IReadOnlyList<Lead> Leads, int Total)> GetByTenantAsync(
        Guid tenantId, LeadFilter filter, CancellationToken ct = default)
    {
        var query = _context.Leads
            .AsNoTracking()
            .Where(l => l.TenantId == tenantId);

        if (filter.Status.HasValue)
            query = query.Where(l => l.Status == filter.Status.Value);

        if (!string.IsNullOrWhiteSpace(filter.Uf))
            query = query.Where(l => l.Uf == filter.Uf);

        if (filter.Type.HasValue)
            query = query.Where(l => l.LeadType == filter.Type.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(l =>
                l.Name.ToLower().Contains(search) ||
                (l.Email != null && l.Email.ToLower().Contains(search)) ||
                (l.City != null && l.City.ToLower().Contains(search)));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<Lead> CreateAsync(Lead lead, CancellationToken ct = default)
    {
        _context.Leads.Add(lead);
        await _context.SaveChangesAsync(ct);
        return lead;
    }

    public async Task<Lead> UpdateAsync(Lead lead, CancellationToken ct = default)
    {
        _context.Leads.Update(lead);
        await _context.SaveChangesAsync(ct);
        return lead;
    }
}
