using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Spedition.Fuel.BusinessLayer;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Services.BaseServices;
using Spedition.Fuel.BusinessLayer.Services.Parsers;
using Spedition.Fuel.BusinessLayer.Services.Parsers.ParserBaseServices;
using Spedition.Fuel.Shared.DTO.ResponseModels;
using Spedition.Fuel.Shared.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.Parsers;

public class ParserBaseFuelReportTest : TestsHelper
{
    public ParserBaseFuelReportTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task CheckIsInstanceExistTest()
    {
        // Arrange
        var existedTransaction = new FuelTransaction
        {
            Id = 0,
            TransactionID = string.Empty,
            CardId = 5024,
            Quantity = 300,
            TotalCost = 150.00M,
            OperationDate = new DateTime(day: 17, month: 2, year: 2021, hour: 7, minute: 38, second: 25), // "17.02.2021 7:38:25.000"
        };

        var notExistedTransaction = new FuelTransaction
        {
            Id = 3,
            TransactionID = string.Empty,
            CardId = 5024,
            Quantity = 300,
            TotalCost = 150.00M,
            OperationDate = new DateTime(day: 17, month: 2, year: 2021, hour: 7, minute: 38, second: 25), // "17.02.2021 7:38:25.000"
        };

        var fuelRepositories = GetFuelRepositories();
        var mockParserItem = new ParserBP(Environment, fuelRepositories, Configuration, Mapper, null, null, null, null, null);

        var mockBase = new Mock<SearchBase<FuelTransaction>>(Configuration, Environment);
        mockBase.Setup(parser => parser.CheckIsInstanceExist(existedTransaction))
            .Returns((mockParserItem as SearchBase<FuelTransaction>).CheckIsInstanceExist(existedTransaction));

        // Action
        var action = async (FuelTransaction trans) => await mockBase.Object.CheckIsInstanceExist(trans);

        // Assert - exist
        Assert.True(await action(existedTransaction));
        Output.WriteLine($"{existedTransaction.ToString()} exist.");

        // Assert - not exist
        Assert.False(await action(notExistedTransaction));
        Output.WriteLine($"{notExistedTransaction.ToString()} not exist.");
    }
}
