using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sucursal360.Web.Domain.Entities;

namespace Sucursal360.Web.Data.Configurations;

public class ReviewCategoryAssignmentConfiguration : IEntityTypeConfiguration<ReviewCategoryAssignment>
{
    public void Configure(EntityTypeBuilder<ReviewCategoryAssignment> builder)
    {
        builder.HasKey(assignment => new { assignment.ReviewId, assignment.ReviewCategoryId });

        builder.HasIndex(assignment => assignment.ReviewCategoryId)
            .HasDatabaseName("IX_ReviewCategoryAssignments_Category");

        builder.Property(assignment => assignment.AssignedByUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasOne(assignment => assignment.Review)
            .WithMany(review => review.CategoryAssignments)
            .HasForeignKey(assignment => assignment.ReviewId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(assignment => assignment.ReviewCategory)
            .WithMany(category => category.Assignments)
            .HasForeignKey(assignment => assignment.ReviewCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(assignment => assignment.AssignedByUser)
            .WithMany()
            .HasForeignKey(assignment => assignment.AssignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
