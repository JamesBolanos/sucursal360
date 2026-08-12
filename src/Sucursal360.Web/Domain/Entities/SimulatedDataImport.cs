using Sucursal360.Web.Data;

namespace Sucursal360.Web.Domain.Entities;

public class SimulatedDataImport
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public int RowCount { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public string ImportedByUserId { get; set; } = string.Empty;

    public DateTimeOffset ImportedAtUtc { get; set; }

    public ApplicationUser ImportedByUser { get; set; } = null!;

    public ICollection<SimulatedOperationalMetric> Metrics { get; } = [];
}
