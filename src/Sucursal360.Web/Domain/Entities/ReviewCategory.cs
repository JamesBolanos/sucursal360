namespace Sucursal360.Web.Domain.Entities;

public class ReviewCategory
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<ReviewCategoryAssignment> Assignments { get; } = [];

    public ICollection<ReviewCategoryAudit> Audits { get; } = [];
}
