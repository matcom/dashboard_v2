using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class RedConfiguration : IEntityTypeConfiguration<Red>
{
    public void Configure(EntityTypeBuilder<Red> builder)
    {
        builder.ToTable("Reds");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasMaxLength(450);
        builder.Property(r => r.Nombre).IsRequired().HasMaxLength(500);
        builder.Property(r => r.CantidadProfesores);

        // País (FK nullable)
        builder.Property(r => r.CountryId);
        builder.HasOne(r => r.Country)
            .WithMany(c => c.Reds)
            .HasForeignKey(r => r.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Eventos coordinados por la red
        builder.HasMany(r => r.Events)
            .WithOne(e => e.Red)
            .HasForeignKey(e => e.RedId)
            .OnDelete(DeleteBehavior.SetNull);

        // Miembros: Red ↔ User (N:N)
        builder.HasMany(r => r.Usuarios)
            .WithMany(u => u.Redes)
            .UsingEntity<Dictionary<string, object>>(
                "RedUsuario",
                right => right
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey("UsuarioId")
                    .HasPrincipalKey(u => u.Id)
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Red>()
                    .WithMany()
                    .HasForeignKey("RedId")
                    .HasPrincipalKey(r => r.Id)
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("RedsUsuarios");
                    join.HasKey("RedId", "UsuarioId");
                });
    }
}
