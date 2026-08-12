namespace Sucursal360.Web.ViewModels.Reviews;

public sealed class ReviewFiltersViewModel
{
    public Guid? BranchId { get; set; }

    public byte? Rating { get; set; }

    public Guid? CategoryId { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }
}
