using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Common;
using SolarSystem.Domain.Dimensioning;
using SolarSystem.Domain.Identity;
using SolarSystem.Domain.Leads;

namespace SolarSystem.Infrastructure.Persistence;

public class SolarDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUser;

    /// <summary>
    /// Tenant do request atual. Guid.Empty quando nao ha usuario autenticado — nesse caso
    /// o filtro global nao casa com nenhuma linha, que e o padrao seguro.
    /// </summary>
    private Guid CurrentTenantId => _currentUser?.TenantId ?? Guid.Empty;

    /// <summary>Usado em design-time (migrations) e em testes que montam o contexto na mao.</summary>
    public SolarDbContext(DbContextOptions<SolarDbContext> options) : base(options)
    {
    }

    public SolarDbContext(DbContextOptions<SolarDbContext> options, ICurrentUserService currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    // Identity
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Global
    public DbSet<IrradiationByUf> IrradiationByUfs => Set<IrradiationByUf>();

    // Leads
    public DbSet<Lead> Leads => Set<Lead>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SolarDbContext).Assembly);

        var applyFilter = typeof(SolarDbContext)
            .GetMethod(nameof(ApplyTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
                applyFilter.MakeGenericMethod(entityType.ClrType).Invoke(this, new object[] { modelBuilder });
        }
    }

    /// <summary>
    /// O filtro referencia a propriedade da instancia (nao uma constante), entao o EF gera um
    /// parametro por consulta. Isso e o que permite o model ser cacheado uma unica vez sem
    /// congelar o tenant do primeiro request para todos os demais.
    /// </summary>
    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : TenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
    }

    public override int SaveChanges()
    {
        ApplyAuditAndTenant();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditAndTenant();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditAndTenant()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Entity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (Entity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;

                // Rede de seguranca: entidade multi-tenant que chegou sem tenant herda o do
                // request, para nunca gravar linha orfa com Guid.Empty.
                if (entity is TenantEntity tenantEntity && tenantEntity.TenantId == Guid.Empty)
                    entry.Property(nameof(TenantEntity.TenantId)).CurrentValue = CurrentTenantId;
            }
            else
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
