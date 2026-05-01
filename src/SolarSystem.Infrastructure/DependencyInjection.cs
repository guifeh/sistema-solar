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
        services.AddDbContext<SolarDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("Default");
            options.UseNpgsql(connectionString);
        });

        // Temporary dev implementation — replace with JWT-based service in EP-08
        services.AddScoped<ICurrentUserService, DevCurrentUserService>();

        // Repositories
        services.AddScoped<ILeadRepository, LeadRepository>();

        return services;
    }
}
