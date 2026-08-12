using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Data.Seed;
using Sucursal360.Web.Domain.Entities;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.Services.Reviews;

namespace Sucursal360.Tests;

[TestClass]
public sealed class ReviewCategorizationServiceTests
{
    [TestMethod]
    public async Task ReplaceCategoriesAddsRemovesAndAuditsWithoutChangingReviewText()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        context.Users.Add(CreateAdminUser());
        var review = CreateReview("REV-001", "Original review text");
        context.Reviews.Add(review);
        context.ReviewCategoryAssignments.Add(new ReviewCategoryAssignment
        {
            ReviewId = review.Id,
            ReviewCategoryId = SeedIds.CategoryServicio,
            AssignedByUserId = "admin-user",
            AssignedAtUtc = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new ReviewCategorizationService(context);

        await service.ReplaceCategoriesAsync(
            review.Id,
            [SeedIds.CategoryCalidad],
            "admin-user",
            CancellationToken.None);

        var categoryIds = await context.ReviewCategoryAssignments
            .Where(assignment => assignment.ReviewId == review.Id)
            .Select(assignment => assignment.ReviewCategoryId)
            .ToListAsync();
        Assert.HasCount(1, categoryIds);
        Assert.AreEqual(SeedIds.CategoryCalidad, categoryIds[0]);

        var audits = await context.ReviewCategoryAudits
            .Where(audit => audit.ReviewId == review.Id)
            .OrderBy(audit => audit.Action)
            .ToListAsync();
        Assert.HasCount(2, audits);
        Assert.IsTrue(audits.Any(audit => audit.ReviewCategoryId == SeedIds.CategoryServicio && audit.Action == CategoryAuditAction.Removed));
        Assert.IsTrue(audits.Any(audit => audit.ReviewCategoryId == SeedIds.CategoryCalidad && audit.Action == CategoryAuditAction.Assigned));

        var persistedReview = await context.Reviews.SingleAsync(candidate => candidate.Id == review.Id);
        Assert.AreEqual("Original review text", persistedReview.Text);
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ApplicationUser CreateAdminUser()
    {
        return new ApplicationUser
        {
            Id = "admin-user",
            UserName = "admin@sucursal360.local",
            Email = "admin@sucursal360.local",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    private static Review CreateReview(string externalReviewId, string text)
    {
        return new Review
        {
            Id = Guid.NewGuid(),
            BranchId = SeedIds.BranchCentro,
            Provider = PublicDataProvider.Demo,
            ExternalReviewId = externalReviewId,
            Rating = 4,
            Text = text,
            PublishedAtUtc = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero),
            AuthorDisplayName = "Cliente demo",
            Language = "es",
            RetrievedAtUtc = new DateTimeOffset(2026, 8, 12, 11, 0, 0, TimeSpan.Zero)
        };
    }
}
