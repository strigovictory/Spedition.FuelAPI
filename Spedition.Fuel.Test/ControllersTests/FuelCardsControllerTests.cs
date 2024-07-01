using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Spedition.Fuel.BusinessLayer.Services;
using Spedition.Fuel.BusinessLayer.Services.ProvidersApi;
using Spedition.Fuel.DataAccess.Infrastructure.Repositories;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.Entities;
using Spedition.Fuel.Shared.Settings;
using Spedition.Fuel.Test.ServicesTests;
using Spedition.FuelAPI.Controllers;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.ControllersTests;

public class FuelCardsControllerTests : CardsServiceTests
{
    public FuelCardsControllerTests(ITestOutputHelper output) : base(output)
    {
    }

    private FuelCardsController GetFuelCardsController()
    {
        var repository = GetFuelCardRepository();

        var serviceItem = new Mock<FuelCardsService>(repository, Environment, Configuration, Mapper);

        return (new Mock<FuelCardsController>(serviceItem.Object)).Object;
    }

    #region Get
    [Fact]
    public async void GetCardsControllerTest()
    {       
        // Arrange
        var controller = GetFuelCardsController();

        // Act
        Func<Task<List<FuelCardFullResponse>>> action = async () => await controller.GetCards(default);

        // Assert
        AssertAction<FuelCardFullResponse>(action, (FuelCardFullResponse card) => card.Id);
    }

    [Fact]
    public async void GetCardsNotFoundTest()
    {
        // Arrange
        var controller = GetFuelCardsController();

        // Act
        Func<Task<List<FuelCardNotFoundResponse>>> action = async () => await controller.GetCardsNotFound(default);

        // Assert
        AssertAction<FuelCardNotFoundResponse>(action, (FuelCardNotFoundResponse card) => card.Id);
    }

    [Fact]
    public void GetCardsAlternativeNumbersTest()
    {
        // Arrange
        int cardId = default;

        Func<Task<List<FuelCardsAlternativeNumberResponse>>> action = async () => await GetFuelCardsController().GetCardsAlternativeNumbers(cardId, default);

        // Act
        (int empty, int notExist, int existed) cardsId = (0, 1, 6478);

        // Assert 1 - empty
        cardId = cardsId.empty;
        AssertAction<FuelCardsAlternativeNumberResponse>(
            action,
            new Predicate<List<FuelCardsAlternativeNumberResponse>>[]
            {
                (List<FuelCardsAlternativeNumberResponse> actionResult) => (actionResult?.Count() ?? 0) == 0,
            });

        // Assert 2 - not existed
        cardId = cardsId.notExist;
        AssertAction<FuelCardsAlternativeNumberResponse>(
            action,
            new Predicate<List<FuelCardsAlternativeNumberResponse>>[]
            {
                (List<FuelCardsAlternativeNumberResponse> actionResult) => (actionResult?.Count() ?? 0) == 0,
            });

        // Assert 3 - existed
        cardId = cardsId.existed;
        AssertAction<FuelCardsAlternativeNumberResponse>(
            action,
            new Predicate<List<FuelCardsAlternativeNumberResponse>>[]
            {
                (List<FuelCardsAlternativeNumberResponse> actionResult) => (actionResult?.Count() ?? 0) > 0,
                (List<FuelCardsAlternativeNumberResponse> actionResult) => actionResult?.All(card => card.Id > 0) ?? false,
            });
    }
    #endregion

    #region Put
    [Fact]
    public async void PutCardTest()
    {
        // Arrange
        var controller = GetFuelCardsController();

        var itemFromDB = (await controller.GetCards(default))?.FirstOrDefault();
        itemFromDB.Note = string.IsNullOrEmpty(itemFromDB.Note) ? "test" : null;
        var itemToUpdate = new RequestSingleAction<FuelCardRequest>("testUser", Mapper.Map<FuelCardRequest>(itemFromDB));

        // Act
        Func<Task<ResponseSingleAction<FuelCardShortResponse>>> updateAction = async () => await controller.PutCard(itemToUpdate, default);
        Func<Task<FuelCardFullResponse>> checkAction = async () => (await controller.GetCards(default))?.FirstOrDefault(card => card.Id == itemToUpdate.Item.Id);

        // Assert
        AssertAction<FuelCardRequest, FuelCardShortResponse, FuelCardFullResponse>(itemToUpdate, updateAction, checkAction);
    }

