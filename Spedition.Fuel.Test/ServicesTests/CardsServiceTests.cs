using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Spedition.Fuel.BusinessLayer.Services;
using Spedition.Fuel.BusinessLayer.Services.ProvidersApi;
using Spedition.Fuel.DataAccess.Infrastructure.Repositories;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.Settings;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.ServicesTests;

public class CardsServiceTests : AssertHelper
{
    public CardsServiceTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void GetCardsTest()
    {
        // Arrange
        var repository = GetFuelCardRepository();
        var serviceItem = new Mock<FuelCardsService>(repository, Environment, Configuration, Mapper);

        // Act
        Func<Task<List<FuelCardFullResponse>>> action = async () => await serviceItem.Object.GetCards(default);

        // Assert
        AssertAction<FuelCardFullResponse>(action, (FuelCardFullResponse card) => card.Id);
    }

    protected FuelCardRepository GetFuelCardRepository()
    {
        var services = new ServiceCollection();

        services.AddDbContext<SpeditionContext>(
            options => options.UseSqlServer(Configuration.GetConnectionString("SpeditionDb")),
            ServiceLifetime.Singleton);

        var servicesProvider = services.BuildServiceProvider();
        var context = servicesProvider.GetRequiredService<SpeditionContext>();
        return new FuelCardRepository(context);
    }
}
