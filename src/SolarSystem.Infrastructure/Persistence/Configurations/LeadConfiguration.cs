using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarSystem.Domain.Leads;

namespace SolarSystem.Infrastructure.Persistence.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("leads");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(l => l.Email)
            .HasMaxLength(200);

        builder.Property(l => l.City)
            .HasMaxLength(100);

        builder.Property(l => l.Uf)
            .HasMaxLength(2);

        builder.Property(l => l.Notes)
            .HasColumnType("text");

        builder.Property(l => l.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(l => l.LeadType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(l => l.LeadSource)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}
