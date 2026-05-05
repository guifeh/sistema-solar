using SolarSystem.Domain.Identity;

namespace SolarSystem.Application.Common.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task RevokeAsync(string token, CancellationToken ct = default);
}
