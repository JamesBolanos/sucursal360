using Sucursal360.Web.Domain.Enums;

namespace Sucursal360.Web.Domain.Entities;

public class Review
{
    public Guid Id { get; set; }

    public Guid BranchId { get; set; }

    public PublicDataProvider Provider { get; set; }

    public string ExternalReviewId { get; set; } = string.Empty;

    public byte? Rating { get; set; }

    public string? Text { get; set; }

    public DateTimeOffset? PublishedAtUtc { get; set; }

    public string? AuthorDisplayName { get; set; }

    public string? Language { get; set; }

    public string? SourceUrl { get; set; }

    public DateTimeOffset RetrievedAtUtc { get; set; }

    public Branch Branch { get; set; } = null!;

    public ICollection<ReviewCategoryAssignment> CategoryAssignments { get; } = [];

    public ICollection<ReviewCategoryAudit> CategoryAudits { get; } = [];
}
