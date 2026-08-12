namespace Sucursal360.Web.ViewModels.Dashboard;

public sealed record CorporateDashboardViewModel(
    DashboardSummaryViewModel Summary,
    IReadOnlyList<DashboardInsightViewModel> Insights,
    IReadOnlyList<BranchRankingItemViewModel> Ranking,
    IReadOnlyList<CorporateDashboardBranchRowViewModel> Branches,
    OperationalSummaryViewModel OperationalSummary);
