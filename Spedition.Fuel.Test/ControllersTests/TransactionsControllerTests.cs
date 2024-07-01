using Moq;
using Spedition.Fuel.BusinessLayer;
using Spedition.Fuel.BusinessLayer.Services;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.Entities;
using Spedition.Fuel.Test.ServicesTests;
using Spedition.FuelAPI.Controllers;
using System.Transactions;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.ControllersTests;

public class TransactionsControllerTests : TransactionsServiceTests
{
    public TransactionsControllerTests(ITestOutputHelper output) : base(output)
    {
    }

    private static (int? empty, int zero, int small, int big, int notExist) paginationTestValues = (null, 0, 5, 100000, 1000000);

    private int? take = 1;
    private int? skip = 0;

    private FuelTransactionsController GetFuelTransactionController()
    {
        var repository = GetFuelTransactionRepository();

        var serviceItem = new Mock<FuelTransactionService>(repository, Environment, Configuration, Mapper);

        return (new Mock<FuelTransactionsController>(serviceItem.Object)).Object;
    }

    // Act
    private Func<Task<List<FuelTransactionFullResponse>>> Action
    {
        get
        {
            Func<Task<List<FuelTransactionFullResponse>>> action = async () => await GetFuelTransactionController().GetTransactions(default, take, skip);
            return action;
        }
    }

    #region Get
    [Fact]
    public async void GetTransactionsTest_take_empty_skip_big()
    {
        take = paginationTestValues.empty;
        skip = paginationTestValues.big;
        AssertAction<FuelTransactionFullResponse>(
            Action,
            new Predicate<List<FuelTransactionFullResponse>>[]
            {
                (List<FuelTransactionFullResponse> actionResult) => (actionResult?.Count() ?? 0) == 1,
            });
    }

    [Fact]
    public async void GetTransactionsTest_take_small_skip_empty()
    {
        take = paginationTestValues.small;
        skip = paginationTestValues.empty;
        AssertAction<FuelTransactionFullResponse>(
            Action,
            new Predicate<List<FuelTransactionFullResponse>>[]
            {
                (List<FuelTransactionFullResponse> actionResult) => (actionResult?.Count() ?? 0) == take,
            });
    }

    [Fact]
    public async void GetTransactionsTest_take_ziro_skip_zero()
    {
        take = paginationTestValues.zero;
        skip = paginationTestValues.zero;
        AssertAction<FuelTransactionFullResponse>(
            Action,
            new Predicate<List<FuelTransactionFullResponse>>[]
            {
                (List<FuelTransactionFullResponse> actionResult) => (actionResult?.Count() ?? 0) == 0,
            });
    }

    [Fact]
    public async void GetTransactionsTest_take_small_skip_null()
    {
        take = paginationTestValues.small;
        skip = paginationTestValues.empty;
        AssertAction<FuelTransactionFullResponse>(
            Action,
            new Predicate<List<FuelTransactionFullResponse>>[]
            {
                (List<FuelTransactionFullResponse> actionResult) => (actionResult?.Count() ?? 0) == paginationTestValues.small,
                (List<FuelTransactionFullResponse> actionResult) => actionResult?.All(transaction => transaction.Id > 0) ?? false,
            });
    }

    [Fact]
    public async void GetTransactionsTest_take_small_skip_small()
    {
        take = paginationTestValues.small;
        skip = paginationTestValues.small;
        AssertAction<FuelTransactionFullResponse>(
            Action,
            new Predicate<List<FuelTransactionFullResponse>>[]
            {
                (List<FuelTransactionFullResponse> actionResult) => (actionResult?.Count() ?? 0) == paginationTestValues.small,
                (List<FuelTransactionFullResponse> actionResult) => actionResult?.All(transaction => transaction.Id > 0) ?? false,
            });
    }

    [Fact]
    public async void GetTransactionsTest_take_big_skip_null()
    {
        take = paginationTestValues.big;
        skip = paginationTestValues.empty;
        AssertAction<FuelTransactionFullResponse>(
            Action,
            new Predicate<List<FuelTransactionFullResponse>>[]
            {
                (List<FuelTransactionFullResponse> actionResult) => (actionResult?.Count() ?? 0) == paginationTestValues.big,
                (List<FuelTransactionFullResponse> actionResult) => actionResult?.All(transaction => transaction.Id > 0) ?? false,
            });
    }

    [Fact]
    public async void GetTransactionsTest_take_small_skip_notexist()
    {
        take = paginationTestValues.small;
        skip = paginationTestValues.notExist;
        AssertAction<FuelTransactionFullResponse>(
            Action,
            new Predicate<List<FuelTransactionFullResponse>>[]
            {
                (List<FuelTransactionFullResponse> actionResult) => (actionResult?.Count() ?? 0) == paginationTestValues.zero,
            });
    }

    [Fact]
    public async void GetTransactionsTest_take_notexist_skip_null()
    {
        take = paginationTestValues.notExist;
        skip = paginationTestValues.empty;
        AssertAction<FuelTransactionFullResponse>(
            Action,
            new Predicate<List<FuelTransactionFullResponse>>[]
            {
                (List<FuelTransactionFullResponse> actionResult) => (actionResult?.Count() ?? 0) > 0,
            });
    }
    #endregion

    #region Put
    [Fact]
    public async Task PutTransactionTest()
    {
        // Arrange
        var repository = GetFuelTransactionRepository();
        var mockServiceItem = new Mock<FuelTransactionService>(repository, Environment, Configuration, Mapper);
        var transactionFromDB = (await repository.GetTransactions(new(), new(), new(), default, 1, 0)).FirstOrDefault();
        transactionFromDB.TransactionID = string.IsNullOrEmpty(transactionFromDB.TransactionID) ? "test" : string.Empty;
        var toUpdate = Mapper.Map<FuelTransactionRequest>(transactionFromDB);
        var checkedTransaction = Mapper.Map<FuelTransaction>(transactionFromDB);
        //
        mockServiceItem.Setup(p => p.CheckIsInstanceExist(checkedTransaction)).Returns(mockServiceItem.Object.CheckIsInstanceExist(checkedTransaction));
        //
        var mockController = new Mock<FuelTransactionsController>(mockServiceItem.Object).Object;

        // Act
        Func<Task<ResponseSingleAction<FuelTransactionShortResponse>>> actionUpdate = async () => await mockController.PutTransaction(toUpdate);
        Func<Task<FuelTransactionFullResponse>> actionCheck = async () => (await mockController.GetTransactions(default))?.FirstOrDefault(transaction => transaction.Id == toUpdate.Id);

        // Assert
        AssertAction<FuelTransactionRequest, FuelTransactionShortResponse, FuelTransactionFullResponse>(toUpdate, actionUpdate, actionCheck);
    }
    #endregion
}
