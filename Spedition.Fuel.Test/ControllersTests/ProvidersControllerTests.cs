using Microsoft.AspNetCore.Mvc;
using Moq;
using Spedition.Fuel.BusinessLayer;
using Spedition.Fuel.BusinessLayer.Services;
using Spedition.Fuel.DataAccess.Infrastructure.Repositories.Interfaces;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.Entities;
using Spedition.FuelAPI.Controllers;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.ControllersTests;

public class ProvidersControllerTests : ControllerHelper<FuelProvidersController, FuelProvidersService>
{
    public ProvidersControllerTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    public async void GetProvidersTest()
    {
        // Arrange
        var controller = GetController<FuelProvider>();

        // Act
        Func<Task<List<FuelProviderResponse>>> action = async () => await controller.GetProviders(default);

        // Assert
        AssertAction<FuelProviderResponse>(action, (FuelProviderResponse fuelType) => fuelType.Id);
    }
}
