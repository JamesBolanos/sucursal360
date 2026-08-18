namespace Sucursal360.Web.ViewModels.Dashboard;

public sealed record ReviewRatingSliceViewModel(
    string Label,
    int Count,
    decimal Percent,
    string Color);
