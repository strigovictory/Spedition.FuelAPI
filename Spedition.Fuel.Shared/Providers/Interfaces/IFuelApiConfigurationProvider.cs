using Microsoft.Extensions.Configuration;

namespace Spedition.Fuel.Shared.Providers.Interfaces
{
    public interface IFuelApiConfigurationProvider
    {
        string GetApiVersion();

        string GetApiKey();

        string GetApiBaseUrl();
    }
}
