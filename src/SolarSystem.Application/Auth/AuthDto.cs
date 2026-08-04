namespace SolarSystem.Application.Auth;

public record AuthUserDto(
    Guid Id,
    string Email,
    string Name,
    string Role,
    Guid TenantId,
    string TenantName);

public record AuthDto(
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken,
    AuthUserDto? User = null);
