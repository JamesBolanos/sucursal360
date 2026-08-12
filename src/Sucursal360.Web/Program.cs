using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data;
using Sucursal360.Web.Integrations.Abstractions;
using Sucursal360.Web.Integrations.Demo;
using Sucursal360.Web.Security;
using Sucursal360.Web.Services.Synchronization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
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
builder.Services.AddScoped<IBranchSynchronizationService, BranchSynchronizationService>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddRazorPages();

var app = builder.Build();

await DevelopmentUserSeeder.SeedAsync(app.Services);

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
