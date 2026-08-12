using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sucursal360.Web.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(user => user.AssignedBranch)
            .WithMany()
            .HasForeignKey(user => user.AssignedBranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
