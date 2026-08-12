using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Controllers;
using Sucursal360.Web.Data;
using Sucursal360.Web.Data.Seed;
using Sucursal360.Web.Domain.Entities;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.ViewModels.Dashboard;

namespace Sucursal360.Tests;

[TestClass]
public sealed class DashboardControllerTests
{
    [TestMethod]
    public async Task IndexBuildsLatestSnapshotComparisonForSqlite()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        context.Users.Add(CreateAdminUser());
        var olderRun = CreateRun("SYNC-OLDER", SeedIds.BranchCentro, new DateTimeOffset(2026, 8, 12, 9, 0, 0, TimeSpan.Zero));
        var latestRun = CreateRun("SYNC-LATEST", SeedIds.BranchCentro, new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero));
        context.IntegrationRuns.AddRange(olderRun, latestRun);
        context.BranchSnapshots.AddRange(
            CreateSnapshot(olderRun, 4.1m, 100, new DateTimeOffset(2026, 8, 12, 9, 0, 0, TimeSpan.Zero)),
            CreateSnapshot(latestRun, 4.4m, 115, new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero)));
        await context.SaveChangesAsync();

        var controller = new DashboardController(context);

        var result = await controller.Index(CancellationToken.None);

        var view = result as ViewResult;
        Assert.IsNotNull(view);
        var model = view.Model as CorporateDashboardViewModel;
        Assert.IsNotNull(model);

        Assert.AreEqual(5, model.Summary.TotalBranches);
        Assert.AreEqual(1, model.Summary.BranchesWithSnapshots);
        Assert.AreEqual(4.4m, model.Summary.AverageRating);
        Assert.AreEqual(115, model.Summary.TotalReviewCount);
        Assert.AreEqual(new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero), model.Summary.LastSynchronizationAtUtc);

        var branch = model.Branches.Single(branch => branch.BranchId == SeedIds.BranchCentro);
        Assert.AreEqual(4.4m, branch.Rating);
        Assert.AreEqual(0.3m, branch.RatingDelta);
        Assert.AreEqual(115, branch.ReviewCount);
        Assert.AreEqual(15, branch.ReviewCountDelta);
        Assert.AreEqual("Actualizado", branch.DataStatus);
        Assert.AreEqual("Operando", branch.BusinessStatus);

        Assert.HasCount(4, model.Insights);
        Assert.AreEqual("Mejor reputacion", model.Insights[0].Label);
        Assert.AreEqual("Cafe Horizonte Centro", model.Insights[0].Detail);
        Assert.AreEqual(SeedIds.BranchCentro, model.Ranking[0].BranchId);
        Assert.AreEqual(88, model.Ranking[0].RatingPercent);

        var unsynchronizedBranch = model.Branches.Single(branch => branch.BranchId == SeedIds.BranchGalerias);
        Assert.AreEqual("Sin sincronizar", unsynchronizedBranch.DataStatus);
        Assert.IsNull(unsynchronizedBranch.Rating);
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

    private static IntegrationRun CreateRun(string correlationId, Guid branchId, DateTimeOffset startedAtUtc)
    {
        return new IntegrationRun
        {
            Id = Guid.NewGuid(),
            CorrelationId = correlationId,
            Provider = PublicDataProvider.Demo,
            BranchId = branchId,
            StartedAtUtc = startedAtUtc,
            FinishedAtUtc = startedAtUtc.AddSeconds(1),
            Status = IntegrationRunStatus.Successful,
            RecordsReceived = 4,
            RecordsStored = 4,
            UserMessage = "La sincronizacion finalizo correctamente.",
            TriggeredByUserId = "admin-user"
        };
    }

    private static BranchSnapshot CreateSnapshot(
        IntegrationRun run,
        decimal rating,
        int reviewCount,
        DateTimeOffset retrievedAtUtc)
    {
        return new BranchSnapshot
        {
            Id = Guid.NewGuid(),
            BranchId = run.BranchId,
            Provider = PublicDataProvider.Demo,
            DisplayName = "Cafe Horizonte Centro",
            Address = "Managua",
            BusinessStatus = BusinessStatus.Operational,
            Rating = rating,
            ReviewCount = reviewCount,
            RetrievedAtUtc = retrievedAtUtc,
            IntegrationRunId = run.Id
        };
    }
}
