namespace Sucursal360.Web.ViewModels.Reviews;

public sealed record ReviewListItemViewModel(
    Guid Id,
    Guid BranchId,
    string BranchCode,
    string BranchName,
    byte? Rating,
    string Text,
    string AuthorDisplayName,
    DateTimeOffset? PublishedAtUtc,
    string Provider,
    IReadOnlyList<Guid> SelectedCategoryIds);
