using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.Security;
using Sucursal360.Web.ViewModels.Dashboard;

namespace Sucursal360.Web.Controllers;

[Authorize(Policy = AppPolicies.CanViewCorporateDashboard)]
public class DashboardController(ApplicationDbContext dbContext) : Controller
{
    [HttpGet("/dashboard")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var branches = await dbContext.Branches
            .OrderBy(branch => branch.Code)
            .Select(branch => new BranchProjection(
                branch.Id,
                branch.Code,
                branch.Name,
                branch.IsActive,
                branch.Provider,
                branch.ExternalPlaceId))
            .ToListAsync(cancellationToken);

        var branchIds = branches.Select(branch => branch.Id).ToList();
        var snapshots = await dbContext.BranchSnapshots
            .Where(snapshot => branchIds.Contains(snapshot.BranchId))
            .Select(snapshot => new SnapshotProjection(
                snapshot.BranchId,
                snapshot.Provider,
                snapshot.BusinessStatus,
                snapshot.Rating,
                snapshot.ReviewCount,
                snapshot.RetrievedAtUtc))
            .ToListAsync(cancellationToken);

        var operationalMetrics = await dbContext.SimulatedOperationalMetrics
            .Where(metric => branchIds.Contains(metric.BranchId))
            .Select(metric => new OperationalMetricProjection(
                metric.NetSales,
                metric.TransactionCount,
                metric.Currency))
            .ToListAsync(cancellationToken);

        var rows = branches
            .Select(branch => BuildBranchRow(branch, snapshots.Where(snapshot => snapshot.BranchId == branch.Id)))
            .ToList();

        return View(new CorporateDashboardViewModel(
            BuildSummary(rows),
            BuildInsights(rows),
            BuildRanking(rows),
            rows,
            BuildOperationalSummary(operationalMetrics)));
    }

    private static CorporateDashboardBranchRowViewModel BuildBranchRow(
        BranchProjection branch,
        IEnumerable<SnapshotProjection> branchSnapshots)
    {
        var orderedSnapshots = branchSnapshots
            .OrderByDescending(snapshot => snapshot.RetrievedAtUtc)
            .ToList();

        var latestSnapshot = orderedSnapshots.FirstOrDefault();
        var previousSnapshot = orderedSnapshots.Skip(1).FirstOrDefault();

        return new CorporateDashboardBranchRowViewModel(
            branch.Id,
            branch.Code,
            branch.Name,
            branch.IsActive,
            branch.Provider.ToString(),
            branch.ExternalPlaceId ?? "No disponible",
            latestSnapshot is null ? "No disponible" : FormatBusinessStatus(latestSnapshot.BusinessStatus),
            latestSnapshot is null ? "Sin sincronizar" : "Actualizado",
            latestSnapshot?.Rating,
            latestSnapshot?.Rating is null || previousSnapshot?.Rating is null
                ? null
                : latestSnapshot.Rating.Value - previousSnapshot.Rating.Value,
            latestSnapshot?.ReviewCount,
            latestSnapshot?.ReviewCount is null || previousSnapshot?.ReviewCount is null
                ? null
                : latestSnapshot.ReviewCount.Value - previousSnapshot.ReviewCount.Value,
            latestSnapshot?.RetrievedAtUtc,
            latestSnapshot is null ? "No disponible" : latestSnapshot.Provider.ToString());
    }

    private static DashboardSummaryViewModel BuildSummary(IReadOnlyList<CorporateDashboardBranchRowViewModel> rows)
    {
        var ratings = rows
            .Where(row => row.Rating is not null)
            .Select(row => row.Rating!.Value)
            .ToList();

        var reviewCounts = rows
            .Where(row => row.ReviewCount is not null)
            .Select(row => row.ReviewCount!.Value)
            .ToList();

        var retrievedDates = rows
            .Where(row => row.RetrievedAtUtc is not null)
            .Select(row => row.RetrievedAtUtc!.Value)
            .OrderByDescending(retrievedAtUtc => retrievedAtUtc)
            .ToList();

        return new DashboardSummaryViewModel(
            rows.Count,
            rows.Count(row => row.IsActive),
            rows.Count(row => row.RetrievedAtUtc is not null),
            ratings.Count == 0 ? null : Math.Round(ratings.Average(), 2),
            reviewCounts.Count == 0 ? null : reviewCounts.Sum(),
            retrievedDates.Count == 0 ? null : retrievedDates[0]);
    }

