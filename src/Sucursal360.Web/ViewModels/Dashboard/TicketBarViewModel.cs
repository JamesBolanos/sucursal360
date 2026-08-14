namespace Sucursal360.Web.ViewModels.Dashboard;

public sealed record TicketBarViewModel(
    string Label,
    decimal? AverageTicket,
    int Percent,
    string Color);
