namespace Sucursal360.Web.ViewModels.Dashboard;

public sealed record CorporateDashboardViewModel(
    DashboardFiltersViewModel Filters,
    IReadOnlyList<DashboardBranchOptionViewModel> BranchOptions,
    IReadOnlyList<string> MonthOptions,
    ExecutiveSummaryViewModel ExecutiveSummary,
    DashboardSummaryViewModel Summary,
    IReadOnlyList<DashboardInsightViewModel> Insights,
    IReadOnlyList<BranchRankingItemViewModel> Ranking,
    IReadOnlyList<CorporateDashboardBranchRowViewModel> Branches,
    OperationalSummaryViewModel OperationalSummary,
    IReadOnlyList<SalesSliceViewModel> SalesSlices,
    IReadOnlyList<TicketBarViewModel> TicketBars,
    IReadOnlyList<CategoryImpactViewModel> CategoryImpact,
    IReadOnlyList<ExperienceOperationPointViewModel> ExperienceOperationPoints,
    IReadOnlyList<DashboardRecommendationViewModel> Recommendations);
