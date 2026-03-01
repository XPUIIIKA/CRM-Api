using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_products", t =>
        {
            t.HasCheckConstraint("ck_order_products_price_non_negative", "\"price\" >= 0");
            t.HasCheckConstraint("ck_order_products_quantity_positive", "\"quantity\" > 0");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(x => x.CompanyId)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(x => x.OrderId).HasColumnType("uuid");
        builder.Property(x => x.ProductId).HasColumnType("uuid");

        builder.Property(x => x.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => new { x.OrderId, x.ProductId }).IsUnique();
    }
}
