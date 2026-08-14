namespace Sucursal360.Web.ViewModels.Dashboard;

public sealed record CategoryImpactViewModel(
    Guid CategoryId,
    string CategoryName,
    int ReviewCount,
    int NegativeReviewCount,
    decimal? AverageRating,
    string AffectedBranches,
    decimal? NetSales,
    int? TransactionCount,
    decimal? AverageTicket,
    string Currency,
    int ImpactPercent);
