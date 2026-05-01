using SolarSystem.Application.Common.Interfaces;

namespace SolarSystem.Infrastructure.Services;

/// <summary>
/// Temporary mock implementation for development until EP-08 (Auth/JWT) is implemented.
/// Returns a fixed dev tenant and user so endpoints can be tested without authentication.
/// </summary>
public class DevCurrentUserService : ICurrentUserService
{
    public Guid UserId { get; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public Guid TenantId { get; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public string Role => "admin";
    public bool IsAuthenticated => true;
}
