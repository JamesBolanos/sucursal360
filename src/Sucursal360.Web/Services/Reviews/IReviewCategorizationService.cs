namespace Sucursal360.Web.Services.Reviews;

public interface IReviewCategorizationService
{
    Task ReplaceCategoriesAsync(
        Guid reviewId,
        IReadOnlyCollection<Guid> selectedCategoryIds,
        string changedByUserId,
        CancellationToken cancellationToken);
}
