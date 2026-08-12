namespace Sucursal360.Web.ViewModels.AdminIntegrations;

public sealed record IntegrationResultItemViewModel(
    string BranchCode,
    string BranchName,
    string Status,
    string CorrelationId,
    int RecordsReceived,
    int RecordsStored,
    string UserMessage,
    string? ErrorCode);
