using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("order_status_histories");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("uuid");
        builder.Property(x => x.CompanyId).HasColumnType("uuid");
        builder.Property(x => x.OrderId).HasColumnType("uuid");
        builder.Property(x => x.StatusId).HasColumnType("uuid");
        builder.Property(x => x.CreatedBy).HasColumnType("uuid");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => x.CompanyId);
    }
}
