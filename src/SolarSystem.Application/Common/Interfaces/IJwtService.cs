namespace SolarSystem.Application.Common.Interfaces;

/// <summary>
/// Par de tokens emitido para o usuario. O refresh token e opaco (nao e um JWT)
/// e so tem valor quando persistido via <see cref="IRefreshTokenRepository"/>.
/// </summary>
public record TokenResult(string AccessToken, DateTime ExpiresAt, string RefreshToken, DateTime RefreshExpiresAt);

public interface IJwtService
{
    TokenResult GenerateTokens(Guid userId, Guid tenantId, string role);
}