    [Fact]
    public async void PutCardsTest()
    {
        // Arrange
        var controller = GetFuelCardsController();

        var itemsFromDB = (await controller.GetCards(default))?.Take(10)?.ToList() ?? new();
        itemsFromDB?.ForEach(card => card.Note = string.IsNullOrEmpty(card.Note) ? "test" : null);
        var itemsToUpdate = new RequestGroupAction<FuelCardRequest>("testUser", itemsFromDB?.Select(card => Mapper.Map<FuelCardRequest>(card))?.ToList());

        // Act
        Func<Task<ResponseGroupActionDetailed<FuelCardShortResponse, FuelCardResponse>>> updateAction = async () 
            => await controller.MoveCardsToArchive(itemsToUpdate, default);

        Func<FuelCardRequest, Task<FuelCardResponse>> checkAction = async (FuelCardRequest card) => 
        (await controller?.GetCard(card.Id));

        // Assert
        AssertAction<FuelCardRequest, FuelCardShortResponse, FuelCardResponse>(itemsToUpdate, updateAction, checkAction);
    }

    [Fact]
    public async void PutCardsAlternativeNumberTest()
    {
        // Arrange
        var controller = GetFuelCardsController();
        var itemFromDB = (await GetFuelRepositories()?.CardsAlternativeNumbers?.GetAsync(default))?.FirstOrDefault();
        itemFromDB.Number = itemFromDB.Number.Contains("test")
            ? itemFromDB.Number.Substring(0, itemFromDB.Number.Length - 4)
            : itemFromDB.Number + "test";
        var itemToUpdate = Mapper.Map<FuelCardsAlternativeNumberRequest>(itemFromDB);

        // Act
        Func<Task<ResponseSingleAction<FuelCardsAlternativeNumberResponse>>> updateAction = async () => await controller?.PutCardsAlternativeNumber(itemToUpdate, default);
        Func<Task<FuelCardsAlternativeNumber>> checkAction = async () => (await GetFuelRepositories()?.CardsAlternativeNumbers?.GetAsync(default))?.FirstOrDefault(number => number.Id == itemFromDB.Id);

        // Assert
        AssertAction<FuelCardsAlternativeNumberRequest, FuelCardsAlternativeNumberResponse, FuelCardsAlternativeNumber>(itemToUpdate, updateAction, checkAction);
    }
    #endregion

    #region Post
    [Fact]
    public async void PostCardsTest()
    {
        // Arrange
        var itemsToCreate = new RequestGroupAction<FuelCardRequest>(
            "testUser",
            new List<FuelCardRequest>
            {
                new FuelCardRequest
                {
                    Number = "test1",
                    ExpirationDate = null,
                    ReceiveDate = null,
                    IssueDate = null,
                    IsReserved = false,
                    Note = "test1",
                    IsArchived = false,
                    CarId = 122,
                    DivisionID = 1,
                    ProviderId = 7,
                },
                new FuelCardRequest
                {
                    Number = "test2",
                    ExpirationDate = null,
                    ReceiveDate = null,
                    IssueDate = null,
                    IsReserved = false,
                    Note = "test2",
                    IsArchived = false,
                    CarId = 122,
                    DivisionID = 1,
                    ProviderId = 7,
                },
                new FuelCardRequest
                {
                    Number = "test2",
                    ExpirationDate = null,
                    ReceiveDate = null,
                    IssueDate = null,
                    IsReserved = false,
                    Note = "test2",
                    IsArchived = false,
                    CarId = 122,
                    DivisionID = 1,
                    ProviderId = 7,
                },
            });

        // Mock не подходит, не видит метод CheckIsInstanceExist
        var repository = GetFuelCardRepository();
        var serviceItem = new FuelCardsService(repository, Environment, Configuration, Mapper, null, null, null, null);
        var controller = new FuelCardsController(serviceItem);

        // Act
        Func<Task<ResponseGroupActionDetailed<FuelCardShortResponse, FuelCardResponse>>> createAction = async () => await controller?.PostCards(itemsToCreate, default);
        Func<FuelCardShortResponse, Task<FuelCardResponse>> checkAction = async (FuelCardShortResponse card) 
            => (await controller?.GetCard(card.Id));

        // Assert
        AssertAction<FuelCardRequest, FuelCardShortResponse, FuelCardResponse>(itemsToCreate, createAction, checkAction);
    }
    #endregion
}