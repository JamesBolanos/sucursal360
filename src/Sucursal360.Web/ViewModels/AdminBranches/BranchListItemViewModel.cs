namespace Sucursal360.Web.ViewModels.AdminBranches;

public sealed record BranchListItemViewModel(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    string Provider,
    string ExternalPlaceId,
    DateTimeOffset UpdatedAtUtc);
