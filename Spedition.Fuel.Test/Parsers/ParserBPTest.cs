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
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.Parsers;

public class ParserBPTest : ParsersTestHelper
{
    public ParserBPTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task MappingParsedToDBTest()
    {
        var parsedReportsItemExistedCard = new FuelTransactionBP
        {
            TransactionNumber = "test123456",
            Card_part_1 = "7006790478",
            Card_part_2 = "3100",
            Card_part_3 = "1736",
            OperationDate = "20230606",
            OperationTime = "10:01:00",
            FuelType = "ON ACT",
            NumberSign = "",
            Quantity = 4.698M,
            Cost = 432.22M,
            TotalCost = 1879.2M,
            Country = 29,
            Currency = "PLN",
        };

        var parsedReportsItemNotExistedCard = new FuelTransactionBP
        {
            TransactionNumber = "test123456",
            Card_part_1 = "11111111",
            Card_part_2 = "2222",
            Card_part_3 = "3333",
            OperationDate = "20230606",
            OperationTime = "10:01:00",
            FuelType = "ON ACT",
            NumberSign = "",
            Quantity = 4.698M,
            Cost = 432.22M,
            TotalCost = 1879.2M,
            Country = 29,
            Currency = "PLN",
        };

        var fuelRepositories = GetFuelRepositories();
        var parserItem = new ParserBP(Environment, fuelRepositories, Configuration, Mapper, null, null, null, null, null);
        parserItem.ProviderId = 11;
        await MappingParsedToDBHelper(parserItem, parsedReportsItemExistedCard, parsedReportsItemNotExistedCard);
    }

    [Fact]
    public void GetOperationDateTest()
    {
        DateTime defaultDateTime = default;

        var transactionCorrectDate = new FuelTransactionBP
        {
            TransactionNumber = "test123456",
            Card_part_1 = "7006790478",
            Card_part_2 = "3100",
            Card_part_3 = "1736",
            OperationDate = "20230606",
            OperationTime = "10:01:00",
            FuelType = "ON ACT",
            NumberSign = "",
            Quantity = 4.698M,
            Cost = 432.22M,
            TotalCost = 1879.2M,
            Country = 29,
            Currency = "PLN",
        };

        var transactionWrongDate = new FuelTransactionBP
        {
            TransactionNumber = "test123456",
            Card_part_1 = "7006790478",
            Card_part_2 = "3100",
            Card_part_3 = "1736",
            OperationDate = "zsfdxgh",
            OperationTime = "fdgxhjk",
            FuelType = "ON ACT",
            NumberSign = "",
            Quantity = 4.698M,
            Cost = 432.22M,
            TotalCost = 1879.2M,
            Country = 29,
            Currency = "PLN",
        };

        var fuelRepositories = GetFuelRepositories();
        var parserItem = new ParserBP(Environment, fuelRepositories, Configuration, Mapper, null, null, null, null, null);
        var methods = parserItem.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
        var methodsSameName = methods?.Where(mi => mi.Name.Equals("GetOperationDate"));
        var methodToInvoke = methodsSameName?.FirstOrDefault(
            mi => mi.GetParameters().Count() == 1
            && mi.GetParameters().Any(pi => pi.ParameterType == typeof(FuelTransactionBP)));

        // correct date
        var actionResultObjectCorrect = methodToInvoke.Invoke(obj: parserItem, parameters: new object[] { transactionCorrectDate });
        var actionResultCorrect = Assert.IsAssignableFrom<DateTime?>(actionResultObjectCorrect);
        Output.WriteLine($"GetDivisions operation date: {actionResultCorrect.ToString()}");
        Assert.NotEqual(defaultDateTime, actionResultCorrect);

        // wrong date
        var actionResultObjectWrong = methodToInvoke.Invoke(obj: parserItem, parameters: new object[] { transactionWrongDate });
        var actionResultWrong = Assert.IsAssignableFrom<DateTime?>(actionResultObjectWrong);
        Assert.Equal(defaultDateTime, actionResultWrong);
    }
}
