namespace Sucursal360.Web.ViewModels.Dashboard;

public sealed class DashboardFiltersViewModel
{
    public string? Month { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    public Guid? BranchId { get; set; }
}
