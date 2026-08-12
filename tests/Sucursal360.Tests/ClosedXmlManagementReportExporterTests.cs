using ClosedXML.Excel;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Data.Seed;
using Sucursal360.Web.Domain.Entities;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.Services.Reports;

namespace Sucursal360.Tests;

[TestClass]
public sealed class ClosedXmlManagementReportExporterTests
{
    [TestMethod]
    public async Task ExportCreatesWorkbookSheetsAndNeutralizesFormulaLikeText()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        var branchId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var importId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        context.Users.Add(CreateAdminUser());
        context.Branches.Add(new Branch
        {
            Id = branchId,
            Code = "SUC-099",
            Name = "=Formula Branch",
            IsActive = true,
            Provider = PublicDataProvider.Demo,
            ExternalPlaceId = "DEMO-SUC-099",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });
        context.IntegrationRuns.Add(new IntegrationRun
        {
            Id = runId,
            CorrelationId = "SYNC-REPORT",
            Provider = PublicDataProvider.Demo,
            BranchId = branchId,
            StartedAtUtc = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero),
            FinishedAtUtc = new DateTimeOffset(2026, 8, 12, 10, 0, 1, TimeSpan.Zero),
            Status = IntegrationRunStatus.Successful,
            TriggeredByUserId = "admin-user"
        });
        context.BranchSnapshots.Add(new BranchSnapshot
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            Provider = PublicDataProvider.Demo,
            BusinessStatus = BusinessStatus.Operational,
            Rating = 4.6m,
            ReviewCount = 40,
            RetrievedAtUtc = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero),
            IntegrationRunId = runId
        });
        context.Reviews.Add(new Review
        {
            Id = reviewId,
            BranchId = branchId,
            Provider = PublicDataProvider.Demo,
            ExternalReviewId = "REV-REPORT",
            Rating = 5,
            Text = "Great",
            PublishedAtUtc = new DateTimeOffset(2026, 8, 12, 9, 0, 0, TimeSpan.Zero),
            RetrievedAtUtc = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero)
        });
        context.ReviewCategoryAssignments.Add(new ReviewCategoryAssignment
        {
            ReviewId = reviewId,
            ReviewCategoryId = SeedIds.CategoryCalidad,
            AssignedByUserId = "admin-user",
            AssignedAtUtc = DateTimeOffset.UtcNow
        });
        context.SimulatedDataImports.Add(new SimulatedDataImport
        {
            Id = importId,
            FileName = "ops.csv",
            RowCount = 1,
            PeriodStart = new DateOnly(2026, 8, 12),
            PeriodEnd = new DateOnly(2026, 8, 12),
            ImportedByUserId = "admin-user",
            ImportedAtUtc = DateTimeOffset.UtcNow
        });
        context.SimulatedOperationalMetrics.Add(new SimulatedOperationalMetric
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            BusinessDate = new DateOnly(2026, 8, 12),
            NetSales = 1000m,
            TransactionCount = 100,
            Currency = "NIO",
            DataOrigin = DataOrigin.Simulated,
            ImportId = importId
        });
        await context.SaveChangesAsync();

        var exporter = new ClosedXmlManagementReportExporter(context);

        var result = await exporter.ExportAsync(
            new ManagementReportRequest(new DateOnly(2026, 8, 12), new DateOnly(2026, 8, 12), branchId, null),
            CancellationToken.None);

        Assert.IsTrue(result.FileName.StartsWith("Sucursal360_Reporte_", StringComparison.Ordinal));
        Assert.IsTrue(result.FileName.EndsWith(".xlsx", StringComparison.Ordinal));
        Assert.AreEqual("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.ContentType);
        Assert.IsNotEmpty(result.Content);

        using var stream = new MemoryStream(result.Content);
        using var workbook = new XLWorkbook(stream);
        CollectionAssert.AreEquivalent(
            new[] { "Resumen", "Sucursales", "Tendencias", "Categorias", "Operacion_Simulada" },
            workbook.Worksheets.Select(sheet => sheet.Name).ToArray());

        Assert.AreEqual("DATOS SIMULADOS", workbook.Worksheet("Resumen").Cell(6, 2).GetString());
        Assert.AreEqual("=Formula Branch", workbook.Worksheet("Sucursales").Cell(2, 2).GetString());
        Assert.IsFalse(workbook.Worksheet("Sucursales").Cell(2, 2).HasFormula);
        Assert.AreEqual("DATOS SIMULADOS", workbook.Worksheet("Operacion_Simulada").Cell(1, 1).GetString());
        Assert.AreEqual(10m, workbook.Worksheet("Operacion_Simulada").Cell(3, 6).GetValue<decimal>());
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ApplicationUser CreateAdminUser()
    {
        return new ApplicationUser
        {
            Id = "admin-user",
            UserName = "admin@sucursal360.local",
            Email = "admin@sucursal360.local",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }
}
