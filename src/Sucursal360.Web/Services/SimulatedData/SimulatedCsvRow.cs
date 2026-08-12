namespace Sucursal360.Web.Services.SimulatedData;

public sealed record SimulatedCsvRow(
    int RowNumber,
    Guid BranchId,
    string BranchCode,
    string BranchName,
    DateOnly BusinessDate,
    decimal NetSales,
    int TransactionCount,
    string Currency);
