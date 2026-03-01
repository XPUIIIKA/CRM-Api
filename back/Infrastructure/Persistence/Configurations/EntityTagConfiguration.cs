using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class EntityTagConfiguration : IEntityTypeConfiguration<EntityTag>
{
    public void Configure(EntityTypeBuilder<EntityTag> builder)
    {
        builder.ToTable("entity_tags");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(x => x.CompanyId)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(x => x.EntityId).HasColumnType("uuid");
        builder.Property(x => x.TagId).HasColumnType("uuid");

        builder.Property(x => x.EntityType)
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

        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => new { x.EntityId, x.TagId }).IsUnique();
    }
}
