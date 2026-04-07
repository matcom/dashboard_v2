using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class GrupoDeInvestigacionConfiguration : IEntityTypeConfiguration<GrupoDeInvestigacion>
{
    public void Configure(EntityTypeBuilder<GrupoDeInvestigacion> builder)
    {
        builder.ToTable("GruposDeInvestigacion");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasMaxLength(450);
        builder.Property(g => g.Nombre).IsRequired().HasMaxLength(500);
        builder.Property(g => g.AreaId).IsRequired().HasMaxLength(450);

        // Posee: GrupoDeInvestigacion → Area (1,1)
        builder.HasOne(g => g.Area)
            .WithMany(a => a.GruposDeInvestigacion)
            .HasForeignKey(g => g.AreaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Estudia: GrupoDeInvestigacion ↔ LineaDeInvestigacion (0:N)
        builder.HasMany(g => g.LineasDeInvestigacion)
            .WithMany(l => l.GruposDeInvestigacion)
            .UsingEntity<Dictionary<string, object>>(
                "GrupoDeInvestigacionLineaDeInvestigacion",
                right => right
                    .HasOne<LineaDeInvestigacion>()
                    .WithMany()
                    .HasForeignKey("LineaDeInvestigacionId")
                    .HasPrincipalKey(l => l.Id)
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<GrupoDeInvestigacion>()
                    .WithMany()
                    .HasForeignKey("GrupoDeInvestigacionId")
                    .HasPrincipalKey(g => g.Id)
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("GruposDeInvestigacionLineasDeInvestigacion");
                    join.HasKey("GrupoDeInvestigacionId", "LineaDeInvestigacionId");
                });
    }
}
