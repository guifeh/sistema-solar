using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Infrastructure.Persistence;
using SolarSystem.Infrastructure.Persistence.Repositories;
using SolarSystem.Infrastructure.Services;

namespace SolarSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // O ICurrentUserService entra pelo construtor do contexto e alimenta o filtro
        // global de tenant — precisa estar registrado antes do DbContext.
        services.AddDbContext<SolarDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"));
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Repositories
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IIrradiationRepository, IrradiationRepository>();
        services.AddScoped<IConsumptionProfileRepository, ConsumptionProfileRepository>();

        return services;
    }
}
