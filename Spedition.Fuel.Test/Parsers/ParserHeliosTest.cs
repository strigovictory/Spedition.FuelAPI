using Moq;
using Moq.Protected;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.BaseServices;
using Spedition.Fuel.BusinessLayer.Services.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.Parsers;
using Spedition.Fuel.BusinessLayer.Services.Parsers.ParserBaseServices;
using Spedition.Fuel.Shared.DTO.ResponseModels;
using Spedition.Fuel.Shared.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.Parsers;

public class ParserHeliosTest : ParsersTestHelper
{
    public ParserHeliosTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task MappingParsedToDBTest()
    {
        // Arrange
        var parsedReportsItemExistedCard = new FuelTransactionHelios
        {
            OperationDateTime = DateTime.Now,
            Card = "1206760000213057",
            FuelType = "ДТ-Л",
            Quantity = 0.24M,
            Cost = null,
            TotalCost = null,
        };

        var parsedReportsItemNotExistedCard = new FuelTransactionHelios
        {
            OperationDateTime = DateTime.Now,
            Card = "0111111111111222",
            FuelType = "ДТ-Л",
            Quantity = 0.24M,
            Cost = null,
            TotalCost = null,
        };

        var fuelRepositories = GetFuelRepositories();
        var parserItem = new ParserHelios(Environment, fuelRepositories, Configuration, Mapper, null, null, null, null, null);
        parserItem.ProviderId = 21;
        await MappingParsedToDBHelper(parserItem, parsedReportsItemExistedCard, parsedReportsItemNotExistedCard);
    }
}
