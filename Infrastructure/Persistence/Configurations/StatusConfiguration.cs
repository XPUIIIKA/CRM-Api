using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class StatusConfiguration : IEntityTypeConfiguration<Status>
{
    public void Configure(EntityTypeBuilder<Status> builder)
    {
        builder.ToTable("statuses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();

        var defaultDate = DateTime.UtcNow;

        builder.HasData(
            new Status 
            { 
                Id = OrderStatusIds.Draft, 
                Name = OrderStatusNames.Draft, 
                CompanyId = Guid.Empty,
                CreatedAt = defaultDate,
                UpdatedAt = defaultDate
            },
            new Status 
            { 
                Id = OrderStatusIds.Active, 
                Name = OrderStatusNames.Active, 
                CompanyId = Guid.Empty,
                CreatedAt = defaultDate,
                UpdatedAt = defaultDate
            },
            new Status 
            { 
                Id = OrderStatusIds.Completed, 
                Name = OrderStatusNames.Completed, 
                CompanyId = Guid.Empty,
                CreatedAt = defaultDate,
                UpdatedAt = defaultDate
            },
            new Status 
            { 
                Id = OrderStatusIds.Cancelled, 
                Name = OrderStatusNames.Cancelled, 
                CompanyId = Guid.Empty,
                CreatedAt = defaultDate,
                UpdatedAt = defaultDate
            }
        );
    }
}