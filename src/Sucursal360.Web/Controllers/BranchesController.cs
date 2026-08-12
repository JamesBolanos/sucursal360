using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.Security;
using Sucursal360.Web.ViewModels.Branches;

namespace Sucursal360.Web.Controllers;

[Authorize]
public class BranchesController(
    ApplicationDbContext dbContext,
    IBranchAccessService branchAccessService) : Controller
{
    [HttpGet("/branches/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var branch = await dbContext.Branches
            .Where(branch => branch.Id == id)
            .Select(branch => new BranchProjection(
                branch.Id,
                branch.Code,
                branch.Name,
                branch.IsActive,
                branch.Provider,
                branch.ExternalPlaceId))
            .SingleOrDefaultAsync(cancellationToken);

        if (branch is null)
        {
            return NotFound();
        }

        if (!await branchAccessService.CanAccessAsync(User, id, cancellationToken))
        {
            return Forbid();
        }

        var snapshots = await dbContext.BranchSnapshots
            .Where(snapshot => snapshot.BranchId == id)
            .Select(snapshot => new SnapshotProjection(
                snapshot.Provider,
                snapshot.DisplayName,
                snapshot.Address,
                snapshot.BusinessStatus,
                snapshot.Rating,
                snapshot.ReviewCount,
                snapshot.RetrievedAtUtc))
            .ToListAsync(cancellationToken);

        var integrationRuns = await dbContext.IntegrationRuns
            .Where(run => run.BranchId == id)
            .Select(run => new IntegrationRunProjection(
                run.CorrelationId,
                run.Provider,
                run.Status,
                run.StartedAtUtc,
                run.FinishedAtUtc,
                run.RecordsReceived,
                run.RecordsStored,
                run.ErrorCode,
                run.UserMessage))
            .ToListAsync(cancellationToken);

        var operationalMetrics = await dbContext.SimulatedOperationalMetrics
            .Where(metric => metric.BranchId == id)
            .Select(metric => new OperationalMetricProjection(
                metric.NetSales,
                metric.TransactionCount,
                metric.Currency))
            .ToListAsync(cancellationToken);

        var orderedSnapshots = snapshots
            .OrderByDescending(snapshot => snapshot.RetrievedAtUtc)
            .ToList();

        var latestSnapshot = orderedSnapshots.FirstOrDefault();
        var previousSnapshot = orderedSnapshots.Skip(1).FirstOrDefault();
        var lastRun = integrationRuns
            .OrderByDescending(run => run.StartedAtUtc)
            .FirstOrDefault();

        return View(new BranchDetailsViewModel(
            branch.Id,
            branch.Code,
            branch.Name,
            branch.IsActive,
            branch.Provider.ToString(),
            branch.ExternalPlaceId ?? "No disponible",
            BuildLatestSnapshot(latestSnapshot, previousSnapshot),
            BuildLastIntegrationRun(lastRun),
            BuildTrend(orderedSnapshots),
            BuildHistory(orderedSnapshots),
            BuildOperationalSummary(operationalMetrics)));
    }

    private static BranchSnapshotSummaryViewModel? BuildLatestSnapshot(
        SnapshotProjection? latestSnapshot,
        SnapshotProjection? previousSnapshot)
    {
        if (latestSnapshot is null)
        {
            return null;
        }

        return new BranchSnapshotSummaryViewModel(
            latestSnapshot.DisplayName ?? "No disponible",
            latestSnapshot.Address ?? "No disponible",
            FormatBusinessStatus(latestSnapshot.BusinessStatus),
            latestSnapshot.Rating,
            latestSnapshot.Rating is null || previousSnapshot?.Rating is null
                ? null
                : latestSnapshot.Rating.Value - previousSnapshot.Rating.Value,
            latestSnapshot.ReviewCount,
            latestSnapshot.ReviewCount is null || previousSnapshot?.ReviewCount is null
                ? null
                : latestSnapshot.ReviewCount.Value - previousSnapshot.ReviewCount.Value,
            latestSnapshot.RetrievedAtUtc,
            latestSnapshot.Provider.ToString());
    }

    private static IntegrationRunSummaryViewModel? BuildLastIntegrationRun(IntegrationRunProjection? lastRun)
    {
        if (lastRun is null)
        {
            return null;
        }

        return new IntegrationRunSummaryViewModel(
            lastRun.CorrelationId,
            lastRun.Provider.ToString(),
            lastRun.Status.ToString(),
            lastRun.StartedAtUtc,
            lastRun.FinishedAtUtc,
            lastRun.RecordsReceived,
            lastRun.RecordsStored,
            lastRun.ErrorCode,
            lastRun.UserMessage ?? "No disponible");
    }

    private static IReadOnlyList<BranchTrendPointViewModel> BuildTrend(IReadOnlyList<SnapshotProjection> orderedSnapshots)
    {
        var trendSnapshots = orderedSnapshots
            .Take(12)
            .OrderBy(snapshot => snapshot.RetrievedAtUtc)
            .ToList();

        var maxReviewCount = trendSnapshots
            .Where(snapshot => snapshot.ReviewCount is not null)
            .Select(snapshot => snapshot.ReviewCount!.Value)
            .DefaultIfEmpty(0)
            .Max();

        return trendSnapshots
            .Select(snapshot => new BranchTrendPointViewModel(
                snapshot.RetrievedAtUtc,
                snapshot.Rating,
                snapshot.Rating is null ? 0 : (int)Math.Round(snapshot.Rating.Value * 20),
                snapshot.ReviewCount,
                snapshot.ReviewCount is null || maxReviewCount == 0
                    ? 0
                    : (int)Math.Round(snapshot.ReviewCount.Value * 100m / maxReviewCount)))
            .ToList();
    }

    private static IReadOnlyList<BranchSnapshotHistoryItemViewModel> BuildHistory(IReadOnlyList<SnapshotProjection> orderedSnapshots)
    {
        return orderedSnapshots
            .Take(20)
            .Select(snapshot => new BranchSnapshotHistoryItemViewModel(
                snapshot.RetrievedAtUtc,
                snapshot.Provider.ToString(),
                FormatBusinessStatus(snapshot.BusinessStatus),
                snapshot.Rating,
                snapshot.ReviewCount,
                snapshot.DisplayName ?? "No disponible",
                snapshot.Address ?? "No disponible"))
            .ToList();
    }

    private static BranchOperationalSummaryViewModel BuildOperationalSummary(IReadOnlyList<OperationalMetricProjection> metrics)
    {
        if (metrics.Count == 0)
        {
            return new BranchOperationalSummaryViewModel(false, null, null, null, "NIO", "Datos simulados");
        }

        var netSales = metrics.Sum(metric => metric.NetSales);
        var transactionCount = metrics.Sum(metric => metric.TransactionCount);

        return new BranchOperationalSummaryViewModel(
            true,
            netSales,
            transactionCount,
            transactionCount == 0 ? null : netSales / transactionCount,
            metrics.First().Currency,
            "Datos simulados");
    }

    private static string FormatBusinessStatus(BusinessStatus? businessStatus)
    {
        return businessStatus switch
        {
            BusinessStatus.Operational => "Operando",
            BusinessStatus.TemporarilyClosed => "Cierre temporal",
            BusinessStatus.PermanentlyClosed => "Cierre permanente",
            BusinessStatus.Unknown => "Desconocido",
            _ => "No disponible"
        };
    }

    private sealed record BranchProjection(
        Guid Id,
        string Code,
        string Name,
        bool IsActive,
        PublicDataProvider Provider,
        string? ExternalPlaceId);

    private sealed record SnapshotProjection(
        PublicDataProvider Provider,
        string? DisplayName,
        string? Address,
        BusinessStatus? BusinessStatus,
        decimal? Rating,
        int? ReviewCount,
        DateTimeOffset RetrievedAtUtc);

    private sealed record IntegrationRunProjection(
        string CorrelationId,
        PublicDataProvider Provider,
        IntegrationRunStatus Status,
        DateTimeOffset StartedAtUtc,
        DateTimeOffset? FinishedAtUtc,
        int RecordsReceived,
        int RecordsStored,
        string? ErrorCode,
        string? UserMessage);

    private sealed record OperationalMetricProjection(
        decimal NetSales,
        int TransactionCount,
        string Currency);
}
