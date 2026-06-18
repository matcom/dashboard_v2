using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class PresentationConfiguration : IEntityTypeConfiguration<Presentation>
{
    public void Configure(EntityTypeBuilder<Presentation> builder)
    {
        builder.ToTable("Presentations");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(500);

        builder.HasOne(p => p.Event)
            .WithMany(e => e.Presentations)
            .HasForeignKey(p => p.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
