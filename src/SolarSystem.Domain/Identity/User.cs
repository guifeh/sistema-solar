using SolarSystem.Domain.Common;

namespace SolarSystem.Domain.Identity;

public class User : TenantEntity
{
    public string Email { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private User() { }

    public static User Create(Guid tenantId, string email, string name, string passwordHash, string role)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("E-mail é obrigatório.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Senha é obrigatória.");

        return new User
        {
            TenantId = tenantId,
            Email = email,
            Name = name,
            PasswordHash = passwordHash,
            Role = role
        };
    }
}
