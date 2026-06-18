using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.OwnerId)
            .IsRequired()
            .HasMaxLength(450); // Same as AspNetUsers.Id

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Metadata)
            .HasColumnType("jsonb"); // PostgreSQL JSON type

        // Índices para búsquedas comunes
        builder.HasIndex(r => r.Type);
        builder.HasIndex(r => r.OwnerId);
        builder.HasIndex(r => new { r.Type, r.OwnerId });

        // Relación con Users (Owner)
        builder.HasOne(r => r.Owner)
            .WithMany(u => u.OwnedResources)
            .HasForeignKey(r => r.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
