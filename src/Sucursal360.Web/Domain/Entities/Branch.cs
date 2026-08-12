using Sucursal360.Web.Domain.Enums;

namespace Sucursal360.Web.Domain.Entities;

public class Branch
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public PublicDataProvider Provider { get; set; }

    public string? ExternalPlaceId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ICollection<BranchSnapshot> Snapshots { get; } = [];

    public ICollection<Review> Reviews { get; } = [];

    public ICollection<SimulatedOperationalMetric> SimulatedOperationalMetrics { get; } = [];

    public ICollection<IntegrationRun> IntegrationRuns { get; } = [];
}
