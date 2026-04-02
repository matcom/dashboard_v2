using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class JournalConfiguration : IEntityTypeConfiguration<Journal>
{
    public void Configure(EntityTypeBuilder<Journal> builder)
    {
        builder.ToTable("Journals");

        builder.HasKey(j => j.Id);
        builder.Property(j => j.Id).HasMaxLength(450);
        builder.Property(j => j.Name).IsRequired().HasMaxLength(500);
        builder.Property(j => j.ISSN).HasMaxLength(20);
        builder.Property(j => j.EISSN).HasMaxLength(20);
        builder.Property(j => j.JournalPublicationId).IsRequired().HasMaxLength(450);

        builder.HasOne(j => j.JournalPublication)
            .WithMany(jp => jp.Journals)
            .HasForeignKey(j => j.JournalPublicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
