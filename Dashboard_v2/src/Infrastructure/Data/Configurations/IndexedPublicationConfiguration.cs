using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

/// <summary>
/// Especialización TPT: IndexedPublications es una tabla aparte con FK = PK a Publications.
/// Modela la especialización no disjunta del MERX (pub_id punteado = llave heredada).
/// </summary>
public class IndexedPublicationConfiguration : IEntityTypeConfiguration<IndexedPublication>
{
    public void Configure(EntityTypeBuilder<IndexedPublication> builder)
    {
        builder.ToTable("IndexedPublications");

        // PublicationId es PK y FK — llave heredada (pub_id punteado en el MERX)
        builder.HasKey(ip => ip.PublicationId);

        builder.Property(ip => ip.IndexName)
            .HasMaxLength(200);

        // 1:1 con Publication (una Publication puede ser 0 o 1 Publicación Indexada)
        builder.HasOne(ip => ip.Publication)
            .WithOne(p => p.IndexedPublication)
            .HasForeignKey<IndexedPublication>(ip => ip.PublicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
