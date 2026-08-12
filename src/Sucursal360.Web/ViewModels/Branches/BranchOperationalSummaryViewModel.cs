namespace Sucursal360.Web.ViewModels.Branches;

public sealed record BranchOperationalSummaryViewModel(
    bool HasData,
    decimal? NetSales,
    int? TransactionCount,
    decimal? AverageTicket,
    string Currency,
    string SourceLabel);
