using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Spedition.Fuel.BusinessLayer.Configs;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Models.E100;
using Spedition.Fuel.BusinessLayer.Models.Neftika;
using Spedition.Fuel.BusinessLayer.Models.Rosneft;
using Spedition.Fuel.BusinessLayer.Services.Parsers;
using Spedition.Fuel.BusinessLayer.Services.ProvidersApi;
using Spedition.Fuel.BusinessLayer.Services.ProvidersApi.Helpers;
using Spedition.Fuel.Shared.Entities;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.Jobs;

public class RosneftTests : AssertHelper
{
    public RosneftTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetTransactionsTest()
    {
        // Arrange
        var fuelRepositories = GetFuelRepositories();
        var mockOptions = new Mock<IOptions<RosneftConfig>>();
        var baseUrl = Configuration.GetSection("Rosneft:BaseUrl").Value;
        mockOptions.SetupGet(options => options.Value).Returns(new RosneftConfig { BaseUrl = baseUrl });
        var jobItem = new RosneftService(Environment, fuelRepositories, Configuration, Mapper, null, null, null, null, null, mockOptions.Object);

        // Action
        var actrionResult = await jobItem.GetTransactions();
        var transactions = jobItem?.GetType()?.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)?
            .FirstOrDefault(prop => prop.Name == "ItemsToMapping").GetValue(jobItem) as List<RosneftTransaction>;

        // Assert
        Assert.True(actrionResult);
        AssertAction(transactions);
    }
}
