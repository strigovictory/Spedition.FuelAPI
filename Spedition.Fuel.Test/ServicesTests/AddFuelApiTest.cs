using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Spedition.Fuel.Client.Connectors.Interfaces;
using Spedition.Fuel.Client.Extensions;
using Spedition.Fuel.Shared.DTO.ResponseModels;
using Spedition.Fuel.Shared.Providers.Interfaces;
using Spedition.Fuel.Shared.Settings.Configs;
using Xunit;

namespace Spedition.Fuel.Test.ServicesTests;

public class ClientRegistryTest : DbContextFactoryHelper
{
    [Fact]
    public async void AddFuelApiTest()
    {
        Microsoft.Extensions.DependencyInjection.IServiceCollection services = new ServiceCollection();
        var configSection = ConfigurationDevelop.GetSection(nameof(ApiConfigs));

        var fuelApiConfig = new ApiConfigs
        {
            Name = configSection[nameof(ApiConfigs.Name)],
            BaseUrl = configSection[nameof(ApiConfigs.BaseUrl)],
            Key = configSection[nameof(ApiConfigs.Key)],
            Version = configSection[nameof(ApiConfigs.Version)],
        };
        services = ClientRegistry.AddFuelApi(services, fuelApiConfig);
        var servicesProvider = services.BuildServiceProvider();
        var clientConnector = servicesProvider.GetRequiredService<IFuelTypesClientConnector>();
        Assert.NotNull(clientConnector);
    }
}
