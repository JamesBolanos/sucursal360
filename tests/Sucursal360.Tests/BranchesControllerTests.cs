using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Controllers;
using Sucursal360.Web.Data;
using Sucursal360.Web.Data.Seed;
using Sucursal360.Web.Domain.Entities;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.Security;
using Sucursal360.Web.ViewModels.Branches;

namespace Sucursal360.Tests;

[TestClass]
public sealed class BranchesControllerTests
{
    [TestMethod]
    public async Task DetailsBuildsLatestSnapshotTrendAndLastRunForAccessibleBranch()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        context.Users.Add(CreateAdminUser());
        var olderRun = CreateRun("SYNC-OLDER", new DateTimeOffset(2026, 8, 12, 9, 0, 0, TimeSpan.Zero));
        var latestRun = CreateRun("SYNC-LATEST", new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero));
        context.IntegrationRuns.AddRange(olderRun, latestRun);
        context.BranchSnapshots.AddRange(
            CreateSnapshot(olderRun, 4.0m, 90, new DateTimeOffset(2026, 8, 12, 9, 0, 0, TimeSpan.Zero)),
            CreateSnapshot(latestRun, 4.5m, 120, new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero)));
        await context.SaveChangesAsync();

        var controller = CreateController(context, canAccess: true);

        var result = await controller.Details(SeedIds.BranchCentro, CancellationToken.None);

        var view = result as ViewResult;
        Assert.IsNotNull(view);
        var model = view.Model as BranchDetailsViewModel;
        Assert.IsNotNull(model);

        Assert.AreEqual(SeedIds.BranchCentro, model.Id);
        Assert.AreEqual("SUC-001", model.Code);
        Assert.IsNotNull(model.LatestSnapshot);
        Assert.AreEqual(4.5m, model.LatestSnapshot.Rating);
        Assert.AreEqual(0.5m, model.LatestSnapshot.RatingDelta);
        Assert.AreEqual(120, model.LatestSnapshot.ReviewCount);
        Assert.AreEqual(30, model.LatestSnapshot.ReviewCountDelta);
        Assert.AreEqual("Operando", model.LatestSnapshot.BusinessStatus);

        Assert.IsNotNull(model.LastIntegrationRun);
        Assert.AreEqual("SYNC-LATEST", model.LastIntegrationRun.CorrelationId);
        Assert.AreEqual("Successful", model.LastIntegrationRun.Status);

        Assert.HasCount(2, model.Trend);
        Assert.AreEqual(80, model.Trend[0].RatingPercent);
        Assert.AreEqual(100, model.Trend[1].ReviewCountPercent);
        Assert.HasCount(2, model.SnapshotHistory);
        Assert.AreEqual(new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero), model.SnapshotHistory[0].RetrievedAtUtc);
    }

    [TestMethod]
    public async Task DetailsForbidsWhenBranchAccessServiceRejectsUser()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        var controller = CreateController(context, canAccess: false);

        var result = await controller.Details(SeedIds.BranchCentro, CancellationToken.None);

        Assert.IsInstanceOfType<ForbidResult>(result);
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static BranchesController CreateController(ApplicationDbContext context, bool canAccess)
    {
        return new BranchesController(context, new StubBranchAccessService(canAccess))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreatePrincipal()
                }
            }
        };
    }

    private static ClaimsPrincipal CreatePrincipal()
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "admin-user"),
                new Claim(ClaimTypes.Role, AppRoles.Administrator)
            },
            authenticationType: "Test");

        return new ClaimsPrincipal(identity);
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

    private static IntegrationRun CreateRun(string correlationId, DateTimeOffset startedAtUtc)
    {
        return new IntegrationRun
        {
            Id = Guid.NewGuid(),
            CorrelationId = correlationId,
            Provider = PublicDataProvider.Demo,
            BranchId = SeedIds.BranchCentro,
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

    private sealed class StubBranchAccessService(bool canAccess) : IBranchAccessService
    {
        public Task<bool> CanAccessAsync(ClaimsPrincipal user, Guid branchId, CancellationToken cancellationToken)
        {
            return Task.FromResult(canAccess);
        }
    }
}
