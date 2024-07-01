using Microsoft.Extensions.DependencyInjection;
using Moq;
using Spedition.Fuel.BusinessLayer.Services;
using Spedition.Fuel.DataAccess.Infrastructure.Repositories.Interfaces;
using Spedition.Fuel.DataAccess.Infrastructure.Repositories;
using Spedition.Fuel.Shared.Entities;
using Xunit;
using Xunit.Abstractions;
using Spedition.Fuel.Shared.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Spedition.Fuel.DataAccess;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.Test.ServicesTests;

public class TransactionsServiceTests : AssertHelper
{
    public TransactionsServiceTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void GetTransactionsTest()
    {
        // Arrange
        var transactionRepository = GetFuelTransactionRepository();
        var serviceItem = new Mock<FuelTransactionService>(transactionRepository, Environment, Configuration, Mapper);
        int? toTake = 100000;
        int? toSkip = 100000;

        // Act
        Func<Task<List<FuelTransactionFullResponse>>> action = async () => await serviceItem.Object.GetTransactions(default, toTake, toSkip);

        // Assert
        AssertAction<FuelTransactionFullResponse>(action, (FuelTransactionFullResponse transaction) => transaction.Id);
    }

    protected FuelTransactionRepository GetFuelTransactionRepository()
    {
        var services = new ServiceCollection();

        services.AddDbContext<SpeditionContext>(
            options => options.UseSqlServer(Configuration.GetConnectionString("SpeditionDb")),
            ServiceLifetime.Singleton);

        var servicesProvider = services.BuildServiceProvider();
        var context = servicesProvider.GetRequiredService<SpeditionContext>();
        return new FuelTransactionRepository(context);
    }
}
