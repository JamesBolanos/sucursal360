namespace Sucursal360.Web.ViewModels.AdminIntegrations;

public sealed record IntegrationDashboardViewModel(
    IReadOnlyList<IntegrationBranchItemViewModel> Branches,
    IReadOnlyList<IntegrationRunItemViewModel> RecentRuns,
    IReadOnlyList<IntegrationResultItemViewModel> LastResults);
