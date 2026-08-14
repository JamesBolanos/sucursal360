namespace Sucursal360.Web.ViewModels.Dashboard;

public sealed record ExecutiveSummaryViewModel(
    string ScopeLabel,
    string Headline,
    string Detail,
    string RiskLabel,
    string RiskTone,
    int HealthPercent);
