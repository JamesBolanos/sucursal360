namespace Sucursal360.Web.ViewModels.Reports;

public sealed class ManagementReportFiltersViewModel
{
    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? CategoryId { get; set; }
}
