namespace Sucursal360.Web.ViewModels.Dashboard;

public sealed record SalesSliceViewModel(
    string Label,
    decimal Value,
    decimal Percent,
    string Color);
