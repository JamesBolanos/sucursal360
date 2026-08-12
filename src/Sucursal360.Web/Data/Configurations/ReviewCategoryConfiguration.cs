using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sucursal360.Web.Domain.Entities;

namespace Sucursal360.Web.Data.Configurations;

public class ReviewCategoryConfiguration : IEntityTypeConfiguration<ReviewCategory>
{
    public void Configure(EntityTypeBuilder<ReviewCategory> builder)
    {
        builder.Property(category => category.Code)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(category => category.Name)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(category => category.Description)
            .HasMaxLength(300)
            .IsRequired();

        builder.HasIndex(category => category.Code)
            .IsUnique();
    }
}
