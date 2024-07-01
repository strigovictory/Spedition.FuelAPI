using Moq;
using NPOI.SS.Formula.Functions;
using Spedition.Fuel.DataAccess;
using Spedition.Fuel.Shared.DTO.ResponseModels;
using Spedition.Fuel.Shared.Entities;
using System;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.Repositories;

public class RepositoryTests : AssertHelper
{
    public RepositoryTests(ITestOutputHelper output) : base(output)
    {
    }

    private static int cardId = 5;
    private int take = 5;
    private int? skip = 1;

    #region Filters
    [Fact]
    public async void FindFirstTest()
    {
        // Arrange
        var predicate = (FuelCard card) => card.Id == cardId;

        // Action
        var actionResult = GetFuelRepositories<FuelCard>()?.Cards?.FindFirst(card => predicate(card));

        // Assert
        Assert.NotNull(actionResult);

        Output.WriteLine($"Find item: {actionResult.ToString()}");
        Output.WriteLine(actionResult?.ToString() ?? string.Empty);

        Assert.Equal(actionResult?.Id ?? 0, cardId);
    }

    [Fact]
    public async void FindFirstAsyncTest()
    {
        // Action
        var actionResult = await GetFuelRepositories<FuelCard>()?.Cards?.FindFirstAsync((card) => card.Id == cardId, default);

        // Assert
        Assert.NotNull(actionResult);

        Output.WriteLine($"Find item: {actionResult.ToString()}");
        Output.WriteLine(actionResult?.ToString() ?? string.Empty);

        Assert.Equal(actionResult?.Id ?? 0, cardId);
    }

    [Fact]
    public async void FindRangeTest()
    {
        // Action
        var actionResult = () => GetFuelRepositories<FuelCard>()?.Cards?.FindRange((card) => card.Id == cardId);

        // Assert
        AssertAction(actionResult, (card) => card.Id);
    }

    [Fact]
    public async void FindRangeAsyncTest()
    {
        // Action
        var actionResult = async () => await GetFuelRepositories<FuelCard>()?.Cards?.FindRangeAsync((card) => card.Id == cardId, default);

        // Assert
        AssertAction(actionResult, (card) => card.Id);
    }

    [Fact]
    public async void AnyAsyncTest()
    {
        // Action
        var actionResult = await GetFuelRepositories<FuelCard>()?.Cards?.AnyAsync((card) => card.Id == cardId, default);

        // Assert
        Assert.NotNull(actionResult);
        Assert.True(actionResult);
        Output.WriteLine($"Action result: {actionResult}");
    }

    [Fact]
    public void AnyTest()
    {
        // Action
        var actionResult = GetFuelRepositories<FuelCard>()?.Cards?.Any((card) => card.Id == cardId);

        // Assert
        Assert.NotNull(actionResult);
        Assert.True(actionResult);
        Output.WriteLine($"Action result: {actionResult}");
    }
    #endregion

    #region Get
    [Fact]
    public void CountItemsTest()
    {
        // Action
        var actionResult = GetFuelRepositories<FuelCard>()?.Cards?.CountItems;

        // Assert
        Assert.NotNull(actionResult);
        Assert.True(actionResult > 0);
        Output.WriteLine($"Cards count: {actionResult?.ToString()}");
    }

    [Fact]
    public void GetTest()
    {
        // Action
        var actionResult = () => GetFuelRepositories<FuelCard>()?.Cards?.Get();

        // Assert
        AssertAction(actionResult);
    }

    [Fact]
    public async void GetAsyncTest()
    {
        // Action
        var actionResult = async () => await GetFuelRepositories<FuelCard>()?.Cards?.GetAsync(default);

        // Assert
        AssertAction(actionResult);
    }

    [Fact]
    public async void GetPaginatedTest()
    {
        // Action
        var actionResult = async () => await GetFuelRepositories<FuelCard>()?.Cards?.GetAsync(take, skip, default);

        // Assert
        AssertAction(actionResult, (cards) => cards.Count == take);
    }

    [Fact]
    public void GetIQueryableTest()
    {
        var actionResult = () => GetFuelRepositories<FuelCard>()?.Cards?.GetIQueryable()?.ToList();

        // Assert
        AssertAction(actionResult, (cards) => cards.Count > 0);
    }

    [Fact]
    public async void GetByIdTest()
    {
        // Action
        var actionResult = await GetFuelRepositories<FuelCard>()?.Cards?.GetAsync(cardId, default);

        // Assert
        Assert.NotNull(actionResult);
        Assert.True((actionResult?.Id ?? 0) == cardId);

        Output.WriteLine($"GetDivisions item: {actionResult.ToString()}");
        Output.WriteLine(actionResult?.ToString() ?? string.Empty);
    }

    [Fact]
    public async void GetByIdParamsTest()
    {
        // Action
        var actionResult = await GetFuelRepositories<FuelCard>()?.Cards?.GetAsync(token: default, keyValues: cardId);

        // Assert
        Assert.NotNull(actionResult);
        Assert.True((actionResult?.Id ?? 0) == cardId);

        Output.WriteLine($"GetDivisions item: {actionResult.ToString()}");
        Output.WriteLine(actionResult?.ToString() ?? string.Empty);
    }

    #endregion
}
