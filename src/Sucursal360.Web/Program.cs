using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Integrations.Abstractions;
using Sucursal360.Web.Integrations.Demo;
using Sucursal360.Web.Security;
using Sucursal360.Web.Services.DemoBootstrap;
using Sucursal360.Web.Services.Reports;
using Sucursal360.Web.Services.Reviews;
using Sucursal360.Web.Services.SimulatedData;
using Sucursal360.Web.Services.Synchronization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
EnsureSqliteDirectory(connectionString);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 10;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPolicies.CanViewCorporateDashboard, policy =>
        policy.RequireRole(AppRoles.CorporateManager, AppRoles.Administrator));
    options.AddPolicy(AppPolicies.CanExportManagementReport, policy =>
        policy.RequireRole(AppRoles.CorporateManager, AppRoles.Administrator));
    options.AddPolicy(AppPolicies.CanAdministerSystem, policy =>
        policy.RequireRole(AppRoles.Administrator));
});
builder.Services.AddScoped<IBranchAccessService, BranchAccessService>();
builder.Services.Configure<DemoPublicDataOptions>(builder.Configuration.GetSection("PublicData:Demo"));
builder.Services.AddScoped<IPublicBranchDataProvider, DemoPublicBranchDataProvider>();
builder.Services.AddScoped<IManagementReportExporter, ClosedXmlManagementReportExporter>();
builder.Services.AddScoped<IReviewCategorizationService, ReviewCategorizationService>();
builder.Services.AddScoped<ISimulatedDataImportService, CsvSimulatedDataImportService>();
builder.Services.AddScoped<IBranchSynchronizationService, BranchSynchronizationService>();
builder.Services.Configure<DemoBootstrapOptions>(builder.Configuration.GetSection("DemoBootstrap"));
builder.Services.AddScoped<IDemoBootstrapService, DemoBootstrapService>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddRazorPages();

var app = builder.Build();

var demoBootstrapEnabled = app.Configuration.GetValue<bool>("DemoBootstrap:Enabled");
var demoBootstrapResetDatabase = demoBootstrapEnabled && app.Configuration.GetValue<bool>("DemoBootstrap:ResetDatabase");
if (demoBootstrapResetDatabase)
{
    await ResetDatabaseAsync(app.Services);
}
else if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await ApplyDatabaseMigrationsAsync(app.Services);
}

await DevelopmentUserSeeder.SeedAsync(app.Services);
await BootstrapDemoAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();

static void EnsureSqliteDirectory(string connectionString)
{
    var builder = new SqliteConnectionStringBuilder(connectionString);
    var dataSource = builder.DataSource;
    if (string.IsNullOrWhiteSpace(dataSource) || dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
    if (!string.IsNullOrWhiteSpace(directory))
    {
        Directory.CreateDirectory(directory);
    }
}

static async Task ApplyDatabaseMigrationsAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

static async Task ResetDatabaseAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureDeletedAsync();
    await dbContext.Database.MigrateAsync();
}

static async Task BootstrapDemoAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var demoBootstrapService = scope.ServiceProvider.GetRequiredService<IDemoBootstrapService>();
    await demoBootstrapService.BootstrapAsync(CancellationToken.None);
}
