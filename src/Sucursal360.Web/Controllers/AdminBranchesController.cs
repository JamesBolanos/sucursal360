using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Domain.Entities;
using Sucursal360.Web.Security;
using Sucursal360.Web.ViewModels.AdminBranches;

namespace Sucursal360.Web.Controllers;

[Authorize(Policy = AppPolicies.CanAdministerSystem)]
[Route("admin/branches")]
public class AdminBranchesController(ApplicationDbContext dbContext) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var branches = await dbContext.Branches
            .OrderBy(branch => branch.Code)
            .Select(branch => new BranchListItemViewModel(
                branch.Id,
                branch.Code,
                branch.Name,
                branch.IsActive,
                branch.Provider.ToString(),
                branch.ExternalPlaceId ?? "No disponible",
                branch.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return View(branches);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(new BranchFormViewModel());
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(BranchFormViewModel model, CancellationToken cancellationToken)
    {
        Normalize(model);
        await ValidateUniquenessAsync(model, branchId: null, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var now = DateTimeOffset.UtcNow;
        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            Code = model.Code,
            Name = model.Name,
            IsActive = model.IsActive,
            Provider = model.Provider,
            ExternalPlaceId = model.ExternalPlaceId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        dbContext.Branches.Add(branch);
        await dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var branch = await dbContext.Branches.FindAsync([id], cancellationToken);
        if (branch is null)
        {
            return NotFound();
        }

        return View(ToForm(branch));
    }

    [HttpPost("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, BranchFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.Id != id)
        {
            return BadRequest();
        }

        Normalize(model);
        await ValidateUniquenessAsync(model, id, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var branch = await dbContext.Branches.FindAsync([id], cancellationToken);
        if (branch is null)
        {
            return NotFound();
        }

        branch.Code = model.Code;
        branch.Name = model.Name;
        branch.IsActive = model.IsActive;
        branch.Provider = model.Provider;
        branch.ExternalPlaceId = model.ExternalPlaceId;
        branch.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/activate")]
    public Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        return SetActiveAsync(id, isActive: true, cancellationToken);
    }

    [HttpPost("{id:guid}/deactivate")]
    public Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        return SetActiveAsync(id, isActive: false, cancellationToken);
    }

    private async Task<IActionResult> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var branch = await dbContext.Branches.FindAsync([id], cancellationToken);
        if (branch is null)
        {
            return NotFound();
        }

        branch.IsActive = isActive;
        branch.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateUniquenessAsync(BranchFormViewModel model, Guid? branchId, CancellationToken cancellationToken)
    {
        var duplicateCode = await dbContext.Branches
            .AnyAsync(branch => branch.Code == model.Code && branch.Id != branchId, cancellationToken);
        if (duplicateCode)
        {
            ModelState.AddModelError(nameof(model.Code), "Ya existe una sucursal con este codigo.");
        }

        var duplicateExternalId = await dbContext.Branches
            .AnyAsync(branch =>
                branch.Provider == model.Provider &&
                branch.ExternalPlaceId == model.ExternalPlaceId &&
                branch.Id != branchId,
                cancellationToken);
        if (duplicateExternalId)
        {
            ModelState.AddModelError(nameof(model.ExternalPlaceId), "Ya existe una sucursal con este proveedor e identificador.");
        }
    }

    private static void Normalize(BranchFormViewModel model)
    {
        model.Code = model.Code.Trim().ToUpperInvariant();
        model.Name = model.Name.Trim();
        model.ExternalPlaceId = model.ExternalPlaceId.Trim();
    }

    private static BranchFormViewModel ToForm(Branch branch)
    {
        return new BranchFormViewModel
        {
            Id = branch.Id,
            Code = branch.Code,
            Name = branch.Name,
            IsActive = branch.IsActive,
            Provider = branch.Provider,
            ExternalPlaceId = branch.ExternalPlaceId ?? string.Empty
        };
    }
}
