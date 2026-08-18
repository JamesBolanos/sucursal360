namespace Sucursal360.Web.Services.DemoBootstrap;

public interface IDemoBootstrapService
{
    Task BootstrapAsync(CancellationToken cancellationToken);
}
