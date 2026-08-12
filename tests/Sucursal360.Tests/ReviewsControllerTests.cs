using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Controllers;
using Sucursal360.Web.Data;
using Sucursal360.Web.Data.Seed;
using Sucursal360.Web.Domain.Entities;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.Security;
using Sucursal360.Web.Services.Reviews;
using Sucursal360.Web.ViewModels.Reviews;

namespace Sucursal360.Tests;

[TestClass]
public sealed class ReviewsControllerTests
{
    [TestMethod]
    public async Task IndexFiltersReviewsAndBuildsCategoryCounts()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        context.Users.Add(CreateAdminUser());
        var matchingReview = CreateReview("REV-001", SeedIds.BranchCentro, 5, "Excelente cafe");
        var otherBranchReview = CreateReview("REV-002", SeedIds.BranchGalerias, 5, "Buen ambiente");
        var lowerRatingReview = CreateReview("REV-003", SeedIds.BranchCentro, 3, "Servicio lento");
        context.Reviews.AddRange(matchingReview, otherBranchReview, lowerRatingReview);
        context.ReviewCategoryAssignments.Add(new ReviewCategoryAssignment
        {
            ReviewId = matchingReview.Id,
            ReviewCategoryId = SeedIds.CategoryCalidad,
            AssignedByUserId = "admin-user",
            AssignedAtUtc = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();

        var controller = CreateController(context, canAccess: true);
        var filters = new ReviewFiltersViewModel
        {
            BranchId = SeedIds.BranchCentro,
            Rating = 5,
            CategoryId = SeedIds.CategoryCalidad
        };

        var result = await controller.Index(filters, CancellationToken.None);

        var view = result as ViewResult;
        Assert.IsNotNull(view);
        var model = view.Model as ReviewsIndexViewModel;
        Assert.IsNotNull(model);
        Assert.HasCount(1, model.Reviews);
        Assert.AreEqual(matchingReview.Id, model.Reviews[0].Id);

        var qualityCount = model.CategoryCounts.Single(count => count.CategoryId == SeedIds.CategoryCalidad);
        Assert.AreEqual(1, qualityCount.Count);
    }

    [TestMethod]
    public async Task UpdateCategoriesForbidsWhenReviewBranchIsNotAccessible()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        var review = CreateReview("REV-001", SeedIds.BranchCentro, 4, "Buen cafe");
        context.Reviews.Add(review);
        await context.SaveChangesAsync();

        var controller = CreateController(context, canAccess: false);

        var result = await controller.UpdateCategories(
            review.Id,
            new ReviewCategoryUpdateViewModel(),
            CancellationToken.None);

        Assert.IsInstanceOfType<ForbidResult>(result);
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ReviewsController CreateController(ApplicationDbContext context, bool canAccess)
    {
        return new ReviewsController(
            context,
            new StubBranchAccessService(canAccess),
            new ReviewCategorizationService(context))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreatePrincipal()
                }
            }
        };
    }

    private static ClaimsPrincipal CreatePrincipal()
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "admin-user"),
                new Claim(ClaimTypes.Role, AppRoles.Administrator)
            },
            authenticationType: "Test");

        return new ClaimsPrincipal(identity);
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

    private static Review CreateReview(string externalReviewId, Guid branchId, byte rating, string text)
    {
        return new Review
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            Provider = PublicDataProvider.Demo,
            ExternalReviewId = externalReviewId,
            Rating = rating,
            Text = text,
            PublishedAtUtc = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero),
            AuthorDisplayName = "Cliente demo",
            Language = "es",
            RetrievedAtUtc = new DateTimeOffset(2026, 8, 12, 11, 0, 0, TimeSpan.Zero)
        };
    }

    private sealed class StubBranchAccessService(bool canAccess) : IBranchAccessService
    {
        public Task<bool> CanAccessAsync(ClaimsPrincipal user, Guid branchId, CancellationToken cancellationToken)
        {
            return Task.FromResult(canAccess);
        }
    }
}
