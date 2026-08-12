using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sucursal360.Web.Domain.Entities;

namespace Sucursal360.Web.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(review => review.ExternalReviewId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(review => review.Text)
            .HasMaxLength(4000);

        builder.Property(review => review.AuthorDisplayName)
            .HasMaxLength(120);

        builder.Property(review => review.Language)
            .HasMaxLength(10);

        builder.Property(review => review.SourceUrl)
            .HasMaxLength(1000);

        builder.HasIndex(review => new { review.Provider, review.ExternalReviewId })
            .IsUnique()
            .HasDatabaseName("UX_Reviews_Provider_ExternalId");

        builder.HasIndex(review => new { review.BranchId, review.PublishedAtUtc })
            .HasDatabaseName("IX_Reviews_Branch_Published");

        builder.HasIndex(review => new { review.BranchId, review.Rating })
            .HasDatabaseName("IX_Reviews_Branch_Rating");

        builder.HasOne(review => review.Branch)
            .WithMany(branch => branch.Reviews)
            .HasForeignKey(review => review.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
            table.HasCheckConstraint("CK_Reviews_Rating", "Rating IS NULL OR Rating BETWEEN 1 AND 5"));
    }
}
