using Sucursal360.Web.Domain.Enums;

namespace Sucursal360.Web.Integrations.Abstractions;

public sealed record ExternalBranchData(
    PublicDataProvider Provider,
    string ExternalPlaceId,
    string? DisplayName,
    string? Address,
    decimal? Latitude,
    decimal? Longitude,
    BusinessStatus? BusinessStatus,
    IReadOnlyList<string> OpeningHoursText,
    decimal? Rating,
    int? ReviewCount,
    DateTimeOffset RetrievedAtUtc,
    IReadOnlyList<ExternalReviewData> Reviews,
    string? SourceUrl,
    IReadOnlyList<ExternalAttribution> Attributions);
