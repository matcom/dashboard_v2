using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class PublicationTypeConfiguration : IEntityTypeConfiguration<PublicationType>
{
    public void Configure(EntityTypeBuilder<PublicationType> builder)
    {
        builder.ToTable("PublicationTypes");

        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(pt => pt.Name)
            .IsUnique();
    }
}
