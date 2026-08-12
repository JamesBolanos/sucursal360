using Sucursal360.Web.Domain.Enums;

namespace Sucursal360.Web.Services.Synchronization;

public sealed record SynchronizationResult(
    Guid? BranchId,
    string BranchCode,
    string BranchName,
    Guid IntegrationRunId,
    string CorrelationId,
    IntegrationRunStatus Status,
    int RecordsReceived,
    int RecordsStored,
    string UserMessage,
    string? ErrorCode);
