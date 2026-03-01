using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class StatusConfiguration : IEntityTypeConfiguration<Status>
{
    private static readonly DateTime SeedTimestampUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<Status> builder)
    {
        builder.ToTable("statuses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompanyId)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();

        builder.HasData(
            new Status
            {
                Id = OrderStatusIds.Draft,
                Name = OrderStatusNames.Draft,
                CompanyId = Guid.Empty,
                CreatedBy = Guid.Empty,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = SeedTimestampUtc
            },
            new Status
            {
                Id = OrderStatusIds.Active,
                Name = OrderStatusNames.Active,
                CompanyId = Guid.Empty,
                CreatedBy = Guid.Empty,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = SeedTimestampUtc
            },
            new Status
            {
                Id = OrderStatusIds.Completed,
                Name = OrderStatusNames.Completed,
                CompanyId = Guid.Empty,
                CreatedBy = Guid.Empty,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = SeedTimestampUtc
            },
            new Status
            {
                Id = OrderStatusIds.Cancelled,
                Name = OrderStatusNames.Cancelled,
                CompanyId = Guid.Empty,
                CreatedBy = Guid.Empty,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = SeedTimestampUtc
            });
    }
}
