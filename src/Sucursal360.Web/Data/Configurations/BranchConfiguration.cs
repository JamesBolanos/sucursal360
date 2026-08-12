using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sucursal360.Web.Domain.Entities;

namespace Sucursal360.Web.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.Property(branch => branch.Code)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(branch => branch.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(branch => branch.IsActive)
            .HasDefaultValue(true);

        builder.Property(branch => branch.ExternalPlaceId)
            .HasMaxLength(200);

        builder.HasIndex(branch => branch.Code)
            .IsUnique();

        builder.HasIndex(branch => branch.IsActive);

        builder.HasIndex(branch => new { branch.Provider, branch.ExternalPlaceId })
            .IsUnique()
            .HasFilter("ExternalPlaceId IS NOT NULL")
            .HasDatabaseName("UX_Branches_Provider_ExternalPlaceId");
    }
}
