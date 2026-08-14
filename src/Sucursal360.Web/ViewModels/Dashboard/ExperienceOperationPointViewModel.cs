namespace Sucursal360.Web.ViewModels.Dashboard;

public sealed record ExperienceOperationPointViewModel(
    Guid BranchId,
    string Code,
    string Name,
    decimal? Rating,
    decimal? NetSales,
    int? ReviewCount,
    string AttentionLevel,
    int XPercent,
    int YPercent,
    int Size);
