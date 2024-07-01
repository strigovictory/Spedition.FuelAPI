using Moq;
using Moq.Protected;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.BaseServices;
using Spedition.Fuel.BusinessLayer.Services.Parsers;
using Spedition.Fuel.BusinessLayer.Services.Parsers.ParserBaseServices;
using Spedition.Fuel.Shared.DTO.ResponseModels;
using Spedition.Fuel.Shared.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.Parsers;

public class ParserDiesel24Test : ParsersTestHelper
{
    public ParserDiesel24Test(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task MappingParsedToDBTest()
    {
        // Arrange
        var parsedReportsItemExistedCard = new FuelTransactionDiesel24
        {
            Card = "7896960402401342",
            OperationDate = DateTime.Now.AddDays(-5),
            OperationTime = "09:58",
            FuelType = "Diesel",
            Quantity = 300.82M,
            Country = "PL",
        };

        var parsedReportsItemNotExistedCard = new FuelTransactionDiesel24
        {
            Card = "9999999999999",
            OperationDate = DateTime.Now.AddDays(-5),
            OperationTime = "09:58",
            FuelType = "Diesel",
            Quantity = 300.82M,
            Country = "PL",
        };

        var fuelRepositories = GetFuelRepositories();
        var parserItem = new ParserDiesel24(Environment, fuelRepositories, Configuration, Mapper, null, null, null, null, null);
        parserItem.ProviderId = 7;
        await MappingParsedToDBHelper(parserItem, parsedReportsItemExistedCard, parsedReportsItemNotExistedCard);
    }
}
