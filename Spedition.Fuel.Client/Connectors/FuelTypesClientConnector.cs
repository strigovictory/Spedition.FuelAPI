using Spedition.Fuel.Client.Helpers;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.Client.Connectors;

public class FuelTypesClientConnector : ConnectorBase, IFuelTypesClientConnector
{
    public FuelTypesClientConnector(IHttpClientService httpClientService, IFuelApiConfigurationProvider apiVersionProvider)
        : base(httpClientService, apiVersionProvider)
    {
    }

    protected override UriSegment UriSegment => UriSegment.fueltypes;

    public async Task<List<FuelTypeResponse>> GetFuelTypes(CancellationToken token = default)
    {
        return await httpClientService?.SendRequestAsync<List<FuelTypeResponse>>(Uri, HttpMethod.Get, token);
    }
}
