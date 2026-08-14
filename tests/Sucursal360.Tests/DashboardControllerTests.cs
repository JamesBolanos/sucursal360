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

        var result = await controller.Index(new DashboardFiltersViewModel(), CancellationToken.None);

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
        Assert.AreEqual("Ventas", model.Insights[0].Label);
        Assert.AreEqual("Sin datos operativos.", model.Insights[0].Detail);
        Assert.AreEqual(SeedIds.BranchCentro, model.Ranking[0].BranchId);
        Assert.AreEqual(88, model.Ranking[0].RatingPercent);

        var unsynchronizedBranch = model.Branches.Single(branch => branch.BranchId == SeedIds.BranchGalerias);
        Assert.AreEqual("Sin sincronizar", unsynchronizedBranch.DataStatus);
        Assert.IsNull(unsynchronizedBranch.Rating);
    }

    [TestMethod]
    public async Task IndexBuildsManagerDashboardChartsForSelectedMonth()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        context.Users.Add(CreateAdminUser());
        var import = CreateImport();
        context.SimulatedDataImports.Add(import);

        var centroRun = CreateRun("SYNC-CENTRO", SeedIds.BranchCentro, new DateTimeOffset(2026, 8, 1, 10, 0, 0, TimeSpan.Zero));
        var carreteraRun = CreateRun("SYNC-CARRETERA", SeedIds.BranchCarreteraSur, new DateTimeOffset(2026, 8, 1, 10, 5, 0, TimeSpan.Zero));
        context.IntegrationRuns.AddRange(centroRun, carreteraRun);
        context.BranchSnapshots.AddRange(
            CreateSnapshot(centroRun, 3.7m, 40, new DateTimeOffset(2026, 8, 1, 10, 0, 0, TimeSpan.Zero)),
            CreateSnapshot(carreteraRun, 4.5m, 80, new DateTimeOffset(2026, 8, 1, 10, 5, 0, TimeSpan.Zero)));

        var centroReview = CreateReview("REV-CENTRO-SERVICIO", SeedIds.BranchCentro, 2);
        var carreteraReview = CreateReview("REV-CARRETERA-PRECIO", SeedIds.BranchCarreteraSur, 5);
        context.Reviews.AddRange(centroReview, carreteraReview);
        context.SimulatedOperationalMetrics.AddRange(
            CreateMetric(import.Id, SeedIds.BranchCentro, 1000m, 10),
            CreateMetric(import.Id, SeedIds.BranchCarreteraSur, 5000m, 25));
        await context.SaveChangesAsync();

        var controller = new DashboardController(context);
        var filters = new DashboardFiltersViewModel
        {
            Month = "2026-07"
        };

        var result = await controller.Index(filters, CancellationToken.None);

        var view = result as ViewResult;
        Assert.IsNotNull(view);
        var model = view.Model as CorporateDashboardViewModel;
        Assert.IsNotNull(model);

        Assert.AreEqual(6000m, model.OperationalSummary.NetSales);
        Assert.AreEqual(35, model.OperationalSummary.TransactionCount);
        Assert.AreEqual("2026-07", model.Filters.Month);
        Assert.IsTrue(model.MonthOptions.Contains("2026-07"));

        var centro = model.Branches.Single(branch => branch.BranchId == SeedIds.BranchCentro);
        Assert.AreEqual("Alta", centro.AttentionLevel);
        Assert.AreEqual(1000m, centro.NetSales);
        Assert.AreEqual(100m, centro.AverageTicket);
        Assert.AreEqual(1, centro.PeriodReviewCount);
        Assert.AreEqual(1, centro.NegativeReviewCount);

        Assert.AreEqual("Cafe Horizonte Carretera Sur lidera ventas", model.ExecutiveSummary.Headline);
        Assert.AreEqual("1 en atencion", model.ExecutiveSummary.RiskLabel);

        Assert.HasCount(2, model.SalesSlices);
        Assert.AreEqual("SUC-002", model.SalesSlices[0].Label);
        Assert.AreEqual(5000m, model.SalesSlices[0].Value);
        Assert.HasCount(2, model.TicketBars);
        Assert.AreEqual("SUC-002", model.TicketBars[0].Label);
        Assert.AreEqual(200m, model.TicketBars[0].AverageTicket);
        Assert.AreEqual(100, model.TicketBars[0].Percent);
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

    private static Review CreateReview(string externalReviewId, Guid branchId, byte rating)
    {
        return new Review
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            Provider = PublicDataProvider.Demo,
            ExternalReviewId = externalReviewId,
            Rating = rating,
            Text = "Resena demo",
            PublishedAtUtc = new DateTimeOffset(2026, 7, 10, 10, 0, 0, TimeSpan.Zero),
            AuthorDisplayName = "Cliente demo",
            RetrievedAtUtc = new DateTimeOffset(2026, 8, 1, 10, 0, 0, TimeSpan.Zero)
        };
    }

    private static ReviewCategoryAssignment CreateAssignment(Guid reviewId, Guid categoryId)
    {
        return new ReviewCategoryAssignment
        {
            ReviewId = reviewId,
            ReviewCategoryId = categoryId,
            AssignedByUserId = "admin-user",
            AssignedAtUtc = new DateTimeOffset(2026, 8, 1, 11, 0, 0, TimeSpan.Zero)
        };
    }

    private static SimulatedDataImport CreateImport()
    {
        return new SimulatedDataImport
        {
            Id = Guid.NewGuid(),
            FileName = "operacion.csv",
            RowCount = 2,
            PeriodStart = new DateOnly(2026, 7, 1),
            PeriodEnd = new DateOnly(2026, 7, 31),
            ImportedByUserId = "admin-user",
            ImportedAtUtc = new DateTimeOffset(2026, 8, 1, 12, 0, 0, TimeSpan.Zero)
        };
    }

    private static SimulatedOperationalMetric CreateMetric(
        Guid importId,
        Guid branchId,
        decimal netSales,
        int transactionCount)
    {
        return new SimulatedOperationalMetric
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            BusinessDate = new DateOnly(2026, 7, 10),
            NetSales = netSales,
            TransactionCount = transactionCount,
            Currency = "NIO",
            DataOrigin = DataOrigin.Simulated,
            ImportId = importId
        };
    }
}
