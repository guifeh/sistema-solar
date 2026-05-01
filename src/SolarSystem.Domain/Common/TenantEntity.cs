namespace SolarSystem.Domain.Common;

public abstract class TenantEntity : Entity, ITenantScoped
{
    public Guid TenantId { get; protected set; }
}
