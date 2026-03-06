using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasMaxLength(450);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(256);

        builder.HasIndex(r => r.Name).IsUnique().HasDatabaseName("RoleNameIndex");
    }
}
