using Moq;
using Spedition.Fuel.BusinessLayer;
using Spedition.Fuel.BusinessLayer.Services;
using Spedition.Fuel.BusinessLayer.Services.Interfaces;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.Entities;
using Spedition.FuelAPI.Controllers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.ControllersTests;

public class FuelTypesControllerTests : ControllerHelper<FuelTypesController, FuelTypesService>
{
    public FuelTypesControllerTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    public async void GetFuelTypesTest()
    {
        // Arrange
        var controller = GetController<FuelType>();

        // Act
        Func<Task<List<FuelTypeResponse>>> action = async () => await controller.GetFuelTypes(default);

        // Assert
        AssertAction<FuelTypeResponse>(action, (FuelTypeResponse fuelType) => fuelType.Id);
    }
}
