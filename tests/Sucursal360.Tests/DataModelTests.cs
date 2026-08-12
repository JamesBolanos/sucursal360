using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Domain.Entities;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.Security;

namespace Sucursal360.Tests;

[TestClass]
public sealed class DataModelTests
{
    [TestMethod]
    public async Task MigrationsCreateBusinessSchemaAndSeedBaselineData()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        Assert.AreEqual(5, await context.Branches.CountAsync());
        Assert.AreEqual(7, await context.ReviewCategories.CountAsync());
        Assert.AreEqual(3, await context.Roles.CountAsync());

        CollectionAssert.AreEquivalent(
            new[] { "SUC-001", "SUC-002", "SUC-003", "SUC-004", "SUC-005" },
            await context.Branches.Select(branch => branch.Code).ToArrayAsync());

        CollectionAssert.AreEquivalent(
            new[] { AppRoles.Administrator, AppRoles.CorporateManager, AppRoles.BranchManager },
            await context.Roles.Select(role => role.Name).ToArrayAsync());
    }

    [TestMethod]
    public void AverageTicketIsDerivedAndNotPersisted()
    {
        var metric = new SimulatedOperationalMetric
        {
            NetSales = 120m,
            TransactionCount = 3
        };

        Assert.AreEqual(40m, metric.AverageTicket);

        metric.TransactionCount = 0;
        Assert.IsNull(metric.AverageTicket);
    }

    [TestMethod]
    public async Task BranchCodeIsUnique()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        context.Branches.Add(new Branch
        {
            Id = Guid.NewGuid(),
            Code = "SUC-001",
            Name = "Cafe Horizonte Duplicado",
            Provider = PublicDataProvider.Demo,
            ExternalPlaceId = "DEMO-SUC-DUP",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });

        try
        {
            await context.SaveChangesAsync();
            Assert.Fail("Expected duplicate branch code to violate the unique index.");
        }
        catch (DbUpdateException)
        {
            // Expected: branch codes are part of the business identity.
        }
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        return new ApplicationDbContext(options);
    }
}
