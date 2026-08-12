using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sucursal360.Web.Data.Seed;
using Sucursal360.Web.Domain.Entities;

namespace Sucursal360.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<BranchSnapshot> BranchSnapshots => Set<BranchSnapshot>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<ReviewCategory> ReviewCategories => Set<ReviewCategory>();

    public DbSet<ReviewCategoryAssignment> ReviewCategoryAssignments => Set<ReviewCategoryAssignment>();

    public DbSet<ReviewCategoryAudit> ReviewCategoryAudits => Set<ReviewCategoryAudit>();

    public DbSet<SimulatedOperationalMetric> SimulatedOperationalMetrics => Set<SimulatedOperationalMetric>();

    public DbSet<SimulatedDataImport> SimulatedDataImports => Set<SimulatedDataImport>();

    public DbSet<IntegrationRun> IntegrationRuns => Set<IntegrationRun>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ModelSeedData.Apply(builder);
    }
}
