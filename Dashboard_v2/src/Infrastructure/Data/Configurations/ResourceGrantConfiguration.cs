using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class ResourceGrantConfiguration : IEntityTypeConfiguration<ResourceGrant>
{
    public void Configure(EntityTypeBuilder<ResourceGrant> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.UserId)
            .IsRequired()
            .HasMaxLength(450); // Same as AspNetUsers.Id

        builder.Property(g => g.ResourceId)
            .IsRequired();

        builder.Property(g => g.PermissionId)
            .IsRequired();

        builder.Property(g => g.GrantedBy)
            .HasMaxLength(450);

        builder.Property(g => g.GrantedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(g => g.ExpiresAt);

        builder.Property(g => g.FieldsAllowed)
            .HasColumnType("jsonb"); // PostgreSQL JSON type

        builder.Property(g => g.Conditions)
            .HasColumnType("jsonb");

        builder.Property(g => g.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Índices para búsquedas comunes
        builder.HasIndex(g => g.UserId);
        builder.HasIndex(g => g.ResourceId);
        builder.HasIndex(g => g.PermissionId);
        builder.HasIndex(g => new { g.UserId, g.ResourceId, g.PermissionId });
        builder.HasIndex(g => g.ExpiresAt)
            .HasFilter("\"ExpiresAt\" IS NOT NULL");

        // Relaciones
        builder.HasOne(g => g.Resource)
            .WithMany(r => r.Grants)
            .HasForeignKey(g => g.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(g => g.Permission)
            .WithMany(p => p.Grants)
            .HasForeignKey(g => g.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Users (User que recibe el grant)
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Users (User que otorgó el grant)
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(g => g.GrantedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
