using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Security;
using Sucursal360.Web.Services.Reports;
using Sucursal360.Web.ViewModels.Reports;

namespace Sucursal360.Web.Controllers;

[Authorize(Policy = AppPolicies.CanExportManagementReport)]
[Route("reports/management")]
public class ReportsController(
    ApplicationDbContext dbContext,
    IManagementReportExporter managementReportExporter) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Management(
        [FromQuery] ManagementReportFiltersViewModel filters,
        CancellationToken cancellationToken)
    {
        return View(await BuildPageViewModelAsync(filters, cancellationToken));
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] ManagementReportFiltersViewModel filters,
        CancellationToken cancellationToken)
    {
        if (filters.FromDate is not null && filters.ToDate is not null && filters.ToDate < filters.FromDate)
        {
            ModelState.AddModelError(nameof(filters.ToDate), "La fecha final debe ser mayor o igual a la inicial.");
            return View("Management", await BuildPageViewModelAsync(filters, cancellationToken));
        }

        var result = await managementReportExporter.ExportAsync(
            new ManagementReportRequest(filters.FromDate, filters.ToDate, filters.BranchId, filters.CategoryId),
            cancellationToken);

        return File(result.Content, result.ContentType, result.FileName);
    }

    private async Task<ManagementReportPageViewModel> BuildPageViewModelAsync(
        ManagementReportFiltersViewModel filters,
        CancellationToken cancellationToken)
    {
        var branches = await dbContext.Branches
            .OrderBy(branch => branch.Code)
            .Select(branch => new ReportBranchOptionViewModel(branch.Id, branch.Code, branch.Name))
            .ToListAsync(cancellationToken);

        var categories = await dbContext.ReviewCategories
            .Where(category => category.IsActive)
            .OrderBy(category => category.Name)
            .Select(category => new ReportCategoryOptionViewModel(category.Id, category.Name))
            .ToListAsync(cancellationToken);

        return new ManagementReportPageViewModel(filters, branches, categories);
    }
}
