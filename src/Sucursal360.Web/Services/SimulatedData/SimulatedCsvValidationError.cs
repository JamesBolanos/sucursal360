namespace Sucursal360.Web.Services.SimulatedData;

public sealed record SimulatedCsvValidationError(
    int RowNumber,
    string Field,
    string ErrorCode,
    string Message);
