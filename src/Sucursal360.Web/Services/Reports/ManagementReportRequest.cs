namespace Sucursal360.Web.Services.Reports;

public sealed record ManagementReportRequest(
    DateOnly? FromDate,
    DateOnly? ToDate,
    Guid? BranchId,
    Guid? CategoryId);
