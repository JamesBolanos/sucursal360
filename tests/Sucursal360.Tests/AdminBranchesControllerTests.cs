using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Controllers;
using Sucursal360.Web.Data;
using Sucursal360.Web.Data.Seed;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.ViewModels.AdminBranches;

namespace Sucursal360.Tests;

[TestClass]
public sealed class AdminBranchesControllerTests
{
    [TestMethod]
    public async Task CreatePersistsNormalizedBranch()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        var controller = new AdminBranchesController(context);
        var model = new BranchFormViewModel
        {
            Code = "suc-099",
            Name = " Cafe Horizonte Prueba ",
            Provider = PublicDataProvider.Demo,
            ExternalPlaceId = " DEMO-SUC-099 ",
            IsActive = true
        };

        var result = await controller.Create(model, CancellationToken.None);

        var redirect = result as RedirectToActionResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual(nameof(AdminBranchesController.Index), redirect.ActionName);

        var branch = await context.Branches.SingleAsync(branch => branch.Code == "SUC-099");
        Assert.AreEqual("Cafe Horizonte Prueba", branch.Name);
        Assert.AreEqual("DEMO-SUC-099", branch.ExternalPlaceId);
        Assert.IsTrue(branch.IsActive);
    }

    [TestMethod]
    public async Task CreateRejectsDuplicateCode()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        var controller = new AdminBranchesController(context);
        var model = new BranchFormViewModel
        {
            Code = "SUC-001",
            Name = "Cafe Horizonte Duplicado",
            Provider = PublicDataProvider.Demo,
            ExternalPlaceId = "DEMO-SUC-099",
            IsActive = true
        };

        var result = await controller.Create(model, CancellationToken.None);

        Assert.IsInstanceOfType<ViewResult>(result);
        Assert.AreNotEqual(0, controller.ModelState[nameof(model.Code)]?.Errors.Count);
        Assert.AreEqual(5, await context.Branches.CountAsync());
    }

    [TestMethod]
    public async Task CreateRejectsDuplicateProviderExternalId()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        var controller = new AdminBranchesController(context);
        var model = new BranchFormViewModel
        {
            Code = "SUC-099",
            Name = "Cafe Horizonte Duplicado",
            Provider = PublicDataProvider.Demo,
            ExternalPlaceId = "DEMO-SUC-001",
            IsActive = true
        };

        var result = await controller.Create(model, CancellationToken.None);

        Assert.IsInstanceOfType<ViewResult>(result);
        Assert.AreNotEqual(0, controller.ModelState[nameof(model.ExternalPlaceId)]?.Errors.Count);
        Assert.AreEqual(5, await context.Branches.CountAsync());
    }

    [TestMethod]
    public async Task DeactivateMarksBranchInactiveWithoutDeletingIt()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        var controller = new AdminBranchesController(context);

        var result = await controller.Deactivate(SeedIds.BranchCentro, CancellationToken.None);

        var redirect = result as RedirectToActionResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual(nameof(AdminBranchesController.Index), redirect.ActionName);

        var branch = await context.Branches.SingleAsync(branch => branch.Id == SeedIds.BranchCentro);
        Assert.IsFalse(branch.IsActive);
        Assert.AreEqual(5, await context.Branches.CountAsync());
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        return new ApplicationDbContext(options);
    }
}
