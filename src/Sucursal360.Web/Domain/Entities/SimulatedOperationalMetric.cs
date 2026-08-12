using Sucursal360.Web.Domain.Enums;

namespace Sucursal360.Web.Domain.Entities;

public class SimulatedOperationalMetric
{
    public Guid Id { get; set; }

    public Guid BranchId { get; set; }

    public DateOnly BusinessDate { get; set; }

    public decimal NetSales { get; set; }

    public int TransactionCount { get; set; }

    public string Currency { get; set; } = "NIO";

    public DataOrigin DataOrigin { get; set; } = DataOrigin.Simulated;

    public Guid ImportId { get; set; }

    public decimal? AverageTicket => TransactionCount == 0 ? null : NetSales / TransactionCount;

    public Branch Branch { get; set; } = null!;

    public SimulatedDataImport Import { get; set; } = null!;
}
