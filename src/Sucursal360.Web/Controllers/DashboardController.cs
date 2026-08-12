using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Security;

namespace Sucursal360.Web.Controllers;

[Authorize(Policy = AppPolicies.CanViewCorporateDashboard)]
public class DashboardController(ApplicationDbContext dbContext) : Controller
{
    [HttpGet("/dashboard")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var branches = await dbContext.Branches
            .OrderBy(branch => branch.Code)
            .Select(branch => new DashboardBranchRow(
                branch.Id,
                branch.Code,
                branch.Name,
                branch.IsActive,
                branch.Provider.ToString(),
                branch.ExternalPlaceId))
            .ToListAsync(cancellationToken);

        return View(branches);
    }
}

public sealed record DashboardBranchRow(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    string Provider,
    string? ExternalPlaceId);
