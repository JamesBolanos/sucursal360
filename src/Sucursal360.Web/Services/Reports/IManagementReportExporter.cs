namespace Sucursal360.Web.Services.Reports;

public interface IManagementReportExporter
{
    Task<ManagementReportResult> ExportAsync(ManagementReportRequest request, CancellationToken cancellationToken);
}
