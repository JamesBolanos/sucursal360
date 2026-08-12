namespace Sucursal360.Web.ViewModels.Branches;

public sealed record BranchSnapshotHistoryItemViewModel(
    DateTimeOffset RetrievedAtUtc,
    string Provider,
    string BusinessStatus,
    decimal? Rating,
    int? ReviewCount,
    string DisplayName,
    string Address);
