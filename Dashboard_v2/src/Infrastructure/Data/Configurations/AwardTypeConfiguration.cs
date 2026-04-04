using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class AwardTypeConfiguration : IEntityTypeConfiguration<AwardType>
{
    public void Configure(EntityTypeBuilder<AwardType> builder)
    {
        builder.ToTable("AwardTypes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
    }
}
