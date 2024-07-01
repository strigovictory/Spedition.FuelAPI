using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Spedition.Fuel.BusinessLayer.Configs;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Models.E100;
using Spedition.Fuel.BusinessLayer.Models.GazProm;
using Spedition.Fuel.BusinessLayer.Services.Parsers;
using Spedition.Fuel.BusinessLayer.Services.ProvidersApi;
using Spedition.Fuel.BusinessLayer.Services.ProvidersApi.Helpers;
using Spedition.Fuel.Shared.Entities;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.Jobs;

public class GPNTests : AssertHelper
{
    public GPNTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetTransactionsTest()
    {
        // Arrange
        var fuelRepositories = GetFuelRepositories();
        // demo
        var mockOptionsDemo = new Mock<IOptions<GPNConfigDemo>>();
        var baseUrlDemo = Configuration.GetSection("GPNDemo:baseUrl").Value;
        mockOptionsDemo.SetupGet(options => options.Value).Returns(new GPNConfigDemo { BaseUrl = baseUrlDemo });
        // original
        var mockOptions = new Mock<IOptions<GPNConfig>>();
        var baseUrl = Configuration.GetSection("GPN:baseUrl").Value;
        mockOptions.SetupGet(options => options.Value).Returns(new GPNConfig { BaseUrl = baseUrl });

        var jobItem = new GPNService(Environment, fuelRepositories, Configuration, Mapper, null, null, null, null, null, mockOptions.Object, mockOptionsDemo.Object);

        //// Action
        //var actrionResult = await jobItem.GetTransactions();
        //var fueltransactions = jobItem?.GetType()?.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)?
        //    .FirstOrDefault(prop => prop.Name == "ItemsToMapping").GetValue(jobItem) as List<GPNTransaction>;

        //// Assert
        //Assert.True(actrionResult);
        //AssertAction<GPNTransaction>(fueltransactions);
    }
}
