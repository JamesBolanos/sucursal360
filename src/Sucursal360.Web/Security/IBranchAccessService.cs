using System.Security.Claims;

namespace Sucursal360.Web.Security;

public interface IBranchAccessService
{
    Task<bool> CanAccessAsync(ClaimsPrincipal user, Guid branchId, CancellationToken cancellationToken);
}
