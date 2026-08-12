namespace Sucursal360.Web.ViewModels.AdminIntegrations;

public sealed record IntegrationRunItemViewModel(
    Guid Id,
    string CorrelationId,
    string BranchCode,
    string BranchName,
    string Provider,
    string Status,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? FinishedAtUtc,
    int RecordsReceived,
    int RecordsStored,
    string? ErrorCode,
    string? UserMessage);
