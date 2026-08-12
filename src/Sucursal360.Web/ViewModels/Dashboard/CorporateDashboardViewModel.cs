namespace Sucursal360.Web.ViewModels.Dashboard;

public sealed record CorporateDashboardViewModel(
    DashboardSummaryViewModel Summary,
    IReadOnlyList<CorporateDashboardBranchRowViewModel> Branches,
    OperationalSummaryViewModel OperationalSummary);
