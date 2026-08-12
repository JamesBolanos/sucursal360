using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sucursal360.Web.Data;
using Sucursal360.Web.Data.Seed;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.Integrations.Demo;
using Sucursal360.Web.Services.Synchronization;

namespace Sucursal360.Tests;

[TestClass]
public sealed class BranchSynchronizationServiceTests
{
    [TestMethod]
    public async Task SynchronizeBranchPersistsSnapshotReviewsAndSuccessfulRun()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        context.Users.Add(CreateAdminUser());
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.SynchronizeBranchAsync(SeedIds.BranchCentro, "admin-user", CancellationToken.None);

        Assert.AreEqual(IntegrationRunStatus.Successful, result.Status);
        Assert.AreEqual(4, result.RecordsReceived);
        Assert.AreEqual(4, result.RecordsStored);
        Assert.AreEqual(1, await context.BranchSnapshots.CountAsync());
        Assert.AreEqual(3, await context.Reviews.CountAsync());
        Assert.AreEqual(1, await context.IntegrationRuns.CountAsync());
    }

    [TestMethod]
    public async Task PartialProviderDataPersistsUsableValuesAndMarksRunPartial()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        context.Users.Add(CreateAdminUser());
        var branch = await context.Branches.SingleAsync(branch => branch.Id == SeedIds.BranchCentro);
        branch.ExternalPlaceId = "PARTIAL-SUC";
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.SynchronizeBranchAsync(SeedIds.BranchCentro, "admin-user", CancellationToken.None);

        Assert.AreEqual(IntegrationRunStatus.Partial, result.Status);
        Assert.AreEqual(1, await context.BranchSnapshots.CountAsync());
        Assert.AreEqual(1, await context.Reviews.CountAsync());

        var snapshot = await context.BranchSnapshots.SingleAsync();
        Assert.IsNull(snapshot.Rating);
        Assert.IsNull(snapshot.ReviewCount);
    }

    [TestMethod]
    public async Task FailedProviderCallKeepsPreviousValidData()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        context.Users.Add(CreateAdminUser());
        await context.SaveChangesAsync();

        var service = CreateService(context);
        await service.SynchronizeBranchAsync(SeedIds.BranchCentro, "admin-user", CancellationToken.None);

        var branch = await context.Branches.SingleAsync(branch => branch.Id == SeedIds.BranchCentro);
        branch.ExternalPlaceId = "MISSING-SUC";
        await context.SaveChangesAsync();

        var result = await service.SynchronizeBranchAsync(SeedIds.BranchCentro, "admin-user", CancellationToken.None);

        Assert.AreEqual(IntegrationRunStatus.Failed, result.Status);
        Assert.AreEqual("INT-404-PLACE", result.ErrorCode);
        Assert.AreEqual(1, await context.BranchSnapshots.CountAsync());
        Assert.AreEqual(3, await context.Reviews.CountAsync());
        Assert.AreEqual(2, await context.IntegrationRuns.CountAsync());
    }

    [TestMethod]
    public async Task UnsupportedConfiguredProviderCreatesFailedRunWithoutPersistingData()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        context.Users.Add(CreateAdminUser());
        var branch = await context.Branches.SingleAsync(branch => branch.Id == SeedIds.BranchCentro);
        branch.Provider = PublicDataProvider.GooglePlaces;
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.SynchronizeBranchAsync(SeedIds.BranchCentro, "admin-user", CancellationToken.None);

        Assert.AreEqual(IntegrationRunStatus.Failed, result.Status);
        Assert.AreEqual("INT-400-PROVIDER", result.ErrorCode);
        Assert.AreEqual(1, await context.IntegrationRuns.CountAsync());
        Assert.AreEqual(0, await context.BranchSnapshots.CountAsync());
        Assert.AreEqual(0, await context.Reviews.CountAsync());
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static BranchSynchronizationService CreateService(ApplicationDbContext context)
    {
        var environment = new TestWebHostEnvironment
        {
            ContentRootPath = Path.Combine(AppContext.BaseDirectory, "SyncContentRoot", Guid.NewGuid().ToString("N"))
        };

        Directory.CreateDirectory(environment.ContentRootPath);
        CopyFixtures(environment.ContentRootPath);

        var provider = new DemoPublicBranchDataProvider(
            environment,
            Options.Create(new DemoPublicDataOptions()));

        return new BranchSynchronizationService(
            context,
            provider,
            NullLogger<BranchSynchronizationService>.Instance);
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

    private static void CopyFixtures(string contentRootPath)
    {
        var sourceRoot = FindRepoRoot();
        var sourceFixtures = Path.Combine(sourceRoot, "src", "Sucursal360.Web", "Integrations", "Demo", "Fixtures");
        var targetFixtures = Path.Combine(contentRootPath, "Integrations", "Demo", "Fixtures");

        Directory.CreateDirectory(targetFixtures);
        foreach (var fixture in Directory.GetFiles(sourceFixtures, "*.json"))
        {
            File.Copy(fixture, Path.Combine(targetFixtures, Path.GetFileName(fixture)), overwrite: true);
        }
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Sucursal360.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";

        public string ApplicationName { get; set; } = "Sucursal360.Tests";

        public string WebRootPath { get; set; } = string.Empty;

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

        public string ContentRootPath { get; set; } = string.Empty;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
