namespace Sucursal360.Web.Integrations.Abstractions;

public sealed record ExternalReviewData(
    string ExternalReviewId,
    byte? Rating,
    string? Text,
    DateTimeOffset? PublishedAtUtc,
    string? AuthorDisplayName,
    string? Language,
    string? SourceUrl,
    IReadOnlyList<ExternalAttribution> Attributions);
