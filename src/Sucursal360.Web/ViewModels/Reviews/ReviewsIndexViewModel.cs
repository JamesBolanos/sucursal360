namespace Sucursal360.Web.ViewModels.Reviews;

public sealed record ReviewsIndexViewModel(
    ReviewFiltersViewModel Filters,
    IReadOnlyList<ReviewBranchOptionViewModel> Branches,
    IReadOnlyList<ReviewCategoryOptionViewModel> Categories,
    IReadOnlyList<ReviewCategoryCountViewModel> CategoryCounts,
    IReadOnlyList<ReviewListItemViewModel> Reviews);
