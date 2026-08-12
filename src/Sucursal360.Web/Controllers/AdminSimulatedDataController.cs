using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Security;
using Sucursal360.Web.Services.SimulatedData;
using Sucursal360.Web.ViewModels.AdminSimulatedData;

namespace Sucursal360.Web.Controllers;

[Authorize(Policy = AppPolicies.CanAdministerSystem)]
[Route("admin/simulated-data")]
public class AdminSimulatedDataController(
    ApplicationDbContext dbContext,
    ISimulatedDataImportService simulatedDataImportService) : Controller
{
    [HttpGet("import")]
    public async Task<IActionResult> Import(CancellationToken cancellationToken)
    {
        return View(await BuildPageViewModelAsync(null, null, null, 0, cancellationToken));
    }

    [HttpPost("import/validate")]
    public async Task<IActionResult> Validate(IFormFile? csvFile, CancellationToken cancellationToken)
    {
        if (csvFile is null || csvFile.Length == 0)
        {
            ModelState.AddModelError(nameof(csvFile), "Seleccione un archivo CSV.");
            return View("Import", await BuildPageViewModelAsync(null, null, null, 0, cancellationToken));
        }

        var readResult = await ReadUtf8FileAsync(csvFile, cancellationToken);
        if (!readResult.IsValid)
        {
            ModelState.AddModelError(nameof(csvFile), readResult.ErrorMessage ?? "No se pudo leer el archivo.");
            return View("Import", await BuildPageViewModelAsync(null, null, null, 0, cancellationToken));
        }

        var preview = await simulatedDataImportService.ValidateAsync(
            readResult.Content,
            csvFile.FileName,
            csvFile.Length,
            cancellationToken);

        return View("Import", await BuildPageViewModelAsync(
            preview,
            null,
            preview.IsValid ? Convert.ToBase64String(Encoding.UTF8.GetBytes(readResult.Content)) : null,
            csvFile.Length,
            cancellationToken));
    }

    [HttpPost("import/confirm")]
    public async Task<IActionResult> Confirm(
        string encodedCsvContent,
        string fileName,
        long fileSizeBytes,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(encodedCsvContent))
        {
            ModelState.AddModelError(nameof(encodedCsvContent), "Valide un archivo antes de confirmar.");
            return View("Import", await BuildPageViewModelAsync(null, null, null, 0, cancellationToken));
        }

        string csvContent;
        try
        {
            csvContent = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCsvContent));
        }
        catch (FormatException)
        {
            ModelState.AddModelError(nameof(encodedCsvContent), "La vista previa expiro o no es valida.");
            return View("Import", await BuildPageViewModelAsync(null, null, null, 0, cancellationToken));
        }

        var importResult = await simulatedDataImportService.ImportAsync(
            csvContent,
            fileName,
            fileSizeBytes,
            GetUserId(),
            cancellationToken);

        return View("Import", await BuildPageViewModelAsync(null, importResult, null, 0, cancellationToken));
    }

    private async Task<SimulatedDataImportPageViewModel> BuildPageViewModelAsync(
        SimulatedCsvValidationResult? preview,
        SimulatedDataImportResult? importResult,
        string? encodedCsvContent,
        long fileSizeBytes,
        CancellationToken cancellationToken)
    {
        var recentImports = await dbContext.SimulatedDataImports
            .Select(import => new SimulatedDataImportHistoryItemViewModel(
                import.Id,
                import.FileName,
                import.RowCount,
                import.PeriodStart,
                import.PeriodEnd,
                import.ImportedAtUtc,
                import.ImportedByUser.Email ?? "No disponible"))
            .ToListAsync(cancellationToken);

        return new SimulatedDataImportPageViewModel(
            preview,
            importResult,
            encodedCsvContent,
            fileSizeBytes,
            recentImports
                .OrderByDescending(import => import.ImportedAtUtc)
                .Take(10)
                .ToList());
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Authenticated user id was not available.");
    }

    private static async Task<FileReadResult> ReadUtf8FileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, new UTF8Encoding(false, throwOnInvalidBytes: true), detectEncodingFromByteOrderMarks: true);
            return new FileReadResult(true, await reader.ReadToEndAsync(cancellationToken), null);
        }
        catch (DecoderFallbackException)
        {
            return new FileReadResult(false, string.Empty, "El archivo debe estar codificado en UTF-8 valido.");
        }
    }

    private sealed record FileReadResult(bool IsValid, string Content, string? ErrorMessage);
}
