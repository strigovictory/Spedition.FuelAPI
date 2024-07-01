using Spedition.Fuel.Shared.Settings.Configs;

namespace Spedition.Fuel.Shared.Providers
{
    public class FuelApiConfigurationProvider : IFuelApiConfigurationProvider
    {
        private readonly string apiVersion;
        private readonly string apiKey;
        private readonly string apiBaseUrl;

        public FuelApiConfigurationProvider(IOptions<ApiConfigs> clientSettings)
        {
            apiVersion = $"v{clientSettings.Value.Version}";
            apiKey = clientSettings.Value.Key;
            apiBaseUrl = clientSettings.Value.BaseUrl;
        }

        public string GetApiKey()
        {
            return apiKey;
        }

        public string GetApiVersion()
        {
            return apiVersion;
        }

        public string GetApiBaseUrl()
        {
            return apiBaseUrl;
        }
    }
}
