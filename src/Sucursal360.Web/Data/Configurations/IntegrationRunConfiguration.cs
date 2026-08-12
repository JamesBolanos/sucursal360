using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sucursal360.Web.Domain.Entities;

namespace Sucursal360.Web.Data.Configurations;

public class IntegrationRunConfiguration : IEntityTypeConfiguration<IntegrationRun>
{
    public void Configure(EntityTypeBuilder<IntegrationRun> builder)
    {
        builder.Property(run => run.CorrelationId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(run => run.ErrorCode)
            .HasMaxLength(50);

        builder.Property(run => run.UserMessage)
            .HasMaxLength(500);

        builder.Property(run => run.TechnicalMessage)
            .HasMaxLength(2000);

        builder.Property(run => run.TriggeredByUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(run => run.RecordsReceived)
            .HasDefaultValue(0);

        builder.Property(run => run.RecordsStored)
            .HasDefaultValue(0);

        builder.HasIndex(run => run.CorrelationId)
            .IsUnique();

        builder.HasIndex(run => new { run.BranchId, run.StartedAtUtc })
            .HasDatabaseName("IX_IntegrationRuns_Branch_Date");

        builder.HasIndex(run => new { run.Status, run.StartedAtUtc })
            .HasDatabaseName("IX_IntegrationRuns_Status_Date");

        builder.HasOne(run => run.Branch)
            .WithMany(branch => branch.IntegrationRuns)
            .HasForeignKey(run => run.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(run => run.TriggeredByUser)
            .WithMany()
            .HasForeignKey(run => run.TriggeredByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
            table.HasCheckConstraint("CK_IntegrationRuns_Counts", "RecordsReceived >= 0 AND RecordsStored >= 0"));
    }
}
