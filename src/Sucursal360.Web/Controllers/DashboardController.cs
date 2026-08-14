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
    private static readonly string[] ChartColors =
    [
        "#1f6f4a",
        "#1f5eff",
        "#d48806",
        "#b42318",
        "#6f3dbd",
        "#0f766e",
        "#9f1239"
    ];

    [HttpGet("/dashboard")]
    public async Task<IActionResult> Index([FromQuery] DashboardFiltersViewModel filters, CancellationToken cancellationToken)
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

        var reviews = await dbContext.Reviews
            .Where(review => branchIds.Contains(review.BranchId))
            .Select(review => new ReviewProjection(
                review.BranchId,
                review.Rating,
                review.PublishedAtUtc))
            .ToListAsync(cancellationToken);

        var operationalMetrics = await dbContext.SimulatedOperationalMetrics
            .Where(metric => branchIds.Contains(metric.BranchId))
            .Select(metric => new OperationalMetricProjection(
                metric.BranchId,
                metric.BusinessDate,
                metric.NetSales,
                metric.TransactionCount,
                metric.Currency))
            .ToListAsync(cancellationToken);

        var monthOptions = BuildMonthOptions(operationalMetrics, reviews);
        var normalizedFilters = NormalizeFilters(filters, monthOptions);
        var filteredBranches = ApplyBranchFilter(branches, normalizedFilters).ToList();
        var filteredBranchIds = filteredBranches.Select(branch => branch.Id).ToHashSet();
        var filteredReviews = ApplyReviewFilters(reviews, normalizedFilters)
            .Where(review => filteredBranchIds.Contains(review.BranchId))
            .ToList();
        var filteredOperationalMetrics = ApplyOperationalFilters(operationalMetrics, normalizedFilters)
            .Where(metric => filteredBranchIds.Contains(metric.BranchId))
            .ToList();

        var rows = filteredBranches
            .Select(branch => BuildBranchRow(
                branch,
                snapshots.Where(snapshot => snapshot.BranchId == branch.Id),
                filteredReviews.Where(review => review.BranchId == branch.Id),
                filteredOperationalMetrics.Where(metric => metric.BranchId == branch.Id)))
            .ToList();

        rows = ApplyAttentionLevels(rows)
            .OrderByDescending(row => row.NetSales ?? 0)
            .ThenBy(row => row.Code)
            .ToList();

        var operationalSummary = BuildOperationalSummary(filteredOperationalMetrics);

        return View(new CorporateDashboardViewModel(
            normalizedFilters,
            branches.Select(branch => new DashboardBranchOptionViewModel(branch.Id, branch.Code, branch.Name)).ToList(),
            monthOptions,
            BuildExecutiveSummary(rows, operationalSummary, normalizedFilters.BranchId),
            BuildSummary(rows),
            BuildInsights(rows, operationalSummary),
            BuildRanking(rows),
            rows,
            operationalSummary,
            BuildSalesSlices(rows, filteredOperationalMetrics, normalizedFilters.BranchId),
            BuildTicketBars(rows, filteredOperationalMetrics, normalizedFilters.BranchId),
            [],
            [],
            []));
    }

    private static DashboardFiltersViewModel NormalizeFilters(
        DashboardFiltersViewModel filters,
        IReadOnlyList<string> monthOptions)
    {
        var selectedMonth = IsValidMonth(filters.Month)
            ? filters.Month
            : monthOptions.FirstOrDefault();

        var (fromDate, toDate) = GetMonthRange(selectedMonth);
        return new DashboardFiltersViewModel
        {
            Month = selectedMonth,
            FromDate = fromDate,
            ToDate = toDate,
            BranchId = filters.BranchId
        };
    }

    private static IReadOnlyList<string> BuildMonthOptions(
        IReadOnlyList<OperationalMetricProjection> metrics,
        IReadOnlyList<ReviewProjection> reviews)
    {
        return metrics
            .Select(metric => metric.BusinessDate.ToString("yyyy-MM"))
            .Concat(reviews
                .Where(review => review.PublishedAtUtc is not null)
                .Select(review => DateOnly.FromDateTime(review.PublishedAtUtc!.Value.UtcDateTime).ToString("yyyy-MM")))
            .Distinct()
            .OrderByDescending(month => month)
            .ToList();
    }

    private static (DateOnly? FromDate, DateOnly? ToDate) GetMonthRange(string? month)
    {
        if (!IsValidMonth(month))
        {
            return (null, null);
        }

        var selectedMonth = month!;
        var year = int.Parse(selectedMonth[..4]);
        var monthNumber = int.Parse(selectedMonth[5..7]);
        var fromDate = new DateOnly(year, monthNumber, 1);
        var toDate = new DateOnly(year, monthNumber, DateTime.DaysInMonth(year, monthNumber));

        return (fromDate, toDate);
    }

    private static bool IsValidMonth(string? month)
    {
        return month is { Length: 7 } &&
            int.TryParse(month[..4], out var year) &&
            int.TryParse(month[5..7], out var monthNumber) &&
            month[4] == '-' &&
            year is >= 2000 and <= 2100 &&
            monthNumber is >= 1 and <= 12;
    }

    private static IEnumerable<BranchProjection> ApplyBranchFilter(
        IEnumerable<BranchProjection> branches,
        DashboardFiltersViewModel filters)
    {
        return filters.BranchId is null
            ? branches
            : branches.Where(branch => branch.Id == filters.BranchId.Value);
    }

    private static IEnumerable<ReviewProjection> ApplyReviewFilters(
        IEnumerable<ReviewProjection> reviews,
        DashboardFiltersViewModel filters)
    {
        if (filters.FromDate is null || filters.ToDate is null)
        {
            return reviews;
        }

        return reviews.Where(review =>
            review.PublishedAtUtc is not null &&
            DateOnly.FromDateTime(review.PublishedAtUtc.Value.UtcDateTime) >= filters.FromDate.Value &&
            DateOnly.FromDateTime(review.PublishedAtUtc.Value.UtcDateTime) <= filters.ToDate.Value);
    }

    private static IEnumerable<OperationalMetricProjection> ApplyOperationalFilters(
        IEnumerable<OperationalMetricProjection> metrics,
        DashboardFiltersViewModel filters)
    {
        if (filters.FromDate is null || filters.ToDate is null)
        {
            return metrics;
        }

        return metrics.Where(metric =>
            metric.BusinessDate >= filters.FromDate.Value &&
            metric.BusinessDate <= filters.ToDate.Value);
    }

    private static CorporateDashboardBranchRowViewModel BuildBranchRow(
        BranchProjection branch,
        IEnumerable<SnapshotProjection> branchSnapshots,
        IEnumerable<ReviewProjection> branchReviews,
        IEnumerable<OperationalMetricProjection> branchMetrics)
    {
        var orderedSnapshots = branchSnapshots
            .OrderByDescending(snapshot => snapshot.RetrievedAtUtc)
            .ToList();

        var reviews = branchReviews.ToList();
        var metrics = branchMetrics.ToList();
        var latestSnapshot = orderedSnapshots.FirstOrDefault();
        var previousSnapshot = orderedSnapshots.Skip(1).FirstOrDefault();
        var netSales = metrics.Count == 0 ? (decimal?)null : metrics.Sum(metric => metric.NetSales);
        var transactionCount = metrics.Count == 0 ? (int?)null : metrics.Sum(metric => metric.TransactionCount);
        var negativeReviewCount = reviews.Count(review => review.Rating is >= 1 and <= 2);

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
            reviews.Count,
            negativeReviewCount,
            "No disponible",
            netSales,
            transactionCount,
            transactionCount is null or 0 || netSales is null ? null : netSales.Value / transactionCount.Value,
            metrics.FirstOrDefault()?.Currency ?? "NIO",
            "Normal",
            "Operacion estable con los filtros actuales.",
            latestSnapshot?.RetrievedAtUtc,
            latestSnapshot is null ? "No disponible" : latestSnapshot.Provider.ToString());
    }

    private static IReadOnlyList<CorporateDashboardBranchRowViewModel> ApplyAttentionLevels(
        IReadOnlyList<CorporateDashboardBranchRowViewModel> rows)
    {
        var salesValues = rows
            .Where(row => row.NetSales is not null)
            .Select(row => row.NetSales!.Value)
            .ToList();
        var averageSales = salesValues.Count == 0 ? (decimal?)null : salesValues.Average();

        return rows
            .Select(row =>
            {
                var attention = CalculateAttention(row, averageSales);
                return row with
                {
                    AttentionLevel = attention.Level,
                    AttentionReason = attention.Reason
                };
            })
            .ToList();
    }

    private static AttentionResult CalculateAttention(
        CorporateDashboardBranchRowViewModel row,
        decimal? averageSales)
    {
        if (row.NetSales is null)
        {
            return new AttentionResult("Media", "Falta importar ventas del periodo.");
        }

        if (row.Rating is null)
        {
            return new AttentionResult("Media", "Falta sincronizar reputacion publica.");
        }

        if (row.Rating < 4m && averageSales is not null && row.NetSales < averageSales)
        {
            return new AttentionResult("Alta", "Rating bajo y ventas bajo promedio.");
        }

        if (row.NegativeReviewCount > 0 || row.Rating < 4m)
        {
            return new AttentionResult("Media", "Hay resenas de baja calificacion.");
        }

        return new AttentionResult("Normal", "Operacion estable con los filtros actuales.");
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

    private static ExecutiveSummaryViewModel BuildExecutiveSummary(
        IReadOnlyList<CorporateDashboardBranchRowViewModel> rows,
        OperationalSummaryViewModel operationalSummary,
        Guid? selectedBranchId)
    {
        if (selectedBranchId is not null)
        {
            var branch = rows.FirstOrDefault();
            if (branch is null)
            {
                return new ExecutiveSummaryViewModel(
                    "Sucursal",
                    "No disponible",
                    "La sucursal seleccionada no tiene datos para el periodo.",
                    "Sin datos",
                    "muted",
                    0);
            }

            return new ExecutiveSummaryViewModel(
                branch.Name,
                branch.AttentionLevel == "Alta" ? "Requiere seguimiento" : "Vista mensual de la sucursal",
                $"Ventas {branch.Currency} {FormatAmount(branch.NetSales)}; ticket promedio {branch.Currency} {FormatAmount(branch.AverageTicket)}; rating {FormatAmount(branch.Rating)}.",
                branch.AttentionLevel,
                ToneForAttention(branch.AttentionLevel),
                HealthPercent(branch));
        }

        var bestSalesBranch = rows
            .Where(row => row.NetSales is not null)
            .OrderByDescending(row => row.NetSales)
            .FirstOrDefault();
        var highAttentionCount = rows.Count(row => row.AttentionLevel == "Alta");
        var normalCount = rows.Count(row => row.AttentionLevel == "Normal");

        return new ExecutiveSummaryViewModel(
            "Todas las sucursales",
            bestSalesBranch is null ? "Sin ventas del periodo" : $"{bestSalesBranch.Name} lidera ventas",
            $"Total {operationalSummary.Currency} {FormatAmount(operationalSummary.NetSales)}; {FormatAmount(operationalSummary.TransactionCount)} transacciones; ticket promedio {operationalSummary.Currency} {FormatAmount(operationalSummary.AverageTicket)}.",
            highAttentionCount == 0 ? "Controlado" : $"{highAttentionCount} en atencion",
            highAttentionCount == 0 ? "success" : "warning",
            rows.Count == 0 ? 0 : (int)Math.Round(normalCount * 100m / rows.Count));
    }

    private static IReadOnlyList<DashboardInsightViewModel> BuildInsights(
        IReadOnlyList<CorporateDashboardBranchRowViewModel> rows,
        OperationalSummaryViewModel operationalSummary)
    {
        var bestSalesBranch = rows
            .Where(row => row.NetSales is not null)
            .OrderByDescending(row => row.NetSales)
            .FirstOrDefault();
        var bestTicketBranch = rows
            .Where(row => row.AverageTicket is not null)
            .OrderByDescending(row => row.AverageTicket)
            .FirstOrDefault();
        var lowestRatingBranch = rows
            .Where(row => row.Rating is not null)
            .OrderBy(row => row.Rating)
            .FirstOrDefault();

        return
        [
            new DashboardInsightViewModel("Ventas", $"{operationalSummary.Currency} {FormatAmount(operationalSummary.NetSales)}", bestSalesBranch is null ? "Sin datos operativos." : $"Mayor aporte: {bestSalesBranch.Code}.", "info"),
            new DashboardInsightViewModel("Transacciones", FormatAmount(operationalSummary.TransactionCount), "Movimiento del mes seleccionado.", "muted"),
            new DashboardInsightViewModel("Ticket", $"{operationalSummary.Currency} {FormatAmount(operationalSummary.AverageTicket)}", bestTicketBranch is null ? "Sin ticket promedio." : $"Mayor ticket: {bestTicketBranch.Code}.", "success"),
            lowestRatingBranch is null
                ? new DashboardInsightViewModel("Rating", "No disponible", "Sin reputacion sincronizada.", "muted")
                : new DashboardInsightViewModel("Rating", lowestRatingBranch.Rating!.Value.ToString("0.00"), $"Menor rating: {lowestRatingBranch.Code}.", lowestRatingBranch.Rating < 4 ? "warning" : "success")
        ];
    }

    private static IReadOnlyList<BranchRankingItemViewModel> BuildRanking(IReadOnlyList<CorporateDashboardBranchRowViewModel> rows)
    {
        return rows
            .OrderByDescending(row => row.NetSales ?? 0)
            .ThenBy(row => row.Code)
            .Select((row, index) => new BranchRankingItemViewModel(
                row.BranchId,
                index + 1,
                row.Code,
                row.Name,
                row.Rating,
                row.Rating is null ? 0 : (int)Math.Round(row.Rating.Value * 20),
                row.ReviewCount,
                row.AttentionLevel))
            .ToList();
    }

    private static IReadOnlyList<SalesSliceViewModel> BuildSalesSlices(
        IReadOnlyList<CorporateDashboardBranchRowViewModel> rows,
        IReadOnlyList<OperationalMetricProjection> metrics,
        Guid? selectedBranchId)
    {
        var source = selectedBranchId is null
            ? rows
                .Where(row => row.NetSales is not null)
                .Select(row => new ChartValue(row.Code, row.NetSales!.Value))
                .ToList()
            : metrics
                .GroupBy(metric => metric.BusinessDate)
                .OrderBy(group => group.Key)
                .Select(group => new ChartValue(group.Key.ToString("dd MMM"), group.Sum(metric => metric.NetSales)))
                .ToList();

        var total = source.Sum(item => item.Value);
        if (total <= 0)
        {
            return [];
        }

        return source
            .Select((item, index) => new SalesSliceViewModel(
                item.Label,
                item.Value,
                Math.Round(item.Value * 100m / total, 1),
                ChartColors[index % ChartColors.Length]))
            .ToList();
    }

    private static IReadOnlyList<TicketBarViewModel> BuildTicketBars(
        IReadOnlyList<CorporateDashboardBranchRowViewModel> rows,
        IReadOnlyList<OperationalMetricProjection> metrics,
        Guid? selectedBranchId)
    {
        var source = selectedBranchId is null
            ? rows
                .Where(row => row.AverageTicket is not null)
                .Select(row => new ChartValue(row.Code, row.AverageTicket!.Value))
                .ToList()
            : metrics
                .GroupBy(metric => metric.BusinessDate)
                .OrderBy(group => group.Key)
                .Select(group =>
                {
                    var sales = group.Sum(metric => metric.NetSales);
                    var transactions = group.Sum(metric => metric.TransactionCount);
                    return new ChartValue(group.Key.ToString("dd MMM"), transactions == 0 ? 0 : sales / transactions);
                })
                .ToList();

        var max = source.Select(item => item.Value).DefaultIfEmpty(0).Max();
        if (max <= 0)
        {
            return [];
        }

        return source
            .Select((item, index) => new TicketBarViewModel(
                item.Label,
                item.Value,
                (int)Math.Round(item.Value * 100m / max),
                ChartColors[index % ChartColors.Length]))
            .ToList();
    }

    private static string ToneForAttention(string attentionLevel)
    {
        return attentionLevel switch
        {
            "Alta" => "warning",
            "Media" => "info",
            "Normal" => "success",
            _ => "muted"
        };
    }

    private static int HealthPercent(CorporateDashboardBranchRowViewModel row)
    {
        if (row.AttentionLevel == "Alta")
        {
            return 35;
        }

        if (row.AttentionLevel == "Media")
        {
            return 65;
        }

        return 90;
    }

    private static string FormatAmount(decimal? value)
    {
        return value is null ? "No disponible" : value.Value.ToString("N2");
    }

    private static string FormatAmount(int? value)
    {
        return value is null ? "No disponible" : value.Value.ToString("N0");
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

    private sealed record ReviewProjection(
        Guid BranchId,
        byte? Rating,
        DateTimeOffset? PublishedAtUtc);

    private sealed record OperationalMetricProjection(
        Guid BranchId,
        DateOnly BusinessDate,
        decimal NetSales,
        int TransactionCount,
        string Currency);

    private sealed record AttentionResult(
        string Level,
        string Reason);

    private sealed record ChartValue(
        string Label,
        decimal Value);
}
