using Microsoft.Extensions.Options;
using Moq;
using Spedition.Fuel.BusinessLayer.Configs;
using Spedition.Fuel.BusinessLayer.Models.E100;
using Spedition.Fuel.BusinessLayer.Services.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.Parsers;
using Spedition.Fuel.BusinessLayer.Services.ProvidersApi;
using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.Enums;
using Spedition.FuelAPI.Controllers;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.ControllersTests;

public class JobsControllerTests : TestsHelper
{
    public JobsControllerTests(ITestOutputHelper output)
    : base(output)
    {
    }

    private FuelJobsController GetController(ProvidersApiNames provider)
    {
        // Arrange
        var mockFuelRepositories = GetFuelRepositories();
        //
        var mockOptionsE100 = new Mock<IOptions<E100Config>>();
        var configE100 = Configuration.GetSection($"FuelProvidersApi:{nameof(ProvidersApiNames.E100)}");
        mockOptionsE100.SetupGet(options => options.Value).Returns(new E100Config { BaseUrl = configE100[nameof(E100Config.BaseUrl)], Name = configE100[nameof(E100Config.Name)] });
        //
        var mockOptionsGPN = new Mock<IOptions<GPNConfig>>();
        var configGPN = Configuration.GetSection($"FuelProvidersApi:{nameof(ProvidersApiNames.GPN)}");
        mockOptionsGPN.SetupGet(options => options.Value).Returns(new GPNConfig { BaseUrl = configGPN[nameof(GPNConfig.BaseUrl)], Name = configGPN[nameof(GPNConfig.Name)] });

        var mockOptionsDemoGPN = new Mock<IOptions<GPNConfigDemo>>();
        var configGPNDemo = Configuration.GetSection($"FuelProvidersApi:{nameof(ProvidersApiNames.GPNDemo)}");
        mockOptionsDemoGPN.SetupGet(options => options.Value).Returns(new GPNConfigDemo { BaseUrl = configGPNDemo[nameof(GPNConfigDemo.BaseUrl)], Name = configGPNDemo[nameof(GPNConfigDemo.Name)] });
        //
        var mockOptionsNeftika = new Mock<IOptions<NeftikaConfig>>();
        var configNeftika = Configuration.GetSection($"FuelProvidersApi:{nameof(ProvidersApiNames.Neftika)}");
        mockOptionsNeftika.SetupGet(options => options.Value).Returns(new NeftikaConfig { BaseUrl = configNeftika[nameof(NeftikaConfig.BaseUrl)], Name = configNeftika[nameof(NeftikaConfig.Name)] });
        //
        var mockOptionsRosneft = new Mock<IOptions<RosneftConfig>>();
        var configRosneft = Configuration.GetSection($"FuelProvidersApi:{nameof(ProvidersApiNames.Rosneft)}");
        mockOptionsRosneft.SetupGet(options => options.Value).Returns(new RosneftConfig { BaseUrl = configRosneft[nameof(RosneftConfig.BaseUrl)], Name = configRosneft[nameof(RosneftConfig.Name)] });
        //
        var mockOptionsTatneft = new Mock<IOptions<TatneftConfig>>();
        var configTatneft = Configuration.GetSection($"FuelProvidersApi:{nameof(ProvidersApiNames.Tatneft)}");
        mockOptionsTatneft.SetupGet(options => options.Value).Returns(new TatneftConfig { BaseUrl = configTatneft[nameof(TatneftConfig.BaseUrl)], Name = configTatneft[nameof(TatneftConfig.Name)] });

        IProvidersApiBase<FuelTransactionShortResponse, NotParsedTransaction> providersApiService = provider switch
        {
            ProvidersApiNames.E100 => new E100Service(Environment, mockFuelRepositories, Configuration, Mapper, null, null, null, null, null, mockOptionsE100.Object),
            ProvidersApiNames.GPN => new GPNService(Environment, mockFuelRepositories, Configuration, Mapper, null, null, null, null, null, mockOptionsGPN.Object, mockOptionsDemoGPN.Object),
            ProvidersApiNames.Neftika => new NeftikaService(Environment, mockFuelRepositories, Configuration, Mapper, null, null, null, null, null, mockOptionsNeftika.Object),
            ProvidersApiNames.Rosneft => new RosneftService(Environment, mockFuelRepositories, Configuration, Mapper, null, null, null, null, null, mockOptionsRosneft.Object),
            ProvidersApiNames.Tatneft => new TatneftService(Environment, mockFuelRepositories, Configuration, Mapper, null, null, null, null, null, mockOptionsTatneft.Object),
        };

        var providers = new List<IProvidersApiBase<FuelTransactionShortResponse, NotParsedTransaction>>
        {
            providersApiService
        };

        return new FuelJobsController(providers);
    }

    private async Task DoJobTest(ProvidersApiNames provider)
    {
        var controller = GetController(provider);

        // Act
        var action = async () => await controller.DoJob(provider.ToString());

        // Assert
        Output.WriteLine($"Start job: «{provider.ToString() ?? string.Empty}». ");

        var actionResultInner = Assert.IsAssignableFrom<ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>>(action.Invoke().Result);

        Output.WriteLine($"Added to DB {actionResultInner?.SuccessItems?.Count ?? 0} successItems: ");
        actionResultInner?.SuccessItems?.ForEach(transaction => Output.WriteLine(transaction?.ToString() ?? string.Empty));

        Output.WriteLine($"Updated in DB {actionResultInner?.SecondarySuccessItems?.Count ?? 0} successItems: ");
        actionResultInner?.SecondarySuccessItems?.ForEach(transaction => Output.WriteLine(transaction?.ToString() ?? string.Empty));

        Output.WriteLine($"GetDivisions {actionResultInner?.NotSuccessItems?.Count ?? 0} notsuccessItems: ");
        actionResultInner?.NotSuccessItems?.ForEach(transaction => Output.WriteLine(
            $"{transaction?.NotSuccessItem?.ToString() ?? string.Empty} - {transaction?.Reason?.ToString() ?? string.Empty}"));

        Assert.True((actionResultInner?.SuccessItems?.Count ?? 0) > 0
            || (actionResultInner?.SecondarySuccessItems?.Count ?? 0) > 0
            || (actionResultInner?.NotSuccessItems?.Count ?? 0) > 0);
    }

    [Fact]
    public async Task DoJobTestE100()
    {
        await DoJobTest(ProvidersApiNames.E100);
    }

    [Fact]
    public async Task DoJobTestGPN()
    {
        await DoJobTest(ProvidersApiNames.GPN);
    }

    [Fact]
    public async Task DoJobTestNeftika()
    {
        await DoJobTest(ProvidersApiNames.Neftika);
    }

    [Fact]
    public async Task DoJobTestRosneft()
    {
        await DoJobTest(ProvidersApiNames.Rosneft);
    }

    [Fact]
    public async Task DoJobTestTatneft()
    {
        await DoJobTest(ProvidersApiNames.Tatneft);
    }
}
