namespace Sucursal360.Web.ViewModels.AdminIntegrations;

public sealed record IntegrationBranchItemViewModel(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    string Provider,
    string ExternalPlaceId);
