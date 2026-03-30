using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard_v2.Infrastructure.Data.Configurations;

public class AuthorPresentationConfiguration : IEntityTypeConfiguration<AuthorPresentation>
{
    public void Configure(EntityTypeBuilder<AuthorPresentation> builder)
    {
        builder.ToTable("AuthorPresentations");

        builder.HasKey(ap => new { ap.AuthorId, ap.PresentationId });

        builder.Property(ap => ap.AuthorId).HasMaxLength(450);

        builder.HasOne(ap => ap.Author)
            .WithMany(a => a.AuthorPresentations)
            .HasForeignKey(ap => ap.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ap => ap.Presentation)
            .WithMany(p => p.AuthorPresentations)
            .HasForeignKey(ap => ap.PresentationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
