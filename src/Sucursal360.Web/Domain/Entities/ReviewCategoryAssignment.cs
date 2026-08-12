using Sucursal360.Web.Data;

namespace Sucursal360.Web.Domain.Entities;

public class ReviewCategoryAssignment
{
    public Guid ReviewId { get; set; }

    public Guid ReviewCategoryId { get; set; }

    public string AssignedByUserId { get; set; } = string.Empty;

    public DateTimeOffset AssignedAtUtc { get; set; }

    public Review Review { get; set; } = null!;

    public ReviewCategory ReviewCategory { get; set; } = null!;

    public ApplicationUser AssignedByUser { get; set; } = null!;
}
