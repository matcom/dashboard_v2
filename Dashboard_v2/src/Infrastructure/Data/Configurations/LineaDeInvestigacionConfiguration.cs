using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class LineaDeInvestigacionConfiguration : IEntityTypeConfiguration<LineaDeInvestigacion>
{
    public void Configure(EntityTypeBuilder<LineaDeInvestigacion> builder)
    {
        builder.ToTable("LineasDeInvestigacion");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasMaxLength(450);
        builder.Property(l => l.Nombre).IsRequired().HasMaxLength(500);
        builder.Property(l => l.Descripcion);

        // Posee: AreaDelConocimiento ↔ LineaDeInvestigacion (N:N)
        builder.HasMany(l => l.AreasDelConocimiento)
            .WithMany(a => a.LineasDeInvestigacion)
            .UsingEntity<Dictionary<string, object>>(
                "AreaDelConocimientoLineaDeInvestigacion",
                right => right
                    .HasOne<AreaDelConocimiento>()
                    .WithMany()
                    .HasForeignKey("AreaDelConocimientoId")
                    .HasPrincipalKey(a => a.Id)
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<LineaDeInvestigacion>()
                    .WithMany()
                    .HasForeignKey("LineaDeInvestigacionId")
                    .HasPrincipalKey(l => l.Id)
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("AreasDelConocimientoLineasDeInvestigacion");
                    join.HasKey("AreaDelConocimientoId", "LineaDeInvestigacionId");
                });
    }
}
