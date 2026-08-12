namespace Sucursal360.Web.Services.SimulatedData;

public sealed record SimulatedDataImportResult(
    Guid ImportId,
    string FileName,
    int RowCount,
    DateOnly PeriodStart,
    DateOnly PeriodEnd);
