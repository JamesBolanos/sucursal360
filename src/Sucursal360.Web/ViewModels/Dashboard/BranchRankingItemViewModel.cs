namespace Sucursal360.Web.ViewModels.Dashboard;

public sealed record BranchRankingItemViewModel(
    Guid BranchId,
    int Rank,
    string Code,
    string Name,
    decimal? Rating,
    int RatingPercent,
    int? ReviewCount,
    string DataStatus);
