using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.RoleId)
            .IsRequired()
            .HasMaxLength(450); // Same as AspNetRoles.Id

        builder.Property(rp => rp.PermissionId)
            .IsRequired();

        builder.Property(rp => rp.ResourceType)
            .HasMaxLength(50);

        builder.Property(rp => rp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Índices para búsquedas comunes
        builder.HasIndex(rp => rp.RoleId);
        builder.HasIndex(rp => rp.PermissionId);
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId, rp.ResourceType });
        builder.HasIndex(rp => rp.ResourceType);

        // Relaciones
        builder.HasOne(rp => rp.Permission)
            .WithMany()
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Roles
        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
