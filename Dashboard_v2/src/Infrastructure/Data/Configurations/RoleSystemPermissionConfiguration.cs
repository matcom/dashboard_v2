using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class RoleSystemPermissionConfiguration : IEntityTypeConfiguration<RoleSystemPermission>
{
    public void Configure(EntityTypeBuilder<RoleSystemPermission> builder)
    {
        builder.HasKey(rsp => rsp.Id);

        builder.Property(rsp => rsp.RoleId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(rsp => rsp.Permission)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(rsp => rsp.GrantedBy)
            .HasMaxLength(450);

        builder.Property(rsp => rsp.GrantedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(rsp => rsp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(rsp => new { rsp.RoleId, rsp.Permission })
            .IsUnique()
            .HasFilter("\"IsActive\" = true");

        builder.HasOne(rsp => rsp.Role)
            .WithMany(r => r.SystemPermissions)
            .HasForeignKey(rsp => rsp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
