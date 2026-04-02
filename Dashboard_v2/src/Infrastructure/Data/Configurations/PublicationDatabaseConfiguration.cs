using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class PublicationDatabaseConfiguration : IEntityTypeConfiguration<PublicationDatabase>
{
    public void Configure(EntityTypeBuilder<PublicationDatabase> builder)
    {
        builder.ToTable("PublicationDatabases");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasMaxLength(450);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Url).HasMaxLength(1000);
        builder.Property(d => d.JournalPublicationId).IsRequired().HasMaxLength(450);

        builder.HasOne(d => d.JournalPublication)
            .WithMany(jp => jp.Databases)
            .HasForeignKey(d => d.JournalPublicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
