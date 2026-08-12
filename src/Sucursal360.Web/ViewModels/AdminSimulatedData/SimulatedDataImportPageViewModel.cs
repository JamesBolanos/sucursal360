using Sucursal360.Web.Services.SimulatedData;

namespace Sucursal360.Web.ViewModels.AdminSimulatedData;

public sealed record SimulatedDataImportPageViewModel(
    SimulatedCsvValidationResult? Preview,
    SimulatedDataImportResult? ImportResult,
    string? EncodedCsvContent,
    long FileSizeBytes,
    IReadOnlyList<SimulatedDataImportHistoryItemViewModel> RecentImports);
