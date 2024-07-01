using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Spedition.Fuel.BusinessLayer.Configs;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Models.E100;
using Spedition.Fuel.BusinessLayer.Services.Parsers;
using Spedition.Fuel.BusinessLayer.Services.ProvidersApi;
using Spedition.Fuel.BusinessLayer.Services.ProvidersApi.Helpers;
using Spedition.Fuel.Shared.Entities;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.Jobs;

public class E100Tests : AssertHelper
{
    public E100Tests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetTransactionsTest()
    {
        // Arrange
        var fuelRepositories = GetFuelRepositories();
        var mockOptions = new Mock<IOptions<E100Config>>();
        var baseUrl = Configuration.GetSection("FuelProvidersApi:E100:baseUrl").Value;
        mockOptions.SetupGet(options => options.Value).Returns(new E100Config { BaseUrl = baseUrl });
        var jobItem = new E100Service(Environment, fuelRepositories, Configuration, Mapper, null, null, null, null, null, mockOptions.Object);

        // Action
        var actrionResult = await jobItem.GetTransactions();
        var fueltransactions = jobItem?.GetType()?.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)?
        .FirstOrDefault(prop => prop.Name == "ItemsToMapping").GetValue(jobItem) as List<E100Transaction>;

        // Assert
        Assert.True(actrionResult);
        AssertAction<E100Transaction>(fueltransactions);
    }
}
