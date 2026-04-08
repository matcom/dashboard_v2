using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class AreaDelConocimientoConfiguration : IEntityTypeConfiguration<AreaDelConocimiento>
{
    public void Configure(EntityTypeBuilder<AreaDelConocimiento> builder)
    {
        builder.ToTable("AreasDelConocimiento");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasMaxLength(450);
        builder.Property(a => a.Nombre).IsRequired().HasMaxLength(500);
        builder.Property(a => a.Descripcion);

        // Investiga sobre: Area ↔ AreaDelConocimiento (1:N)
        builder.HasMany(a => a.Areas)
            .WithMany(area => area.AreasDelConocimiento)
            .UsingEntity<Dictionary<string, object>>(
                "AreaInvestigaAreaDelConocimiento",
                right => right
                    .HasOne<Area>()
                    .WithMany()
                    .HasForeignKey("AreaId")
                    .HasPrincipalKey(a => a.Id)
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<AreaDelConocimiento>()
                    .WithMany()
                    .HasForeignKey("AreaDelConocimientoId")
                    .HasPrincipalKey(a => a.Id)
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("AreasInvestiganAreasDelConocimiento");
                    join.HasKey("AreaId", "AreaDelConocimientoId");
                });
    }
}
