using System.Text.Json;
using Microsoft.Extensions.Options;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.Integrations.Abstractions;

namespace Sucursal360.Web.Integrations.Demo;

public sealed class DemoPublicBranchDataProvider(
    IWebHostEnvironment environment,
    IOptions<DemoPublicDataOptions> options) : IPublicBranchDataProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PublicDataProvider Provider => PublicDataProvider.Demo;

    public async Task<ExternalBranchData> GetBranchAsync(string externalPlaceId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(externalPlaceId))
        {
            throw CreateProviderException("INT-400-CONFIG", "Revise la configuracion de la sucursal.", "External place id is empty.");
        }

        var fixturePath = Path.Combine(environment.ContentRootPath, options.Value.FixturesPath, $"{externalPlaceId}.json");
        if (!File.Exists(fixturePath))
        {
            throw CreateProviderException("INT-404-PLACE", "No se encontro el establecimiento configurado.", $"Fixture not found: {fixturePath}");
        }

        DemoBranchFixture? fixture;
        try
        {
            await using var stream = File.OpenRead(fixturePath);
            fixture = await JsonSerializer.DeserializeAsync<DemoBranchFixture>(stream, JsonOptions, cancellationToken);
        }
        catch (JsonException exception)
        {
            throw CreateProviderException("INT-422-PAYLOAD", "La respuesta no contiene datos utilizables.", "Fixture JSON is invalid.", exception);
        }

        if (fixture is null)
        {
            throw CreateProviderException("INT-422-PAYLOAD", "La respuesta no contiene datos utilizables.", "Fixture JSON deserialized to null.");
        }

        if (fixture.SchemaVersion != "1.0")
        {
            throw CreateProviderException("INT-422-SCHEMA", "El formato recibido no es compatible.", $"Unsupported schema version: {fixture.SchemaVersion}");
        }

        if (!string.Equals(fixture.Provider, "DEMO", StringComparison.OrdinalIgnoreCase))
        {
            throw CreateProviderException("INT-422-PAYLOAD", "La respuesta no contiene datos utilizables.", $"Unexpected provider: {fixture.Provider}");
        }

        if (!string.Equals(fixture.ExternalPlaceId, externalPlaceId, StringComparison.Ordinal))
        {
            throw CreateProviderException("INT-422-PAYLOAD", "La respuesta no contiene datos utilizables.", "Fixture externalPlaceId does not match requested id.");
        }

        var reviews = fixture.Reviews
            .Where(review => !string.IsNullOrWhiteSpace(review.ExternalReviewId))
            .GroupBy(review => review.ExternalReviewId, StringComparer.Ordinal)
            .Select(group => group.First())
            .Select(review => new ExternalReviewData(
                review.ExternalReviewId,
                NormalizeReviewRating(review.Rating),
                review.Text,
                review.PublishedAtUtc,
                review.AuthorDisplayName,
                review.Language,
                review.SourceUrl,
                []))
            .ToList();

        return new ExternalBranchData(
            PublicDataProvider.Demo,
            fixture.ExternalPlaceId,
            fixture.DisplayName,
            fixture.Address,
            NormalizeCoordinate(fixture.Latitude, -90, 90),
            NormalizeCoordinate(fixture.Longitude, -180, 180),
            ParseBusinessStatus(fixture.BusinessStatus),
            fixture.OpeningHoursText,
            NormalizeRating(fixture.Rating),
            fixture.ReviewCount is >= 0 ? fixture.ReviewCount : null,
            fixture.RetrievedAtUtc > DateTimeOffset.UtcNow.AddMinutes(5)
                ? DateTimeOffset.UtcNow
                : fixture.RetrievedAtUtc,
            reviews,
            fixture.SourceUrl,
            []);
    }

    private static decimal? NormalizeRating(decimal? rating)
    {
        return rating is >= 1m and <= 5m ? rating : null;
    }

    private static byte? NormalizeReviewRating(byte? rating)
    {
        return rating is >= 1 and <= 5 ? rating : null;
    }

    private static decimal? NormalizeCoordinate(decimal? coordinate, decimal min, decimal max)
    {
        return coordinate is not null && coordinate >= min && coordinate <= max ? coordinate : null;
    }

    private static BusinessStatus? ParseBusinessStatus(string? businessStatus)
    {
        return businessStatus?.ToUpperInvariant() switch
        {
            "OPERATIONAL" => BusinessStatus.Operational,
            "TEMPORARILY_CLOSED" => BusinessStatus.TemporarilyClosed,
            "PERMANENTLY_CLOSED" => BusinessStatus.PermanentlyClosed,
            "UNKNOWN" => BusinessStatus.Unknown,
            _ => null
        };
    }

    private static PublicDataProviderException CreateProviderException(
        string errorCode,
        string userMessage,
        string technicalMessage,
        Exception? innerException = null)
    {
        return new PublicDataProviderException(errorCode, userMessage, technicalMessage, innerException);
    }

    private sealed class DemoBranchFixture
    {
        public string SchemaVersion { get; set; } = string.Empty;

        public string Provider { get; set; } = string.Empty;

        public string ExternalPlaceId { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        public string? Address { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? BusinessStatus { get; set; }

        public List<string> OpeningHoursText { get; set; } = [];

        public decimal? Rating { get; set; }

        public int? ReviewCount { get; set; }

        public DateTimeOffset RetrievedAtUtc { get; set; }

        public List<DemoReviewFixture> Reviews { get; set; } = [];

        public string? SourceUrl { get; set; }
    }

    private sealed class DemoReviewFixture
    {
        public string ExternalReviewId { get; set; } = string.Empty;

        public byte? Rating { get; set; }

        public string? Text { get; set; }

        public DateTimeOffset? PublishedAtUtc { get; set; }

        public string? AuthorDisplayName { get; set; }

        public string? Language { get; set; }

        public string? SourceUrl { get; set; }
    }
}
