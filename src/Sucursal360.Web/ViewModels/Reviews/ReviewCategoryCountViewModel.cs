namespace Sucursal360.Web.ViewModels.Reviews;

public sealed record ReviewCategoryCountViewModel(
    Guid CategoryId,
    string Name,
    int Count);
