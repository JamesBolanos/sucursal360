namespace Sucursal360.Web.ViewModels.Branches;

public sealed record BranchSnapshotSummaryViewModel(
    string DisplayName,
    string Address,
    string BusinessStatus,
    decimal? Rating,
    decimal? RatingDelta,
    int? ReviewCount,
    int? ReviewCountDelta,
    DateTimeOffset RetrievedAtUtc,
    string SourceLabel);
