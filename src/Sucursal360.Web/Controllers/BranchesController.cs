using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Security;

namespace Sucursal360.Web.Controllers;

[Authorize]
public class BranchesController(
    ApplicationDbContext dbContext,
    IBranchAccessService branchAccessService) : Controller
{
    [HttpGet("/branches/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var branch = await dbContext.Branches
            .Where(branch => branch.Id == id)
            .Select(branch => new BranchDetailsViewModel(
                branch.Id,
                branch.Code,
                branch.Name,
                branch.IsActive,
                branch.Provider.ToString(),
                branch.ExternalPlaceId))
            .SingleOrDefaultAsync(cancellationToken);

        if (branch is null)
        {
            return NotFound();
        }

        if (!await branchAccessService.CanAccessAsync(User, id, cancellationToken))
        {
            return Forbid();
        }

        return View(branch);
    }
}

public sealed record BranchDetailsViewModel(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    string Provider,
    string? ExternalPlaceId);
