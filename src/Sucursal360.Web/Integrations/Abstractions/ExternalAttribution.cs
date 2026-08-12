namespace Sucursal360.Web.Integrations.Abstractions;

public sealed record ExternalAttribution(
    string ProviderName,
    string? DisplayText,
    string? Uri);
