using Sucursal360.Web.Data;
using Sucursal360.Web.Domain.Enums;

namespace Sucursal360.Web.Domain.Entities;

public class ReviewCategoryAudit
{
    public Guid Id { get; set; }

    public Guid ReviewId { get; set; }

    public Guid ReviewCategoryId { get; set; }

    public CategoryAuditAction Action { get; set; }

    public string ChangedByUserId { get; set; } = string.Empty;

    public DateTimeOffset ChangedAtUtc { get; set; }

    public Review Review { get; set; } = null!;

    public ReviewCategory ReviewCategory { get; set; } = null!;

    public ApplicationUser ChangedByUser { get; set; } = null!;
}
