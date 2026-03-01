using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    private static readonly DateTime SeedTimestampUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CompanyId)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(x => x.Permissions)
            .HasColumnType("integer[]")
            .IsRequired();

        builder.Property(x => x.AccessLevel)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => new { x.CompanyId, x.Name })
            .IsUnique();

        builder.HasData(
            new
            {
                Id = RoleIds.CompanyOwner,
                Name = RoleNames.CompanyOwner,
                AccessLevel = 100,
                Permissions = Enum.GetValues<Permission>().ToList(),
                CompanyId = Guid.Empty,
                CreatedBy = Guid.Empty,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = SeedTimestampUtc
            },
            new
            {
                Id = RoleIds.Manager,
                Name = RoleNames.Manager,
                AccessLevel = 50,
                Permissions = new List<Permission>
                {
                    Permission.ViewClients,
                    Permission.CreateClient,
                    Permission.ViewOwnOrders,
                    Permission.ManageOrders
                },
                CompanyId = Guid.Empty,
                CreatedBy = Guid.Empty,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = SeedTimestampUtc
            },
            new
            {
                Id = RoleIds.Employee,
                Name = RoleNames.Employee,
                AccessLevel = 10,
                Permissions = new List<Permission> { Permission.ViewClients },
                CompanyId = Guid.Empty,
                CreatedBy = Guid.Empty,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = SeedTimestampUtc
            });
    }
}
