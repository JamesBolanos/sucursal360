namespace Sucursal360.Web.ViewModels.Branches;

public sealed record BranchTrendPointViewModel(
    DateTimeOffset RetrievedAtUtc,
    decimal? Rating,
    int RatingPercent,
    int? ReviewCount,
    int ReviewCountPercent);
