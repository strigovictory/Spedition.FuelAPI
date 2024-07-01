using Spedition.Fuel.Client.Helpers;

namespace Spedition.Fuel.Client.Connectors;

public abstract class ConnectorBase : RoutesBase
{
    protected readonly IHttpClientService httpClientService;

    public ConnectorBase(IHttpClientService httpClientService, IFuelApiConfigurationProvider apiVersionProvider)
        : base(apiVersionProvider)
    {
        this.httpClientService = httpClientService;
    }
}
