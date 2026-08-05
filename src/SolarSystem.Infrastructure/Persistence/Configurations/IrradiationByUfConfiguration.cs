using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarSystem.Domain.Dimensioning;

namespace SolarSystem.Infrastructure.Persistence.Configurations;

public class IrradiationByUfConfiguration : IEntityTypeConfiguration<IrradiationByUf>
{
    public void Configure(EntityTypeBuilder<IrradiationByUf> builder)
    {
        builder.ToTable("irradiation_by_uf");

        builder.HasKey(i => i.Uf);

        builder.Property(i => i.Uf)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(i => i.StateName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.AverageIrradiation)
            .IsRequired()
            .HasColumnType("decimal(6,4)");

        builder.Property(i => i.Source)
            .HasMaxLength(100);

        builder.Property(i => i.UpdatedAt)
            .IsRequired();
    }
}
