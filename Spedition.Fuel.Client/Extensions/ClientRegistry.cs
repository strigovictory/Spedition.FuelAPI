using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.Providers;
using Spedition.Fuel.Shared.Settings.Configs;

namespace Spedition.Fuel.Client.Extensions;

public static class ClientRegistry
{
    private const string ApiKeyHeader = "x-api-key";
    private const int ClientTimeout = 20;
    private const int RetryCount = 3;
    private const int BreakDuration = 30;
    private const int HandledEventsAllowedBeforeBreaking = 3;

    public static IServiceCollection AddFuelApi(this IServiceCollection services, ApiConfigs apiConfigs)
    {
        services.AddHttpClient("fuel-api").ConfigureHttpClient(x =>
        {
            x.Timeout = TimeSpan.FromMinutes(ClientTimeout);
            x.BaseAddress = new Uri(apiConfigs.BaseUrl);
            x.DefaultRequestHeaders.Add(ApiKeyHeader, apiConfigs.Key);
        });

        services.AddTransient<IHttpClientService, HttpClientService>();

        services.AddTransient<IFuelApiConfigurationProvider, FuelApiConfigurationProvider>();
        services.AddTransient<IFuelCardsClientConnector, FuelCardsClientConnector>();
        services.AddTransient<IFuelCardsEventsClientConnector, FuelCardsEventsClientConnector>();
        services.AddTransient<IFuelProvidersClientConnector, FuelProvidersClientConnector>();
        services.AddTransient<IFuelTransactionsClientConnector, FuelTransactionsClientConnector>();
        services.AddTransient<IFuelTypesClientConnector, FuelTypesClientConnector>();
        services.AddTransient<IFuelParserClientConnector, FuelParserClientConnector>();
        services.AddTransient<IJobsClientConnector, JobsClientConnector>();
        services.AddTransient<IFuelPrintClientConnector, FuelPrintClientConnector>();

        return services;
    }

    private static string GetConfigurationFilePath()
    {
        var configurationFilePath = string.Empty;
        var assemblyDir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(FuelCardRequest)).Location);
#if DEBUG
        configurationFilePath = Path.Combine(assemblyDir, "appsettings.Development.json");
#else
        configurationFilePath = Path.Combine(assemblyDir, "appsettings.json");
#endif
        return configurationFilePath;
    }
}
