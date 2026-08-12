using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Security;
using Sucursal360.Web.Services.Synchronization;
using Sucursal360.Web.ViewModels.AdminIntegrations;

namespace Sucursal360.Web.Controllers;

[Authorize(Policy = AppPolicies.CanAdministerSystem)]
[Route("admin/integrations")]
public class AdminIntegrationsController(
    ApplicationDbContext dbContext,
    IBranchSynchronizationService synchronizationService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await BuildViewModelAsync([], cancellationToken));
    }

    [HttpPost("sync/{branchId:guid}")]
    public async Task<IActionResult> SyncBranch(Guid branchId, CancellationToken cancellationToken)
    {
        var result = await synchronizationService.SynchronizeBranchAsync(branchId, GetUserId(), cancellationToken);
        return View("Index", await BuildViewModelAsync([ToViewModel(result)], cancellationToken));
    }

    [HttpPost("sync-all")]
    public async Task<IActionResult> SyncAll(CancellationToken cancellationToken)
    {
        var results = await synchronizationService.SynchronizeAllActiveBranchesAsync(GetUserId(), cancellationToken);
        return View("Index", await BuildViewModelAsync(results.Select(ToViewModel).ToList(), cancellationToken));
    }

    private async Task<IntegrationDashboardViewModel> BuildViewModelAsync(
        IReadOnlyList<IntegrationResultItemViewModel> lastResults,
        CancellationToken cancellationToken)
    {
        var branches = await dbContext.Branches
            .OrderBy(branch => branch.Code)
            .Select(branch => new IntegrationBranchItemViewModel(
                branch.Id,
                branch.Code,
                branch.Name,
                branch.IsActive,
                branch.Provider.ToString(),
                branch.ExternalPlaceId ?? "No disponible"))
            .ToListAsync(cancellationToken);

        var integrationRuns = await dbContext.IntegrationRuns
            .Select(run => new IntegrationRunItemViewModel(
                run.Id,
                run.CorrelationId,
                run.Branch.Code,
                run.Branch.Name,
                run.Provider.ToString(),
                run.Status.ToString(),
                run.StartedAtUtc,
                run.FinishedAtUtc,
                run.RecordsReceived,
                run.RecordsStored,
                run.ErrorCode,
                run.UserMessage))
            .ToListAsync(cancellationToken);

        var recentRuns = integrationRuns
            .OrderByDescending(run => run.StartedAtUtc)
            .Take(20)
            .ToList();

        return new IntegrationDashboardViewModel(branches, recentRuns, lastResults);
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Authenticated user id was not available.");
    }

    private static IntegrationResultItemViewModel ToViewModel(SynchronizationResult result)
    {
        return new IntegrationResultItemViewModel(
            result.BranchCode,
            result.BranchName,
            result.Status.ToString(),
            result.CorrelationId,
            result.RecordsReceived,
            result.RecordsStored,
            result.UserMessage,
            result.ErrorCode);
    }
}
