using Moq;
using Moq.Protected;
using NPOI.OpenXmlFormats.Wordprocessing;
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

public class ParserLiderTest : ParsersTestHelper
{
    public ParserLiderTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task MappingParsedToDBTest()
    {
        // Arrange
        var parsedReportsItemExistedCard = new FuelTransactionLider
        {
            OperationDateTime = DateTime.Now,
            Card = "10421",
            FuelType = "ДТФ",
            Quantity = 50.5M,
            Cost = 286.47M,
            TotalCost = 14466.74M,
        };

        var parsedReportsItemNotExistedCard = new FuelTransactionLider
        {
            OperationDateTime = DateTime.Now,
            Card = "212121",
            FuelType = "ДТФ",
            Quantity = 50.5M,
            Cost = 286.47M,
            TotalCost = 14466.74M,
        };

        var fuelRepositories = GetFuelRepositories();
        var parserItem = new ParserLider(Environment, fuelRepositories, Configuration, Mapper, null, null, null, null, null);
        parserItem.ProviderId = 17;

        // Action + assert
        await MappingParsedToDBHelper(parserItem, parsedReportsItemExistedCard, parsedReportsItemNotExistedCard);
    }
}
