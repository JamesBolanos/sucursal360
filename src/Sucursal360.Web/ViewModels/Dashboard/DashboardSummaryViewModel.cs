namespace Sucursal360.Web.ViewModels.Dashboard;

public sealed record DashboardSummaryViewModel(
    int TotalBranches,
    int ActiveBranches,
    int BranchesWithSnapshots,
    decimal? AverageRating,
    int? TotalReviewCount,
    DateTimeOffset? LastSynchronizationAtUtc);
