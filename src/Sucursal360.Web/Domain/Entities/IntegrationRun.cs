using Sucursal360.Web.Data;
using Sucursal360.Web.Domain.Enums;

namespace Sucursal360.Web.Domain.Entities;

public class IntegrationRun
{
    public Guid Id { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public PublicDataProvider Provider { get; set; }

    public Guid BranchId { get; set; }

    public DateTimeOffset StartedAtUtc { get; set; }

    public DateTimeOffset? FinishedAtUtc { get; set; }

    public IntegrationRunStatus Status { get; set; }

    public int? HttpStatusCode { get; set; }

    public int RecordsReceived { get; set; }

    public int RecordsStored { get; set; }

    public string? ErrorCode { get; set; }

    public string? UserMessage { get; set; }

    public string? TechnicalMessage { get; set; }

    public string TriggeredByUserId { get; set; } = string.Empty;

    public Branch Branch { get; set; } = null!;

    public ApplicationUser TriggeredByUser { get; set; } = null!;

    public ICollection<BranchSnapshot> Snapshots { get; } = [];
}
