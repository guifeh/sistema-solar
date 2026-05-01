using SolarSystem.Domain.Common;

namespace SolarSystem.Domain.Identity;

public class Tenant : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Domain { get; private set; }
    public string? LogoUrl { get; private set; }
    public string Settings { get; private set; } = "{}";
    public bool IsActive { get; private set; } = true;

    private Tenant() { }

    public static Tenant Create(string name, string? domain = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do tenant é obrigatório.");

        return new Tenant
        {
            Name = name,
            Domain = domain
        };
    }
}
