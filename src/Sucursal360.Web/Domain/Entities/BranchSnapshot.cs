using Sucursal360.Web.Domain.Enums;

namespace Sucursal360.Web.Domain.Entities;

public class BranchSnapshot
{
    public Guid Id { get; set; }

    public Guid BranchId { get; set; }

    public PublicDataProvider Provider { get; set; }

    public string? DisplayName { get; set; }

    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public BusinessStatus? BusinessStatus { get; set; }

    public string? OpeningHoursJson { get; set; }

    public decimal? Rating { get; set; }

    public int? ReviewCount { get; set; }

    public DateTimeOffset RetrievedAtUtc { get; set; }

    public Guid IntegrationRunId { get; set; }

    public Branch Branch { get; set; } = null!;

    public IntegrationRun IntegrationRun { get; set; } = null!;
}
