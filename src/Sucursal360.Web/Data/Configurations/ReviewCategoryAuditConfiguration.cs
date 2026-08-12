using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sucursal360.Web.Domain.Entities;

namespace Sucursal360.Web.Data.Configurations;

public class ReviewCategoryAuditConfiguration : IEntityTypeConfiguration<ReviewCategoryAudit>
{
    public void Configure(EntityTypeBuilder<ReviewCategoryAudit> builder)
    {
        builder.Property(audit => audit.ChangedByUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasIndex(audit => new { audit.ReviewId, audit.ChangedAtUtc })
            .HasDatabaseName("IX_ReviewCategoryAudits_Review_Date");

        builder.HasOne(audit => audit.Review)
            .WithMany(review => review.CategoryAudits)
            .HasForeignKey(audit => audit.ReviewId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(audit => audit.ReviewCategory)
            .WithMany(category => category.Audits)
            .HasForeignKey(audit => audit.ReviewCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(audit => audit.ChangedByUser)
            .WithMany()
            .HasForeignKey(audit => audit.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
