using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Data.Seed;
using Sucursal360.Web.Services.SimulatedData;

namespace Sucursal360.Tests;

[TestClass]
public sealed class CsvSimulatedDataImportServiceTests
{
    [TestMethod]
    public async Task ValidCsvImportsAllRowsAndCalculatesImportPeriod()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        context.Users.Add(CreateAdminUser());
        await context.SaveChangesAsync();

        var csv = """
business_date,branch_code,net_sales,transaction_count,currency,data_origin
2026-07-01,SUC-001,42500.00,350,NIO,SIMULATED
2026-07-02,SUC-002,31890.50,285,NIO,SIMULATED
""";
        var service = new CsvSimulatedDataImportService(context);

        var result = await service.ImportAsync(csv, "../../operacion.csv", csv.Length, "admin-user", CancellationToken.None);

        Assert.AreEqual("operacion.csv", result.FileName);
        Assert.AreEqual(2, result.RowCount);
        Assert.AreEqual(new DateOnly(2026, 7, 1), result.PeriodStart);
        Assert.AreEqual(new DateOnly(2026, 7, 2), result.PeriodEnd);
        Assert.AreEqual(1, await context.SimulatedDataImports.CountAsync());
        Assert.AreEqual(2, await context.SimulatedOperationalMetrics.CountAsync());

        var centroMetric = await context.SimulatedOperationalMetrics.SingleAsync(metric => metric.BranchId == SeedIds.BranchCentro);
        Assert.AreEqual(42500.00m, centroMetric.NetSales);
        Assert.AreEqual(350, centroMetric.TransactionCount);
        Assert.AreEqual(121.43m, Math.Round(centroMetric.AverageTicket!.Value, 2));
    }

    [TestMethod]
    public async Task InvalidCsvReturnsErrorsAndDoesNotPersist()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        context.Users.Add(CreateAdminUser());
        await context.SaveChangesAsync();

        var csv = """
business_date,branch_code,net_sales,transaction_count,currency,data_origin
bad-date,SUC-404,-1.00,nope,USD,REAL
""";
        var service = new CsvSimulatedDataImportService(context);

        var validation = await service.ValidateAsync(csv, "bad.csv", csv.Length, CancellationToken.None);

        Assert.IsFalse(validation.IsValid);
        Assert.IsTrue(validation.Errors.Any(error => error.ErrorCode == "CSV-422-DATE"));
        Assert.IsTrue(validation.Errors.Any(error => error.ErrorCode == "CSV-422-BRANCH"));
        Assert.IsTrue(validation.Errors.Any(error => error.ErrorCode == "CSV-422-SALES"));
        Assert.IsTrue(validation.Errors.Any(error => error.ErrorCode == "CSV-422-TRANSACTIONS"));
        Assert.IsTrue(validation.Errors.Any(error => error.ErrorCode == "CSV-422-CURRENCY"));
        Assert.IsTrue(validation.Errors.Any(error => error.ErrorCode == "CSV-422-ORIGIN"));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ImportAsync(csv, "bad.csv", csv.Length, "admin-user", CancellationToken.None));
        Assert.AreEqual(0, await context.SimulatedDataImports.CountAsync());
        Assert.AreEqual(0, await context.SimulatedOperationalMetrics.CountAsync());
    }

    [TestMethod]
    public async Task DuplicateBranchDateInFileIsRejected()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        var csv = """
business_date,branch_code,net_sales,transaction_count,currency,data_origin
2026-07-01,SUC-001,42500.00,350,NIO,SIMULATED
2026-07-01,SUC-001,43000.00,360,NIO,SIMULATED
""";
        var service = new CsvSimulatedDataImportService(context);

        var validation = await service.ValidateAsync(csv, "duplicate.csv", csv.Length, CancellationToken.None);

        Assert.IsFalse(validation.IsValid);
        Assert.IsTrue(validation.Errors.Any(error => error.ErrorCode == "CSV-422-DUPLICATE"));
        Assert.AreEqual(0, await context.SimulatedDataImports.CountAsync());
        Assert.AreEqual(0, await context.SimulatedOperationalMetrics.CountAsync());
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
