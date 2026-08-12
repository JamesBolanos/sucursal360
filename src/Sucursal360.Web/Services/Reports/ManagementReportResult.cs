namespace Sucursal360.Web.Services.Reports;

public sealed record ManagementReportResult(
    string FileName,
    string ContentType,
    byte[] Content);
