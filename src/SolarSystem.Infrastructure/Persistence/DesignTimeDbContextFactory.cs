using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SolarSystem.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SolarDbContext>
{
    public SolarDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Port=5433;Database=solar;Username=solaruser;Password=solarpass";

        var optionsBuilder = new DbContextOptionsBuilder<SolarDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new SolarDbContext(optionsBuilder.Options);
    }
}