    private static OperationalSummaryViewModel BuildOperationalSummary(IReadOnlyList<OperationalMetricProjection> metrics)
    {
        if (metrics.Count == 0)
        {
            return new OperationalSummaryViewModel(false, null, null, null, "NIO", "Datos simulados");
        }

        var netSales = metrics.Sum(metric => metric.NetSales);
        var transactionCount = metrics.Sum(metric => metric.TransactionCount);

        return new OperationalSummaryViewModel(
            true,
            netSales,
            transactionCount,
            transactionCount == 0 ? null : netSales / transactionCount,
            metrics.First().Currency,
            "Datos simulados");
    }

    private static IReadOnlyList<DashboardInsightViewModel> BuildInsights(IReadOnlyList<CorporateDashboardBranchRowViewModel> rows)
    {
        var synchronizedRows = rows
            .Where(row => row.Rating is not null)
            .OrderByDescending(row => row.Rating)
            .ToList();

        var bestBranch = synchronizedRows.FirstOrDefault();
        var lowestBranch = synchronizedRows.OrderBy(row => row.Rating).FirstOrDefault();
        var growingBranch = rows
            .Where(row => row.ReviewCountDelta is not null)
            .OrderByDescending(row => row.ReviewCountDelta)
            .FirstOrDefault();
        var unsynchronizedCount = rows.Count(row => row.RetrievedAtUtc is null);

        return
        [
            bestBranch is null
                ? new DashboardInsightViewModel("Mejor reputacion", "No disponible", "Sin sincronizaciones todavia.", "muted")
                : new DashboardInsightViewModel("Mejor reputacion", bestBranch.Rating!.Value.ToString("0.00"), bestBranch.Name, "success"),
            lowestBranch is null
                ? new DashboardInsightViewModel("Requiere atencion", "No disponible", "Sin datos para comparar.", "muted")
                : new DashboardInsightViewModel("Requiere atencion", lowestBranch.Rating!.Value.ToString("0.00"), lowestBranch.Name, "warning"),
            growingBranch is null
                ? new DashboardInsightViewModel("Mayor crecimiento", "No disponible", "Se necesita una segunda sincronizacion.", "muted")
                : new DashboardInsightViewModel("Mayor crecimiento", $"+{growingBranch.ReviewCountDelta:N0}", growingBranch.Name, "info"),
            new DashboardInsightViewModel("Pendientes de datos", unsynchronizedCount.ToString("N0"), "Sucursales sin snapshot publico.", unsynchronizedCount == 0 ? "success" : "warning")
        ];
    }

    private static IReadOnlyList<BranchRankingItemViewModel> BuildRanking(IReadOnlyList<CorporateDashboardBranchRowViewModel> rows)
    {
        return rows
            .OrderByDescending(row => row.Rating ?? 0)
            .ThenByDescending(row => row.ReviewCount ?? 0)
            .Select((row, index) => new BranchRankingItemViewModel(
                row.BranchId,
                index + 1,
                row.Code,
                row.Name,
                row.Rating,
                row.Rating is null ? 0 : (int)Math.Round(row.Rating.Value * 20),
                row.ReviewCount,
                row.DataStatus))
            .ToList();
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
        Guid BranchId,
        PublicDataProvider Provider,
        BusinessStatus? BusinessStatus,
        decimal? Rating,
        int? ReviewCount,
        DateTimeOffset RetrievedAtUtc);

    private sealed record OperationalMetricProjection(
        decimal NetSales,
        int TransactionCount,
        string Currency);
}
