using Microsoft.AspNetCore.Mvc;
using Moq;
using Spedition.Fuel.BusinessLayer.Services;
using Spedition.Fuel.BusinessLayer.Services.BaseServices;
using Spedition.FuelAPI.Controllers;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test;

public abstract class ControllerHelper<TController, TService> : AssertHelper
    where TController : ControllerBase
    where TService : class
{
    protected ControllerHelper(ITestOutputHelper output) : base(output)
    {
    }

    protected TController GetController<TRepository>()
        where TRepository : class
    {
        var fuelRepositories = GetFuelRepositories<TRepository>();

        var serviceItem = new Mock<TService>(Environment, fuelRepositories, Configuration, Mapper);

        return (new Mock<TController>(serviceItem.Object)).Object;
    }

    protected TController GetController()
    {
        var fuelRepositories = GetFuelRepositories();

        var serviceItem = new Mock<TService>(Environment, fuelRepositories, Configuration, Mapper);

        return (new Mock<TController>(serviceItem.Object)).Object;
    }
}
