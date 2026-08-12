using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sucursal360.Web.Domain.Entities;

namespace Sucursal360.Web.Data.Configurations;

public class SimulatedOperationalMetricConfiguration : IEntityTypeConfiguration<SimulatedOperationalMetric>
{
    public void Configure(EntityTypeBuilder<SimulatedOperationalMetric> builder)
    {
        builder.Property(metric => metric.NetSales)
            .HasPrecision(18, 2);

        builder.Property(metric => metric.Currency)
            .HasMaxLength(3)
            .IsFixedLength()
            .HasDefaultValue("NIO")
            .IsRequired();

        builder.HasIndex(metric => new { metric.BranchId, metric.BusinessDate })
            .IsUnique()
            .HasDatabaseName("UX_SimulatedMetrics_Branch_Date");

        builder.HasIndex(metric => metric.ImportId)
            .HasDatabaseName("IX_SimulatedMetrics_Import");

        builder.Ignore(metric => metric.AverageTicket);

        builder.HasOne(metric => metric.Branch)
            .WithMany(branch => branch.SimulatedOperationalMetrics)
            .HasForeignKey(metric => metric.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(metric => metric.Import)
            .WithMany(import => import.Metrics)
            .HasForeignKey(metric => metric.ImportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_SimulatedMetrics_NetSales", "NetSales >= 0");
            table.HasCheckConstraint("CK_SimulatedMetrics_Transactions", "TransactionCount >= 0");
            table.HasCheckConstraint("CK_SimulatedMetrics_Origin", "DataOrigin = 1");
        });
    }
}
