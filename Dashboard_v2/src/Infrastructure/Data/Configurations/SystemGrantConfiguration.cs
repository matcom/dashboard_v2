using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class SystemGrantConfiguration : IEntityTypeConfiguration<SystemGrant>
{
    public void Configure(EntityTypeBuilder<SystemGrant> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(g => g.Permission)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(g => g.GrantedBy)
            .HasMaxLength(450);

        builder.Property(g => g.GrantedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(g => g.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Índices
        builder.HasIndex(g => g.UserId);
        builder.HasIndex(g => new { g.UserId, g.Permission });
        builder.HasIndex(g => g.ExpiresAt)
            .HasFilter("\"ExpiresAt\" IS NOT NULL");

        // Relación con User
        builder.HasOne(g => g.User)
            .WithMany(u => u.SystemGrants)
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
