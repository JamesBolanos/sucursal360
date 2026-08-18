namespace Sucursal360.Web.Services.DemoBootstrap;

public sealed class DemoBootstrapOptions
{
    public bool Enabled { get; set; }

    public bool ResetDatabase { get; set; }

    public string? OperationalMetricsCsvPath { get; set; }
}
