using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sucursal360.Web.Domain.Entities;

namespace Sucursal360.Web.Data.Configurations;

public class BranchSnapshotConfiguration : IEntityTypeConfiguration<BranchSnapshot>
{
    public void Configure(EntityTypeBuilder<BranchSnapshot> builder)
    {
        builder.Property(snapshot => snapshot.DisplayName)
            .HasMaxLength(160);

        builder.Property(snapshot => snapshot.Address)
            .HasMaxLength(300);

        builder.Property(snapshot => snapshot.Latitude)
            .HasPrecision(9, 6);

        builder.Property(snapshot => snapshot.Longitude)
            .HasPrecision(9, 6);

        builder.Property(snapshot => snapshot.Rating)
            .HasPrecision(2, 1);

        builder.HasIndex(snapshot => new { snapshot.BranchId, snapshot.Provider, snapshot.RetrievedAtUtc })
            .IsUnique()
            .HasDatabaseName("UX_BranchSnapshots_Branch_Provider_Date");

        builder.HasIndex(snapshot => new { snapshot.BranchId, snapshot.RetrievedAtUtc })
            .HasDatabaseName("IX_BranchSnapshots_Branch_Date");

        builder.HasOne(snapshot => snapshot.Branch)
            .WithMany(branch => branch.Snapshots)
            .HasForeignKey(snapshot => snapshot.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(snapshot => snapshot.IntegrationRun)
            .WithMany(run => run.Snapshots)
            .HasForeignKey(snapshot => snapshot.IntegrationRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_BranchSnapshots_Rating", "Rating IS NULL OR Rating BETWEEN 1.0 AND 5.0");
            table.HasCheckConstraint("CK_BranchSnapshots_ReviewCount", "ReviewCount IS NULL OR ReviewCount >= 0");
            table.HasCheckConstraint("CK_BranchSnapshots_Latitude", "Latitude IS NULL OR Latitude BETWEEN -90 AND 90");
            table.HasCheckConstraint("CK_BranchSnapshots_Longitude", "Longitude IS NULL OR Longitude BETWEEN -180 AND 180");
        });
    }
}
