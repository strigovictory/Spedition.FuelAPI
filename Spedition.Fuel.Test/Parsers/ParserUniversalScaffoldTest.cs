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
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.Parsers;

public class ParserUniversalScaffoldTest : ParsersTestHelper
{
    public ParserUniversalScaffoldTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async void MappingParsedToDBTest()
    {
        // Arrange
        var parsedReportsItemExistedCard = new FuelTransactionUniversalScaffold
        {
            OperationDateTime_Day = DateTime.Now,
            OperationDateTime_Hour = 1,
            OperationDateTime_Minute = 15,
            CarNum = "AP 4822-5",
            Quantity = 400M,
            Cost = 44.2M,
            FuelType = "топливо",
        };

        var parsedReportsItemNotExistedCard = new FuelTransactionUniversalScaffold
        {
            OperationDateTime_Day = DateTime.Now,
            OperationDateTime_Hour = 1,
            OperationDateTime_Minute = 15,
            CarNum = "11 1111-1",
            Quantity = 400M,
            Cost = 44.2M,
            FuelType = "топливо",
        };

        var fuelRepositories = GetFuelRepositories();
        var parserItem = new ParserUniversalScaffold(Environment, fuelRepositories, Configuration, Mapper, null, null, null, null, null);
        parserItem.ProviderId = 23;
        await MappingParsedToDBHelper(parserItem, parsedReportsItemExistedCard, parsedReportsItemNotExistedCard);
    }

    [Fact]
    public void GetOperationDateTest()
    {
        DateTime defaultDateTime = default;

        var transactionCorrectDate = new
        {
            OperationDateTime_Day = DateTime.Now,
            OperationDateTime_Hour = 1,
            OperationDateTime_Minute = 15,
        };

        var transactionWrongDate = new
        {
            OperationDateTime_Day = DateTime.Now,
            OperationDateTime_Hour = 25,
            OperationDateTime_Minute = 67,
        };

        var fuelRepositories = GetFuelRepositories();
        var parserItem = new ParserUniversalScaffold(Environment, fuelRepositories, Configuration, Mapper, null, null, null, null, null);
        var methods = parserItem.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
        var methodsSameName = methods?.Where(mi => mi.Name.Equals("GetOperationDate"));
        var methodToInvoke = methodsSameName?.FirstOrDefault(
            mi => mi.GetParameters().Count() == 3);

        // correct date
        var actionResultObjectCorrect = methodToInvoke.Invoke(
            obj: parserItem,
            parameters: new object[]
            {
                transactionCorrectDate.OperationDateTime_Day,
                transactionCorrectDate.OperationDateTime_Hour,
                transactionCorrectDate.OperationDateTime_Minute,
            });
        var actionResultCorrect = Assert.IsAssignableFrom<DateTime?>(actionResultObjectCorrect);
        Output.WriteLine($"GetDivisions operation date: {actionResultCorrect.ToString()}");
        Assert.NotEqual(defaultDateTime, actionResultCorrect);

        // wrong date
        var actionResultObjectWrong = methodToInvoke.Invoke(
            obj: parserItem,
            parameters: new object[]
            {
                transactionWrongDate.OperationDateTime_Day,
                transactionWrongDate.OperationDateTime_Hour,
                transactionWrongDate.OperationDateTime_Minute,
            });
        var actionResultWrong = Assert.IsAssignableFrom<DateTime?>(actionResultObjectWrong);
        Assert.Equal(defaultDateTime, actionResultWrong);
    }
}
