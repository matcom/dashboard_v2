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
            .HasMaxLength(450);

        // Índices para búsquedas comunes del sistema de permisos
        builder.HasIndex(r => r.Type);
        builder.HasIndex(r => r.OwnerId);
        builder.HasIndex(r => new { r.Type, r.OwnerId });

        // Relación con Users (Owner)
        builder.HasOne<User>()
            .WithMany(u => u.OwnedResources)
            .HasForeignKey(r => r.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
