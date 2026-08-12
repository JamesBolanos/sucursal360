namespace Sucursal360.Web.ViewModels.Reports;

public sealed record ManagementReportPageViewModel(
    ManagementReportFiltersViewModel Filters,
    IReadOnlyList<ReportBranchOptionViewModel> Branches,
    IReadOnlyList<ReportCategoryOptionViewModel> Categories);
