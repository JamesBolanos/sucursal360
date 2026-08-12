using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Security;
using Sucursal360.Web.Services.Reviews;
using Sucursal360.Web.ViewModels.Reviews;

namespace Sucursal360.Web.Controllers;

[Authorize]
[Route("reviews")]
public class ReviewsController(
    ApplicationDbContext dbContext,
    IBranchAccessService branchAccessService,
    IReviewCategorizationService reviewCategorizationService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] ReviewFiltersViewModel filters, CancellationToken cancellationToken)
    {
        var branches = await GetAccessibleBranchesAsync(cancellationToken);
        var branchIds = branches.Select(branch => branch.Id).ToHashSet();
        if (filters.BranchId is not null && !branchIds.Contains(filters.BranchId.Value))
        {
            return Forbid();
        }

        var categories = await dbContext.ReviewCategories
            .Where(category => category.IsActive)
            .OrderBy(category => category.Name)
            .Select(category => new ReviewCategoryOptionViewModel(category.Id, category.Code, category.Name))
            .ToListAsync(cancellationToken);

        var reviews = await dbContext.Reviews
            .Where(review => branchIds.Contains(review.BranchId))
            .Select(review => new ReviewProjection(
                review.Id,
                review.BranchId,
                review.Branch.Code,
                review.Branch.Name,
                review.Rating,
                review.Text,
                review.AuthorDisplayName,
                review.PublishedAtUtc,
                review.Provider.ToString(),
                review.CategoryAssignments
                    .Select(assignment => assignment.ReviewCategoryId)
                    .ToList()))
            .ToListAsync(cancellationToken);

        var filteredReviews = ApplyFilters(reviews, filters)
            .OrderByDescending(review => review.PublishedAtUtc ?? DateTimeOffset.MinValue)
            .ThenBy(review => review.BranchCode)
            .Take(100)
            .ToList();

        var categoryCounts = categories
            .Select(category => new ReviewCategoryCountViewModel(
                category.Id,
                category.Name,
                filteredReviews.Count(review => review.CategoryIds.Contains(category.Id))))
            .ToList();

        return View(new ReviewsIndexViewModel(
            filters,
            branches,
            categories,
            categoryCounts,
            filteredReviews.Select(ToViewModel).ToList()));
    }

    [HttpPost("{reviewId:guid}/categories")]
    public async Task<IActionResult> UpdateCategories(
        Guid reviewId,
        ReviewCategoryUpdateViewModel model,
        CancellationToken cancellationToken)
    {
        var branchId = await dbContext.Reviews
            .Where(review => review.Id == reviewId)
            .Select(review => (Guid?)review.BranchId)
            .SingleOrDefaultAsync(cancellationToken);

        if (branchId is null)
        {
            return NotFound();
        }

        if (!await branchAccessService.CanAccessAsync(User, branchId.Value, cancellationToken))
        {
            return Forbid();
        }

        await reviewCategorizationService.ReplaceCategoriesAsync(
            reviewId,
            model.SelectedCategoryIds,
            GetUserId(),
            cancellationToken);

        return RedirectToAction(nameof(Index), BuildRouteValues(model, branchId.Value));
    }

    private async Task<IReadOnlyList<ReviewBranchOptionViewModel>> GetAccessibleBranchesAsync(CancellationToken cancellationToken)
    {
        var branches = await dbContext.Branches
            .OrderBy(branch => branch.Code)
            .Select(branch => new ReviewBranchOptionViewModel(branch.Id, branch.Code, branch.Name))
            .ToListAsync(cancellationToken);

        var accessibleBranches = new List<ReviewBranchOptionViewModel>();
        foreach (var branch in branches)
        {
            if (await branchAccessService.CanAccessAsync(User, branch.Id, cancellationToken))
            {
                accessibleBranches.Add(branch);
            }
        }

        return accessibleBranches;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Authenticated user id was not available.");
    }

    private static IEnumerable<ReviewProjection> ApplyFilters(
        IEnumerable<ReviewProjection> reviews,
        ReviewFiltersViewModel filters)
    {
        var filteredReviews = reviews;

        if (filters.BranchId is not null)
        {
            filteredReviews = filteredReviews.Where(review => review.BranchId == filters.BranchId);
        }

        if (filters.Rating is not null)
        {
            filteredReviews = filteredReviews.Where(review => review.Rating == filters.Rating);
        }

        if (filters.CategoryId is not null)
        {
            filteredReviews = filteredReviews.Where(review => review.CategoryIds.Contains(filters.CategoryId.Value));
        }

        if (filters.FromDate is not null)
        {
            filteredReviews = filteredReviews.Where(review =>
                review.PublishedAtUtc is not null &&
                DateOnly.FromDateTime(review.PublishedAtUtc.Value.UtcDateTime) >= filters.FromDate);
        }

        if (filters.ToDate is not null)
        {
            filteredReviews = filteredReviews.Where(review =>
                review.PublishedAtUtc is not null &&
                DateOnly.FromDateTime(review.PublishedAtUtc.Value.UtcDateTime) <= filters.ToDate);
        }

        return filteredReviews;
    }

    private static ReviewListItemViewModel ToViewModel(ReviewProjection review)
    {
        return new ReviewListItemViewModel(
            review.Id,
            review.BranchId,
            review.BranchCode,
            review.BranchName,
            review.Rating,
            review.Text ?? "Sin comentario escrito.",
            review.AuthorDisplayName ?? "Autor anonimo",
            review.PublishedAtUtc,
            review.Provider,
            review.CategoryIds);
    }

    private static object BuildRouteValues(ReviewCategoryUpdateViewModel model, Guid branchId)
    {
        return new
        {
            BranchId = model.BranchId ?? branchId,
            model.Rating,
            model.CategoryId,
            FromDate = model.FromDate?.ToString("yyyy-MM-dd"),
            ToDate = model.ToDate?.ToString("yyyy-MM-dd")
        };
    }

    private sealed record ReviewProjection(
        Guid Id,
        Guid BranchId,
        string BranchCode,
        string BranchName,
        byte? Rating,
        string? Text,
        string? AuthorDisplayName,
        DateTimeOffset? PublishedAtUtc,
        string Provider,
        IReadOnlyList<Guid> CategoryIds);
}
