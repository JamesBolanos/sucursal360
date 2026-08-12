namespace Sucursal360.Web.Services.SimulatedData;

public interface ISimulatedDataImportService
{
    Task<SimulatedCsvValidationResult> ValidateAsync(
        string csvContent,
        string fileName,
        long fileSizeBytes,
        CancellationToken cancellationToken);

    Task<SimulatedDataImportResult> ImportAsync(
        string csvContent,
        string fileName,
        long fileSizeBytes,
        string importedByUserId,
        CancellationToken cancellationToken);
}
