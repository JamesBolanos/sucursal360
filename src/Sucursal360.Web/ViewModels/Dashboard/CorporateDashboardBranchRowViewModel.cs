namespace Sucursal360.Web.ViewModels.Dashboard;

public sealed record CorporateDashboardBranchRowViewModel(
    Guid BranchId,
    string Code,
    string Name,
    bool IsActive,
    string Provider,
    string ExternalPlaceId,
    string BusinessStatus,
    string DataStatus,
    decimal? Rating,
    decimal? RatingDelta,
    int? ReviewCount,
    int? ReviewCountDelta,
    DateTimeOffset? RetrievedAtUtc,
    string SourceLabel);
