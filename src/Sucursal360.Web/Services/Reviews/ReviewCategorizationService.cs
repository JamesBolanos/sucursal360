using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Domain.Entities;
using Sucursal360.Web.Domain.Enums;

namespace Sucursal360.Web.Services.Reviews;

public sealed class ReviewCategorizationService(ApplicationDbContext dbContext) : IReviewCategorizationService
{
    public async Task ReplaceCategoriesAsync(
        Guid reviewId,
        IReadOnlyCollection<Guid> selectedCategoryIds,
        string changedByUserId,
        CancellationToken cancellationToken)
    {
        var normalizedSelectedIds = selectedCategoryIds
            .Distinct()
            .ToHashSet();

        var reviewExists = await dbContext.Reviews.AnyAsync(review => review.Id == reviewId, cancellationToken);
        if (!reviewExists)
        {
            throw new InvalidOperationException("Review was not found.");
        }

        var activeCategoryIds = await dbContext.ReviewCategories
            .Where(category => category.IsActive && normalizedSelectedIds.Contains(category.Id))
            .Select(category => category.Id)
            .ToListAsync(cancellationToken);

        var selectedActiveIds = activeCategoryIds.ToHashSet();
        var existingAssignments = await dbContext.ReviewCategoryAssignments
            .Where(assignment => assignment.ReviewId == reviewId)
            .ToListAsync(cancellationToken);

        var existingIds = existingAssignments
            .Select(assignment => assignment.ReviewCategoryId)
            .ToHashSet();

        var now = DateTimeOffset.UtcNow;
        var idsToRemove = existingIds.Except(selectedActiveIds).ToList();
        var idsToAdd = selectedActiveIds.Except(existingIds).ToList();

        if (idsToRemove.Count == 0 && idsToAdd.Count == 0)
        {
            return;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        foreach (var assignment in existingAssignments.Where(assignment => idsToRemove.Contains(assignment.ReviewCategoryId)))
        {
            dbContext.ReviewCategoryAssignments.Remove(assignment);
            dbContext.ReviewCategoryAudits.Add(CreateAudit(reviewId, assignment.ReviewCategoryId, CategoryAuditAction.Removed, changedByUserId, now));
        }

        foreach (var categoryId in idsToAdd)
        {
            dbContext.ReviewCategoryAssignments.Add(new ReviewCategoryAssignment
            {
                ReviewId = reviewId,
                ReviewCategoryId = categoryId,
                AssignedByUserId = changedByUserId,
                AssignedAtUtc = now
            });
            dbContext.ReviewCategoryAudits.Add(CreateAudit(reviewId, categoryId, CategoryAuditAction.Assigned, changedByUserId, now));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static ReviewCategoryAudit CreateAudit(
        Guid reviewId,
        Guid categoryId,
        CategoryAuditAction action,
        string changedByUserId,
        DateTimeOffset changedAtUtc)
    {
        return new ReviewCategoryAudit
        {
            Id = Guid.NewGuid(),
            ReviewId = reviewId,
            ReviewCategoryId = categoryId,
            Action = action,
            ChangedByUserId = changedByUserId,
            ChangedAtUtc = changedAtUtc
        };
    }
}
