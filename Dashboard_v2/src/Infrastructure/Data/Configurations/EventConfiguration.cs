using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Institutions).HasColumnType("text[]");
        builder.Property(e => e.EventType).IsRequired();

        builder.HasOne(e => e.Country)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
