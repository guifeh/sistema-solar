using Microsoft.EntityFrameworkCore;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Identity;

namespace SolarSystem.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly SolarDbContext _context;

    public RefreshTokenRepository(SolarDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);
    }

    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        _context.Set<RefreshToken>().Add(refreshToken);
        await _context.SaveChangesAsync(ct);
        return refreshToken;
    }

    public async Task RevokeAsync(string token, CancellationToken ct = default)
    {
        var refreshToken = await GetByTokenAsync(token, ct);
        if (refreshToken != null)
        {
            refreshToken.Revoke();
            await _context.SaveChangesAsync(ct);
        }
    }
}
