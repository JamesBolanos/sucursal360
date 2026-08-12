using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;

namespace Sucursal360.Web.Security;

public sealed class BranchAccessService(ApplicationDbContext dbContext) : IBranchAccessService
{
    public async Task<bool> CanAccessAsync(ClaimsPrincipal user, Guid branchId, CancellationToken cancellationToken)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (user.IsInRole(AppRoles.Administrator) || user.IsInRole(AppRoles.CorporateManager))
        {
            return true;
        }

        if (!user.IsInRole(AppRoles.BranchManager))
        {
            return false;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        return await dbContext.Users
            .Where(applicationUser => applicationUser.Id == userId && applicationUser.IsActive)
            .AnyAsync(applicationUser => applicationUser.AssignedBranchId == branchId, cancellationToken);
    }
}
