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

public class ParserDieselTransTest : ParsersTestHelper
{
    public ParserDieselTransTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task MappingParsedToDBTest()
    {
        // Arrange
        var parsedReportsItemExistedCard = new FuelTransactionDieselTrans
        {
            OperationDateTime = DateTime.Now,
            Card = "AP 4822-5",
            FuelType = "ДТФ",
            Quantity = 50.5M,
            Cost = 286.47M,
            TotalCost = 14466.74M,
        };

        var parsedReportsItemNotExistedCard = new FuelTransactionDieselTrans
        {
            OperationDateTime = DateTime.Now,
            Card = "212121",
            FuelType = "ДТФ",
            Quantity = 50.5M,
            Cost = 286.47M,
            TotalCost = 14466.74M,
        };

        var fuelRepositories = GetFuelRepositories();
        var parserItem = new ParserDieselTrans(Environment, fuelRepositories, Configuration, Mapper, null, null, null, null, null);
        parserItem.ProviderId = 23;

        // Action + assert
        await MappingParsedToDBHelper(parserItem, parsedReportsItemExistedCard, parsedReportsItemNotExistedCard);
    }
}
