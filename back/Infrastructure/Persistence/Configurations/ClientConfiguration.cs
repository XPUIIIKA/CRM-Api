using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("uuid");

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Surname)
            .HasMaxLength(100);

        builder.Property(x => x.Patronymic)
            .HasMaxLength(100);

        builder.Property(x => x.Phone)
            .HasMaxLength(32);

        builder.Property(x => x.Email)
            .HasMaxLength(255);

        builder.Property(x => x.CompanyId)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => new { x.CompanyId, x.Email })
            .HasFilter("\"email\" IS NOT NULL")
            .IsUnique();
    }
}
