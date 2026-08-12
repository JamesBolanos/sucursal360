using Sucursal360.Web.Domain.Enums;

namespace Sucursal360.Web.Integrations.Abstractions;

public interface IPublicBranchDataProvider
{
    PublicDataProvider Provider { get; }

    Task<ExternalBranchData> GetBranchAsync(string externalPlaceId, CancellationToken cancellationToken);
}
