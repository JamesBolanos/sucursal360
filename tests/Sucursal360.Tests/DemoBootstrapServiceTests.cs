using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sucursal360.Web.Data;
using Sucursal360.Web.Data.Seed;
using Sucursal360.Web.Domain.Entities;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.Services.DemoBootstrap;
using Sucursal360.Web.Services.SimulatedData;
using Sucursal360.Web.Services.Synchronization;

namespace Sucursal360.Tests;

[TestClass]
public sealed class DemoBootstrapServiceTests
{
    [TestMethod]
    public async Task BootstrapSeedsDemoDataAndDefaultReviewCategories()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        context.Users.Add(CreateAdminUser());
        await context.SaveChangesAsync();

        var csvPath = Path.Combine(Path.GetTempPath(), $"sucursal360-demo-{Guid.NewGuid():N}.csv");
        await File.WriteAllTextAsync(csvPath, "business_date,branch_code,net_sales,transaction_count,currency,data_origin\n");

        var service = new DemoBootstrapService(
            context,
            new StubBranchSynchronizationService(context),
            new StubSimulatedDataImportService(context),
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["SeedUsers:Administrator:Email"] = "admin@sucursal360.local"
                })
                .Build(),
            new StubWebHostEnvironment(),
            Options.Create(new DemoBootstrapOptions
            {
                Enabled = true,
                OperationalMetricsCsvPath = csvPath
            }),
            NullLogger<DemoBootstrapService>.Instance);

        await service.BootstrapAsync(CancellationToken.None);

        Assert.AreEqual(1, await context.BranchSnapshots.CountAsync());
        Assert.AreEqual(1, await context.SimulatedOperationalMetrics.CountAsync());
        Assert.AreEqual(1, await context.Reviews.CountAsync());
        Assert.IsTrue(await context.ReviewCategoryAssignments.AnyAsync());
        Assert.IsTrue(await context.ReviewCategoryAudits.AnyAsync());
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
            NormalizedEmail = "ADMIN@SUCURSAL360.LOCAL",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    private sealed class StubBranchSynchronizationService(ApplicationDbContext context) : IBranchSynchronizationService
    {
        public async Task<SynchronizationResult> SynchronizeBranchAsync(
            Guid branchId,
            string triggeredByUserId,
            CancellationToken cancellationToken)
        {
            var run = new IntegrationRun
            {
                Id = Guid.NewGuid(),
                CorrelationId = "SYNC-DEMO-BOOTSTRAP",
                Provider = PublicDataProvider.Demo,
                BranchId = branchId,
                StartedAtUtc = DateTimeOffset.UtcNow,
                FinishedAtUtc = DateTimeOffset.UtcNow,
                Status = IntegrationRunStatus.Successful,
                RecordsReceived = 2,
                RecordsStored = 2,
                UserMessage = "La sincronizacion finalizo correctamente.",
                TriggeredByUserId = triggeredByUserId
            };

            context.IntegrationRuns.Add(run);
            context.BranchSnapshots.Add(new BranchSnapshot
            {
                Id = Guid.NewGuid(),
                BranchId = branchId,
                Provider = PublicDataProvider.Demo,
                DisplayName = "Cafe Horizonte Centro",
                Address = "Managua",
                BusinessStatus = BusinessStatus.Operational,
                Rating = 4.2m,
                ReviewCount = 1,
                RetrievedAtUtc = DateTimeOffset.UtcNow,
                IntegrationRunId = run.Id
            });
            context.Reviews.Add(new Review
            {
                Id = Guid.NewGuid(),
                BranchId = branchId,
                Provider = PublicDataProvider.Demo,
                ExternalReviewId = "BOOTSTRAP-REVIEW-001",
                Rating = 5,
                Text = "Excelente servicio y cafe consistente.",
                PublishedAtUtc = DateTimeOffset.UtcNow,
                AuthorDisplayName = "Cliente demo",
                RetrievedAtUtc = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync(cancellationToken);

            return new SynchronizationResult(branchId, "SUC-001", "Cafe Horizonte Centro", run.Id, run.CorrelationId, run.Status, 2, 2, run.UserMessage, null);
        }

        public async Task<IReadOnlyList<SynchronizationResult>> SynchronizeAllActiveBranchesAsync(
            string triggeredByUserId,
            CancellationToken cancellationToken)
        {
            return [await SynchronizeBranchAsync(SeedIds.BranchCentro, triggeredByUserId, cancellationToken)];
        }
    }

    private sealed class StubSimulatedDataImportService(ApplicationDbContext context) : ISimulatedDataImportService
    {
        public Task<SimulatedCsvValidationResult> ValidateAsync(
            string csvContent,
            string fileName,
            long fileSizeBytes,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new SimulatedCsvValidationResult(fileName, [], []));
        }

        public async Task<SimulatedDataImportResult> ImportAsync(
            string csvContent,
            string fileName,
            long fileSizeBytes,
            string importedByUserId,
            CancellationToken cancellationToken)
        {
            var import = new SimulatedDataImport
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                RowCount = 1,
                PeriodStart = new DateOnly(2026, 7, 1),
                PeriodEnd = new DateOnly(2026, 7, 1),
                ImportedByUserId = importedByUserId,
                ImportedAtUtc = DateTimeOffset.UtcNow
            };
            context.SimulatedDataImports.Add(import);
            context.SimulatedOperationalMetrics.Add(new SimulatedOperationalMetric
            {
                Id = Guid.NewGuid(),
                BranchId = SeedIds.BranchCentro,
                BusinessDate = new DateOnly(2026, 7, 1),
                NetSales = 1000m,
                TransactionCount = 10,
                Currency = "NIO",
                DataOrigin = DataOrigin.Simulated,
                ImportId = import.Id
            });
            await context.SaveChangesAsync(cancellationToken);

            return new SimulatedDataImportResult(import.Id, import.FileName, import.RowCount, import.PeriodStart, import.PeriodEnd);
        }
    }

    private sealed class StubWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Sucursal360.Tests";

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        public string EnvironmentName { get; set; } = "Development";

        public string WebRootPath { get; set; } = Directory.GetCurrentDirectory();

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
