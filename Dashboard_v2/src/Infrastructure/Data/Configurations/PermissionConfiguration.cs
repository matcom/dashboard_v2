using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.ResourceType)
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        // Índice único en Name para evitar duplicados
        builder.HasIndex(p => p.Name)
            .IsUnique();

        // Índice para búsquedas por tipo de recurso
        builder.HasIndex(p => p.ResourceType);
    }
}
