using System.Security.Claims;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Data.Seed;
using Sucursal360.Web.Security;

namespace Sucursal360.Tests;

[TestClass]
public sealed class BranchAccessServiceTests
{
    [TestMethod]
    public async Task AdministratorCanAccessAnyBranch()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        var service = new BranchAccessService(context);
        var user = CreatePrincipal("admin-user", AppRoles.Administrator);

        Assert.IsTrue(await service.CanAccessAsync(user, SeedIds.BranchCentro, CancellationToken.None));
        Assert.IsTrue(await service.CanAccessAsync(user, SeedIds.BranchLasColinas, CancellationToken.None));
    }

    [TestMethod]
    public async Task CorporateManagerCanAccessAnyBranch()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        var service = new BranchAccessService(context);
        var user = CreatePrincipal("corporate-user", AppRoles.CorporateManager);

        Assert.IsTrue(await service.CanAccessAsync(user, SeedIds.BranchCentro, CancellationToken.None));
        Assert.IsTrue(await service.CanAccessAsync(user, SeedIds.BranchGalerias, CancellationToken.None));
    }

    [TestMethod]
    public async Task BranchManagerCanAccessOnlyAssignedBranch()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        var branchManager = new ApplicationUser
        {
            Id = "branch-user",
            UserName = "sucursal@sucursal360.local",
            Email = "sucursal@sucursal360.local",
            IsActive = true,
            EmailConfirmed = true,
            AssignedBranchId = SeedIds.BranchCentro,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        context.Users.Add(branchManager);
        await context.SaveChangesAsync();

        var service = new BranchAccessService(context);
        var user = CreatePrincipal(branchManager.Id, AppRoles.BranchManager);

        Assert.IsTrue(await service.CanAccessAsync(user, SeedIds.BranchCentro, CancellationToken.None));
        Assert.IsFalse(await service.CanAccessAsync(user, SeedIds.BranchGalerias, CancellationToken.None));
    }

    [TestMethod]
    public async Task AnonymousUserCannotAccessBranch()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        var service = new BranchAccessService(context);
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.IsFalse(await service.CanAccessAsync(anonymous, SeedIds.BranchCentro, CancellationToken.None));
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ClaimsPrincipal CreatePrincipal(string userId, string role)
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            },
            authenticationType: "Test");

        return new ClaimsPrincipal(identity);
    }
}
