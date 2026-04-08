using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class AwardsConfiguration : IEntityTypeConfiguration<Award>
{
    public void Configure(EntityTypeBuilder<Award> builder)
    {
        builder.ToTable("Awards");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(300);

        builder.HasOne(a => a.AwardType)
            .WithMany(t => t.Awards)
            .HasForeignKey(a => a.AwardTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
