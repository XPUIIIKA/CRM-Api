using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("uuid");
        builder.Property(x => x.ClientId).HasColumnType("uuid");
        builder.Property(x => x.CompanyId).HasColumnType("uuid");
        builder.Property(x => x.CreatedBy).HasColumnType("uuid");
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");
        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Navigation(x => x.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => x.ClientId);
    }
}

public class OrderProductConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_products");

        builder.Property(x => x.Price)
            .HasColumnType("numeric(18,2)")
            .IsRequired();
        
        builder.HasKey(x => new { x.OrderId, x.ProductId });
        builder.Property(x => x.OrderId).HasColumnType("uuid");
        builder.Property(x => x.ProductId).HasColumnType("uuid");
        builder.Property(x => x.Quantity).IsRequired();
    }
}

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("order_status_histories");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("uuid");
        builder.Property(x => x.OrderId).HasColumnType("uuid");
        builder.Property(x => x.StatusId).HasColumnType("uuid");
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");
    }
}