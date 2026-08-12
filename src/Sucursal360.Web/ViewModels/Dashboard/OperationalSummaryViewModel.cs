namespace Sucursal360.Web.ViewModels.Dashboard;

public sealed record OperationalSummaryViewModel(
    bool HasData,
    decimal? NetSales,
    int? TransactionCount,
    decimal? AverageTicket,
    string Currency,
    string SourceLabel);
