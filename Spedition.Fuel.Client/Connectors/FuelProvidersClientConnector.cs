using Spedition.Fuel.Client.Helpers;
using Spedition.Fuel.Client.Infrastructure;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.Client.Connectors;

public class FuelProvidersClientConnector : ConnectorBase, IFuelProvidersClientConnector
{
    public FuelProvidersClientConnector(IHttpClientService httpClientService, IFuelApiConfigurationProvider apiVersionProvider)
        : base(httpClientService, apiVersionProvider)
    {
    }

    protected override UriSegment UriSegment => UriSegment.fuelproviders;

    public async Task<List<FuelProviderResponse>> GetProviders(CancellationToken token = default)
    {
        return await httpClientService?.SendRequestAsync<List<FuelProviderResponse>>(Uri, HttpMethod.Get, token);
    }
}
