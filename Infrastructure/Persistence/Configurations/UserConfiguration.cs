using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("uuid");

        builder.Property(x => x.CompanyId).HasColumnType("uuid");
        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(200);

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(512);
        builder.Property(x => x.CreatedBy).HasColumnType("uuid");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");
        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => new { x.CompanyId, x.Email }).IsUnique();
    }
}
