using Moq;
using Moq.Protected;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.BaseServices;
using Spedition.Fuel.BusinessLayer.Services.Parsers;
using Spedition.Fuel.BusinessLayer.Services.Parsers.ParserBaseServices;
using Spedition.Fuel.Shared.DTO.ResponseModels;
using Spedition.Fuel.Shared.Entities;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.Parsers;

public class ParserAdnocTest : ParsersTestHelper
{
    public ParserAdnocTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task MappingParsedToDBTest()
    {
        // Arrange
        var parsedReportsItemExistedCard = new FuelTransactionAdnoc
        {
            TrucksNumber = "AP 8258-5",
            DivisionName = "Райзинг ООО",
            OperationDate = DateTime.Now.AddDays(-5),
            OperationTime = DateTime.Now.AddMinutes(-36),
            Place = "Смоленская область, р-н Смоленский, 449 км.",
            FuelType = "AdBlue",
            Quantity = 544.6M,
            Cost = 42.8M,
            TotalCost = 23308.88M,
        };

        var parsedReportsItemNotExistedCard = new FuelTransactionAdnoc
        {
            TrucksNumber = "00 0000-0",
            DivisionName = "Райзинг ООО",
            OperationDate = DateTime.Now.AddDays(-5),
            OperationTime = DateTime.Now.AddMinutes(-36),
            Place = "Смоленская область, р-н Смоленский, 449 км.",
            FuelType = "AdBlue",
            Quantity = 544.6M,
            Cost = 42.8M,
            TotalCost = 23308.88M,
        };

        var fuelRepositories = GetFuelRepositories();
        var parserItem = new ParserAdnoc(Environment, fuelRepositories, Configuration, Mapper, null, null, null, null, null);
        parserItem.ProviderId = 19;
        await MappingParsedToDBHelper(parserItem, parsedReportsItemExistedCard, parsedReportsItemNotExistedCard);
    }
}
