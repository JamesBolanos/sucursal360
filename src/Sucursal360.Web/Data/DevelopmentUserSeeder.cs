using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data.Seed;
using Sucursal360.Web.Security;

namespace Sucursal360.Web.Data;

public static class DevelopmentUserSeeder
{
    private const string DefaultPasswordKey = "SeedUsers:DefaultPassword";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        if (!environment.IsDevelopment())
        {
            return;
        }

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DevelopmentUserSeeder");
        var defaultPassword = configuration[DefaultPasswordKey];

        if (string.IsNullOrWhiteSpace(defaultPassword))
        {
            logger.LogInformation("Development users were not seeded because {Key} is not configured.", DefaultPasswordKey);
            return;
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!await dbContext.Branches.AnyAsync())
        {
            logger.LogWarning("Development users were not seeded because branch seed data is missing.");
            return;
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await EnsureRoleAsync(roleManager, AppRoles.Administrator);
        await EnsureRoleAsync(roleManager, AppRoles.CorporateManager);
        await EnsureRoleAsync(roleManager, AppRoles.BranchManager);

        await EnsureUserAsync(
            userManager,
            email: configuration["SeedUsers:Administrator:Email"] ?? "admin@sucursal360.local",
            displayName: "Administrador Demo",
            role: AppRoles.Administrator,
            assignedBranchId: null,
            defaultPassword,
            logger);

        await EnsureUserAsync(
            userManager,
            email: configuration["SeedUsers:CorporateManager:Email"] ?? "corporativo@sucursal360.local",
            displayName: "Gerente Corporativo Demo",
            role: AppRoles.CorporateManager,
            assignedBranchId: null,
            defaultPassword,
            logger);

        await EnsureUserAsync(
            userManager,
            email: configuration["SeedUsers:BranchManager:Email"] ?? "sucursal@sucursal360.local",
            displayName: "Gerente de Sucursal Demo",
            role: AppRoles.BranchManager,
            assignedBranchId: SeedIds.BranchCentro,
            defaultPassword,
            logger);
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string displayName,
        string role,
        Guid? assignedBranchId,
        string defaultPassword,
        ILogger logger)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                IsActive = true,
                AssignedBranchId = assignedBranchId,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            var createResult = await userManager.CreateAsync(user, defaultPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(error => error.Code));
                logger.LogWarning("Could not create development user {Email}: {Errors}", email, errors);
                return;
            }

            logger.LogInformation("Created development user {Email} ({DisplayName}).", email, displayName);
        }
        else
        {
            user.IsActive = true;
            user.EmailConfirmed = true;
            user.AssignedBranchId = assignedBranchId;
            await userManager.UpdateAsync(user);
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
