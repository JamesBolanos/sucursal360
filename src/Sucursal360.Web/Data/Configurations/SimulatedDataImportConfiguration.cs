using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sucursal360.Web.Domain.Entities;

namespace Sucursal360.Web.Data.Configurations;

public class SimulatedDataImportConfiguration : IEntityTypeConfiguration<SimulatedDataImport>
{
    public void Configure(EntityTypeBuilder<SimulatedDataImport> builder)
    {
        builder.Property(import => import.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(import => import.ImportedByUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasOne(import => import.ImportedByUser)
            .WithMany()
            .HasForeignKey(import => import.ImportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
            table.HasCheckConstraint("CK_Imports_Period", "PeriodEnd >= PeriodStart"));
    }
}
