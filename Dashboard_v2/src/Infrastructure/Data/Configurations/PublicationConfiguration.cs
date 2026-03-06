using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class PublicationConfiguration : IEntityTypeConfiguration<Publication>
{
    public void Configure(EntityTypeBuilder<Publication> builder)
    {
        builder.ToTable("Publications");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.AuthorRelation)
            .HasMaxLength(1000);

        builder.Property(p => p.PublicationDate);

        builder.Property(p => p.PublicationTypeId)
            .IsRequired();

        builder.Property(p => p.ResourceId)
            .IsRequired();

        // Cada Publication corresponde a exactamente un Resource (1:1 obligatorio desde Publication)
        builder.HasOne(p => p.Resource)
            .WithOne()
            .HasForeignKey<Publication>(p => p.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con tipo de publicación: 0,* (publicaciones) → 1,1 (un tipo por publicación)
        builder.HasOne(p => p.PublicationType)
            .WithMany(pt => pt.Publications)
            .HasForeignKey(p => p.PublicationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ResourceId debe ser único: un Resource solo puede pertenecer a una Publication
        builder.HasIndex(p => p.ResourceId).IsUnique();
    }
}
