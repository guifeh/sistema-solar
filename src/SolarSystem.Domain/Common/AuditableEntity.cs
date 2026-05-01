namespace SolarSystem.Domain.Common;

public abstract class AuditableEntity : TenantEntity
{
    public Guid CreatedBy { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }
}
