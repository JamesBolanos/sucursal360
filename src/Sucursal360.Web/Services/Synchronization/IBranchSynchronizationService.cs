namespace Sucursal360.Web.Services.Synchronization;

public interface IBranchSynchronizationService
{
    Task<SynchronizationResult> SynchronizeBranchAsync(Guid branchId, string triggeredByUserId, CancellationToken cancellationToken);

    Task<IReadOnlyList<SynchronizationResult>> SynchronizeAllActiveBranchesAsync(string triggeredByUserId, CancellationToken cancellationToken);
}
