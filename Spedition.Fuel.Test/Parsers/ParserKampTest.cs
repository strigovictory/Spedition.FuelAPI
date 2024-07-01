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

public class ParserKampTest : ParsersTestHelper
{
    public ParserKampTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task MappingParsedToDBTest()
    {
        // Arrange
        var parsedReportsItemExistedCard = new FuelTransactionKamp
        {
            OperationDateTime = DateTime.Now,
            CarNum = "АР 5417-5",
            FuelType = "ДТ",
            Quantity = 1050M,
            Cost = 48,
            TotalCost = 50400,
        };

        var parsedReportsItemNotExistedCard = new FuelTransactionKamp
        {
            OperationDateTime = DateTime.Now,
            CarNum = "00 0000-0",
            FuelType = "ДТ",
            Quantity = 1050M,
            Cost = 48,
            TotalCost = 50400,
        };

        var fuelRepositories = GetFuelRepositories();
        var parserItem = new ParserKamp(Environment, fuelRepositories, Configuration, Mapper, null, null, null, null, null);
        parserItem.ProviderId = 24;
        await MappingParsedToDBHelper(parserItem, parsedReportsItemExistedCard, parsedReportsItemNotExistedCard);
    }
}
