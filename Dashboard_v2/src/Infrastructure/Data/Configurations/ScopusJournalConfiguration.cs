using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class ScopusJournalConfiguration : IEntityTypeConfiguration<ScopusJournal>
{
    public void Configure(EntityTypeBuilder<ScopusJournal> builder)
    {
        builder.ToTable("ScopusJournals");

        builder.HasKey(sj => sj.JournalId);
        builder.Property(sj => sj.JournalId).HasMaxLength(450);
        builder.Property(sj => sj.Cuartil).IsRequired();

        builder.HasOne(sj => sj.Journal)
            .WithOne(j => j.ScopusJournal)
            .HasForeignKey<ScopusJournal>(sj => sj.JournalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
