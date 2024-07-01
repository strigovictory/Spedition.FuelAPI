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

public class ParserGazPromTest : ParsersTestHelper
{
    public ParserGazPromTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task MappingParsedToDBTest()
    {
        // Arrange
        var parsedReportsItemExistedCard = new FuelTransactionGazProm
        {
            Card = "7005830810047803",
            OperationDate = DateTime.Now,
            FuelType = "ДТ",
            Quantity = -150M,
            Cost = 2.46000003814697M,
            TotalCost = -369M,
            Country = "RUS",
            Currency = "RUR",
            CountryWorksheet = "LK_Transactions",
        };

        var parsedReportsItemNotExistedCard = new FuelTransactionGazProm
        {
            Card = "10000020000030000",
            OperationDate = DateTime.Now,
            FuelType = "ДТ",
            Quantity = -150M,
            Cost = 2.46000003814697M,
            TotalCost = -369M,
            Country = "RUS",
            Currency = "RUR",
            CountryWorksheet = "LK_Transactions",
        };

        var fuelRepositories = GetFuelRepositories();
        var parserItem = new ParserGazProm(Environment, fuelRepositories, Configuration, Mapper, null, null, null, null, null);
        parserItem.ProviderId = 9;
        await MappingParsedToDBHelper(parserItem, parsedReportsItemExistedCard, parsedReportsItemNotExistedCard);
    }
}
