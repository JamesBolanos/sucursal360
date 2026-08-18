namespace Sucursal360.Web.ViewModels.Dashboard;

public sealed record ReviewCoverageViewModel(
    int TotalCount,
    int CategorizedCount,
    int UncategorizedCount,
    int CategorizedPercent);
