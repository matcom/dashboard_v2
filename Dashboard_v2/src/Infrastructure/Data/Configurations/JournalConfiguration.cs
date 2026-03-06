using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

/// <summary>
/// Especialización TPT: Journals es una tabla aparte con FK = PK a Publications.
/// Modela la especialización no disjunta del MERX (pub_id punteado = llave heredada).
/// </summary>
public class JournalConfiguration : IEntityTypeConfiguration<Journal>
{
    public void Configure(EntityTypeBuilder<Journal> builder)
    {
        builder.ToTable("Journals");

        // PublicationId es PK y FK — llave heredada (pub_id punteado en el MERX)
        builder.HasKey(j => j.PublicationId);

        builder.Property(j => j.Database)
            .HasMaxLength(200);

        builder.Property(j => j.GroupName)
            .HasMaxLength(200);

        builder.Property(j => j.Quartile)
            .HasMaxLength(10);

        // 1:1 con Publication (una Publication puede ser 0 o 1 Revista)
        builder.HasOne(j => j.Publication)
            .WithOne(p => p.Journal)
            .HasForeignKey<Journal>(j => j.PublicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
