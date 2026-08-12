using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Controllers;
using Sucursal360.Web.Data;
using Sucursal360.Web.Data.Seed;
using Sucursal360.Web.Domain.Entities;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.Services.Synchronization;
using Sucursal360.Web.ViewModels.AdminIntegrations;

namespace Sucursal360.Tests;

[TestClass]
public sealed class AdminIntegrationsControllerTests
{
    [TestMethod]
    public async Task IndexOrdersRecentRunsOnClientForSqlite()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        context.Users.Add(CreateAdminUser());
        context.IntegrationRuns.AddRange(
            CreateRun("SYNC-OLD", SeedIds.BranchCentro, new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero)),
            CreateRun("SYNC-NEW", SeedIds.BranchCentro, new DateTimeOffset(2026, 8, 12, 11, 0, 0, TimeSpan.Zero)));
        await context.SaveChangesAsync();

        var controller = new AdminIntegrationsController(context, new StubSynchronizationService());

        var result = await controller.Index(CancellationToken.None);

        var view = result as ViewResult;
        Assert.IsNotNull(view);
        var model = view.Model as IntegrationDashboardViewModel;
        Assert.IsNotNull(model);
        Assert.HasCount(2, model.RecentRuns);
        Assert.AreEqual("SYNC-NEW", model.RecentRuns[0].CorrelationId);
        Assert.AreEqual("SYNC-OLD", model.RecentRuns[1].CorrelationId);
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

    private sealed class StubSynchronizationService : IBranchSynchronizationService
    {
        public Task<SynchronizationResult> SynchronizeBranchAsync(
            Guid branchId,
            string triggeredByUserId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<SynchronizationResult>> SynchronizeAllActiveBranchesAsync(
            string triggeredByUserId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
