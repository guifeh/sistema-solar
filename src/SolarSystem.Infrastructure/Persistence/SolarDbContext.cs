using Microsoft.EntityFrameworkCore;
using SolarSystem.Domain.Common;
using SolarSystem.Domain.Dimensioning;
using SolarSystem.Domain.Identity;
using SolarSystem.Domain.Leads;

namespace SolarSystem.Infrastructure.Persistence;

public class SolarDbContext : DbContext
{
    private readonly Guid? _currentTenantId;

    public SolarDbContext(DbContextOptions<SolarDbContext> options) : base(options)
    {
    }

    public SolarDbContext(DbContextOptions<SolarDbContext> options, Guid? currentTenantId) : base(options)
    {
        _currentTenantId = currentTenantId;
    }

    // Identity
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();

    // Global
    public DbSet<IrradiationByUf> IrradiationByUfs => Set<IrradiationByUf>();

    // Leads
    public DbSet<Lead> Leads => Set<Lead>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SolarDbContext).Assembly);

        if (_currentTenantId.HasValue)
        {
            var tenantId = _currentTenantId.Value;
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    var property = System.Linq.Expressions.Expression.Property(parameter, nameof(TenantEntity.TenantId));
                    var constant = System.Linq.Expressions.Expression.Constant(tenantId);
                    var filter = System.Linq.Expressions.Expression.Equal(property, constant);
                    var lambda = System.Linq.Expressions.Expression.Lambda(filter, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Entity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (Entity)entry.Entity;
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
