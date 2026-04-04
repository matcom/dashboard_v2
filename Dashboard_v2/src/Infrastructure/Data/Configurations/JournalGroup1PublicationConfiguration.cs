using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class JournalGroup1PublicationConfiguration : IEntityTypeConfiguration<JournalGroup1Publication>
{
    public void Configure(EntityTypeBuilder<JournalGroup1Publication> builder)
    {
        builder.ToTable("JournalGroup1Publications");

        builder.HasKey(g1 => g1.PublicationId);
        builder.Property(g1 => g1.PublicationId).HasMaxLength(450);
        builder.Property(g1 => g1.Cuartil).IsRequired().HasMaxLength(10);

        builder.HasOne(g1 => g1.JournalPublication)
            .WithOne(jp => jp.JournalGroup1Publication)
            .HasForeignKey<JournalGroup1Publication>(g1 => g1.PublicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
