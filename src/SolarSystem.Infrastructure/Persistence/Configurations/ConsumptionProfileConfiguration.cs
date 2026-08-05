using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarSystem.Domain.Dimensioning;

namespace SolarSystem.Infrastructure.Persistence.Configurations;

public class ConsumptionProfileConfiguration : IEntityTypeConfiguration<ConsumptionProfile>
{
    public void Configure(EntityTypeBuilder<ConsumptionProfile> builder)
    {
        builder.ToTable("consumption_profiles");

        builder.HasKey(p => p.Id);

        // Gravado como texto: a tabela e consultada direto em analise de dados, e int
        // exigiria consultar o enum no codigo para entender a linha.
        builder.Property(p => p.PropertyType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(p => p.StateGroup)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(p => p.NumRooms);
        builder.Property(p => p.HasAc).HasDefaultValue(false);
        builder.Property(p => p.HasWaterHeater).HasDefaultValue(false);
        builder.Property(p => p.HasPool).HasDefaultValue(false);

        builder.Property(p => p.ConsumptionMin).IsRequired();
        builder.Property(p => p.ConsumptionMax).IsRequired();
        builder.Property(p => p.ConsumptionAvg).IsRequired();

        builder.HasIndex(p => new { p.PropertyType, p.StateGroup });
    }
}
