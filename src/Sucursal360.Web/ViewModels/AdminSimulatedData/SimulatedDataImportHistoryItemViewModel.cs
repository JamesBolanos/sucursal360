namespace Sucursal360.Web.ViewModels.AdminSimulatedData;

public sealed record SimulatedDataImportHistoryItemViewModel(
    Guid Id,
    string FileName,
    int RowCount,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateTimeOffset ImportedAtUtc,
    string ImportedByEmail);
