using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sucursal360.Web.Data;
using Sucursal360.Web.Data.Seed;
using Sucursal360.Web.Domain.Entities;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.Security;
using Sucursal360.Web.Services.SimulatedData;
using Sucursal360.Web.Services.Synchronization;

namespace Sucursal360.Web.Services.DemoBootstrap;

public sealed class DemoBootstrapService(
    ApplicationDbContext dbContext,
    IBranchSynchronizationService branchSynchronizationService,
    ISimulatedDataImportService simulatedDataImportService,
    IConfiguration configuration,
    IWebHostEnvironment environment,
    IOptions<DemoBootstrapOptions> options,
    ILogger<DemoBootstrapService> logger) : IDemoBootstrapService
{
    public async Task BootstrapAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return;
        }

        var adminUser = await GetAdminUserAsync(cancellationToken);
        if (adminUser is null)
        {
            logger.LogWarning("Demo bootstrap was skipped because the administrator demo user is not available.");
            return;
        }

        await SynchronizeDemoBranchesAsync(adminUser.Id, cancellationToken);
        await ImportOperationalMetricsAsync(adminUser.Id, cancellationToken);
        await AssignDefaultReviewCategoriesAsync(adminUser.Id, cancellationToken);
    }

    private async Task<ApplicationUser?> GetAdminUserAsync(CancellationToken cancellationToken)
    {
        var adminEmail = configuration["SeedUsers:Administrator:Email"] ?? "admin@sucursal360.local";
        return await dbContext.Users.SingleOrDefaultAsync(user => user.Email == adminEmail, cancellationToken);
    }

    private async Task SynchronizeDemoBranchesAsync(string adminUserId, CancellationToken cancellationToken)
    {
        if (await dbContext.BranchSnapshots.AnyAsync(cancellationToken))
        {
            return;
        }

        var results = await branchSynchronizationService.SynchronizeAllActiveBranchesAsync(adminUserId, cancellationToken);
        logger.LogInformation(
            "Demo bootstrap synchronized {Count} branches.",
            results.Count(result => result.Status is IntegrationRunStatus.Successful or IntegrationRunStatus.Partial));
    }

    private async Task ImportOperationalMetricsAsync(string adminUserId, CancellationToken cancellationToken)
    {
        if (await dbContext.SimulatedOperationalMetrics.AnyAsync(cancellationToken))
        {
            return;
        }

        var csvPath = ResolveOperationalMetricsCsvPath();
        if (csvPath is null)
        {
            logger.LogWarning("Demo bootstrap could not find the sample operational metrics CSV.");
            return;
        }

        var csvContent = await File.ReadAllTextAsync(csvPath, cancellationToken);
        var fileInfo = new FileInfo(csvPath);
        await simulatedDataImportService.ImportAsync(
            csvContent,
            fileInfo.Name,
            fileInfo.Length,
            adminUserId,
            cancellationToken);

        logger.LogInformation("Demo bootstrap imported sample operational metrics.");
    }

    private string? ResolveOperationalMetricsCsvPath()
    {
        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(options.Value.OperationalMetricsCsvPath))
        {
            candidates.Add(options.Value.OperationalMetricsCsvPath);
        }

        candidates.Add(Path.Combine(environment.ContentRootPath, "samples", "simulated-operational-metrics.csv"));
        candidates.Add(Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "samples", "simulated-operational-metrics.csv")));
        candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "samples", "simulated-operational-metrics.csv"));

        return candidates.FirstOrDefault(File.Exists);
    }

    private async Task AssignDefaultReviewCategoriesAsync(string adminUserId, CancellationToken cancellationToken)
    {
        var categoryIds = await dbContext.ReviewCategories
            .Where(category => category.IsActive)
            .Select(category => new { category.Id, category.Code })
            .ToDictionaryAsync(category => category.Code, category => category.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var reviews = await dbContext.Reviews
            .Where(review => !review.CategoryAssignments.Any())
            .Select(review => new
            {
                review.Id,
                review.Rating,
                review.Text
            })
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var assignmentCount = 0;
        foreach (var review in reviews)
        {
            foreach (var categoryId in InferCategories(review.Text, categoryIds))
            {
                dbContext.ReviewCategoryAssignments.Add(new ReviewCategoryAssignment
                {
                    ReviewId = review.Id,
                    ReviewCategoryId = categoryId,
                    AssignedByUserId = adminUserId,
                    AssignedAtUtc = now
                });
                dbContext.ReviewCategoryAudits.Add(new ReviewCategoryAudit
                {
                    Id = Guid.NewGuid(),
                    ReviewId = review.Id,
                    ReviewCategoryId = categoryId,
                    Action = CategoryAuditAction.Assigned,
                    ChangedByUserId = adminUserId,
                    ChangedAtUtc = now
                });
                assignmentCount++;
            }
        }

        if (assignmentCount > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Demo bootstrap assigned {Count} review categories.", assignmentCount);
        }
    }

    private static IReadOnlyList<Guid> InferCategories(
        string? text,
        IReadOnlyDictionary<string, Guid> categoryIds)
    {
        var normalizedText = (text ?? string.Empty).ToLowerInvariant();
        var results = new List<Guid>();

        AddIfMatches(results, categoryIds, "SERVICIO", normalizedText, ["personal", "atencion", "servicio"]);
        AddIfMatches(results, categoryIds, "ESPERA", normalizedText, ["espera", "lento", "rapido", "rapida", "fila"]);
        AddIfMatches(results, categoryIds, "CALIDAD", normalizedText, ["cafe", "bebida", "postre", "sabor", "consistente", "presentacion"]);
        AddIfMatches(results, categoryIds, "LIMPIEZA", normalizedText, ["limpia", "limpio", "sucia", "sucio", "mesa", "higiene"]);
        AddIfMatches(results, categoryIds, "PRECIO", normalizedText, ["precio", "caro", "promocion", "valor"]);
        AddIfMatches(results, categoryIds, "INSTALACIONES", normalizedText, ["ambiente", "parqueo", "ruido", "espacio", "comodo"]);

        if (results.Count == 0 && categoryIds.TryGetValue("OTROS", out var otrosId))
        {
            results.Add(otrosId);
        }

        return results.Distinct().ToList();
    }

    private static void AddIfMatches(
        List<Guid> results,
        IReadOnlyDictionary<string, Guid> categoryIds,
        string categoryCode,
        string text,
        IReadOnlyList<string> keywords)
    {
        if (categoryIds.TryGetValue(categoryCode, out var categoryId) &&
            keywords.Any(keyword => text.Contains(keyword, StringComparison.Ordinal)))
        {
            results.Add(categoryId);
        }
    }
}
