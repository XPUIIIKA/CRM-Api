using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("uuid");
        builder.Property(x => x.CompanyId).HasColumnType("uuid");
        builder.Property(x => x.DialogId).HasColumnType("uuid");
        builder.Property(x => x.SenderUserId).HasColumnType("uuid");
        builder.Property(x => x.ReceiverUserId).HasColumnType("uuid");
        builder.Property(x => x.Text)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.HasOne<Dialog>()
            .WithMany()
            .HasForeignKey(x => x.DialogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => new { x.DialogId, x.CreatedAt });
        builder.HasIndex(x => new { x.ReceiverUserId, x.IsRead });
    }
}
