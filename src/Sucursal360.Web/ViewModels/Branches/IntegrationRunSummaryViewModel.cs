namespace Sucursal360.Web.ViewModels.Branches;

public sealed record IntegrationRunSummaryViewModel(
    string CorrelationId,
    string Provider,
    string Status,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? FinishedAtUtc,
    int RecordsReceived,
    int RecordsStored,
    string? ErrorCode,
    string UserMessage);
