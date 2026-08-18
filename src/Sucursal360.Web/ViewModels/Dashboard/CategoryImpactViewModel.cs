namespace Sucursal360.Web.ViewModels.Dashboard;

public sealed record CategoryImpactViewModel(
    Guid CategoryId,
    string CategoryName,
    int MentionCount,
    decimal? AverageRating,
    int RatingPercent);
