using Microsoft.EntityFrameworkCore;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Dimensioning;

namespace SolarSystem.Infrastructure.Persistence.Repositories;

/// <summary>
/// Dado de referencia global (nao multi-tenant): a irradiacao de SP e a mesma para todos
/// os integradores, entao nao ha filtro por tenant aqui.
/// </summary>
public class IrradiationRepository : IIrradiationRepository
{
    private readonly SolarDbContext _context;

    public IrradiationRepository(SolarDbContext context)
    {
        _context = context;
    }

    public async Task<IrradiationByUf?> GetByUfAsync(string uf, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(uf))
            return null;

        var normalized = uf.Trim().ToUpperInvariant();

        return await _context.IrradiationByUfs
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Uf == normalized, ct);
    }

    public async Task<IReadOnlyList<IrradiationByUf>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.IrradiationByUfs
            .AsNoTracking()
            .OrderBy(i => i.Uf)
            .ToListAsync(ct);
    }
}

public class ConsumptionProfileRepository : IConsumptionProfileRepository
{
    private readonly SolarDbContext _context;

    public ConsumptionProfileRepository(SolarDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ConsumptionProfile>> GetCandidatesAsync(
        PropertyType propertyType, StateGroup stateGroup, CancellationToken ct = default)
    {
        return await _context.ConsumptionProfiles
            .AsNoTracking()
            .Where(p => p.PropertyType == propertyType && p.StateGroup == stateGroup)
            .ToListAsync(ct);
    }
}
