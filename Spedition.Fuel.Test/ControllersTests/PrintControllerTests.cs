using AutoMapper;
using Moq;
using Spedition.Fuel.BusinessLayer.Services.Parsers;
using Spedition.Fuel.BusinessLayer.Services.Print;
using Spedition.Fuel.Shared.DTO.RequestModels.Print;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.DTO.ResponseModels;
using Spedition.Fuel.Shared.Interfaces;
using Spedition.FuelAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Configuration.Provider;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Spedition.Fuel.Test.ControllersTests
{
    public class PrintControllerTests : TestsHelper
    {
        public PrintControllerTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void PrintTransactionsTest()
        {
            #region Arrange
            var printGeneratorTransactions = new PrintExcelTransactions<TransactionPrintRequest>(Environment);

            var printGenerators = new List<PrintExcelBase<IPrint>>();
            {
                if(printGeneratorTransactions is PrintExcelBase<IPrint> printGeneratorTransactionToAdd) printGenerators.Add(printGeneratorTransactionToAdd);
            };

            var controller = new Mock<FuelPrintController>(printGenerators);

            var transactions = new List<TransactionPrintRequest>
            {
                new TransactionPrintRequest
                {
                    Id = 1,
                    TransactionID = "",
                    OperationDate = DateTime.Now,
                    Quantity = 5,
                    Cost = 10,
                    TotalCost = 50,
                    IsCheck = false,
                    DivisionName = "Rezon",
                    ProviderName = "BP",
                    FuelTypeName = "Топливо",
                    CurrencyName = "BYN",
                    CardName = "89562 sd fgh",
                    CountryName = "РБ",
                    DriverReportId = default,
                },
                new TransactionPrintRequest
                {
                    Id = 5,
                    TransactionID = "1234567890",
                    OperationDate = DateTime.Now.AddDays(-10),
                    Quantity = null,
                    Cost = null,
                    TotalCost = null,
                    IsCheck = true,
                    DivisionName = "Фестина",
                    ProviderName = "Лидер",
                    FuelTypeName = "AdBlue",
                    CurrencyName = "PLN",
                    CardName = "123845 sfdg 12-dcvf",
                    CountryName = "Польша",
                    DriverReportId = default,
                },

            };
            #endregion

            // Act
            var actionTransactions = controller.Object.PrintTransactions(transactions).Result;

            // Assert
            var actionResultTransactions = Assert.IsAssignableFrom<byte[]>(actionTransactions);

            Assert.NotNull(actionResultTransactions);

            Output.WriteLine($"Generated file (size {actionResultTransactions?.Length ?? 0}) ");

            Assert.True((actionResultTransactions?.Length ?? 0) > 0);
        }

        [Fact]
        public void PrintCardsTest()
        {
            #region Arrange
            var printGeneratorCards = new PrintExcelCards<CardPrintRequest>(Environment);

            var printGenerators = new List<PrintExcelBase<IPrint>>();
            {
                if (printGeneratorCards is PrintExcelBase<IPrint> printGeneratorCardToAdd) printGenerators.Add(printGeneratorCardToAdd);
            };

            var controller = new Mock<FuelPrintController>(printGenerators);

            var cards = new List<CardPrintRequest>
            {
                new CardPrintRequest
                {
                    Id = 1,
                    Number = null,
                    ExpirationDate = null,
                    ReceiveDate = null,
                    IssueDate = null,
                    IsReserve = false,
                    Note = null,
                    IsArchive = false,
                    CarName = null,
                    DivisionName = null,
                    ProviderName = null,
                },
                new CardPrintRequest
                {
                    Id = 2,
                    Number = "12365 hjkkl 89",
                    ExpirationDate = DateTime.Now.AddYears(3),
                    ReceiveDate = DateTime.Now,
                    IssueDate = DateTime.Now,
                    IsReserve = false,
                    Note = "test",
                    IsArchive = true,
                    CarName = "2365 bg-23",
                    DivisionName = "Rezon",
                    ProviderName = "BP",
                },
            };
            #endregion

            // Act
            var actionCards = controller.Object.PrintCards(cards).Result;

            // Assert
            var actionResultCards = Assert.IsAssignableFrom<byte[]>(actionCards);

            Assert.NotNull(actionResultCards);

            Output.WriteLine($"Generated file (size {actionResultCards?.Length ?? 0}) ");

            Assert.True((actionResultCards?.Length ?? 0) > 0);
        }
    }
}
