namespace Sucursal360.Web.ViewModels.Branches;

public sealed record BranchDetailsViewModel(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    string Provider,
    string ExternalPlaceId,
    BranchSnapshotSummaryViewModel? LatestSnapshot,
    IntegrationRunSummaryViewModel? LastIntegrationRun,
    IReadOnlyList<BranchTrendPointViewModel> Trend,
    IReadOnlyList<BranchSnapshotHistoryItemViewModel> SnapshotHistory,
    BranchOperationalSummaryViewModel OperationalSummary);
