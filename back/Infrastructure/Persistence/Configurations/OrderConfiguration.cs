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
        builder.Property(x => x.CurrentStatusId).HasColumnType("uuid");
        builder.Property(x => x.AssignedManagerId).HasColumnType("uuid");
        builder.Property(x => x.CompanyId).HasColumnType("uuid");
        builder.Property(x => x.CreatedBy).HasColumnType("uuid");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Navigation(x => x.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.AssignedManagerId);
    }
}
