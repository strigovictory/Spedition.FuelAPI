using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spedition.Fuel.BusinessLayer;
using Spedition.Fuel.BusinessLayer.Services.BaseServices;
using Spedition.Fuel.BusinessLayer.Services.Interfaces;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.Entities;
using Spedition.Fuel.Shared.Enums;
using Spedition.Fuel.Shared.Providers.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Spedition.FuelAPI.Controllers;

/// <summary>
/// Информация по топливным картам.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/fuelcards")]
public class FuelCardsController : ControllerBase
{
    private readonly IFuelCardsService fuelCardsService;

    public FuelCardsController(IFuelCardsService fuelCardsService)
    {
        this.fuelCardsService = fuelCardsService;
    }

    #region Get
    /// <summary>
    /// Метод для получения топливной карты с заданным идентификатором.
    /// </summary>
    /// <param name="cardId">Идентификатор карты.</param>
    /// <param name="token">Токен отмены запроса.</param>
    /// <returns>Топливная карта.</returns>
    [HttpGet("{cardId:int}")]
    [ProducesResponseType(typeof(FuelCardResponse), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<FuelCardResponse> GetCard([FromRoute] int cardId, [FromHeader] CancellationToken token = default)
    {
        return await fuelCardsService.GetCard(cardId, token);
    }

    /// <summary>
    /// Метод для получения коллекции топливных карт.
    /// </summary>
    /// <param name="token">Токен отмены запроса.</param>
    /// <returns>Коллекция топливных карт.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<FuelCardFullResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<FuelCardFullResponse>> GetCards([FromHeader] CancellationToken token = default)
    {
        return await fuelCardsService.GetCards(token);
    }

    /// <summary>
    /// Метод для получения коллекции номеров топливных карт, которые не были найдены в БД.
    /// </summary>
    /// <param name="token">Токен отмены запроса.</param>
    /// <returns>Коллекция номеров топливных карт, которые не были найдены в БД.</returns>
    [HttpGet($"notfoundcards")]
    [ProducesResponseType(typeof(List<FuelCardNotFoundResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<FuelCardNotFoundResponse>> GetCardsNotFound([FromHeader] CancellationToken token = default)
    {
        return await fuelCardsService?.GetNotFoundCards(token);
    }

    /// <summary>
    /// Метод для получения коллекции альтернативных номеров топливных карт.
    /// </summary>
    /// <param name="cardId">Идентификатор топливной карты.</param>
    /// <param name="token">Токен отмены запроса.</param>
    /// <returns>Коллекция альтернативных номеров топливных карт.</returns>
    [HttpGet("alternativenumbers/{cardId:int}")]
    [ProducesResponseType(typeof(List<FuelCardsAlternativeNumberResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<FuelCardsAlternativeNumberResponse>> GetCardsAlternativeNumbers([FromRoute] int cardId, [FromHeader] CancellationToken token = default)
    {
        return await fuelCardsService.GetCardsAlternativeNumbers(cardId, token);
    }
    #endregion

    #region Put
    /// <summary>
    /// Метод для редактирования топливной карты.
    /// </summary>
    /// <param name="card">Топливная карта, отредактированная и подлежащая сохранению в БД.</param>
    /// <param name="token">Токен отмены запроса.</param>
    /// <returns>Детализированный результат операции.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(ResponseSingleAction<FuelCardShortResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ResponseSingleAction<FuelCardShortResponse>> PutCard(
        [FromBody] RequestSingleAction<FuelCardRequest> card, [FromHeader] CancellationToken token = default)
    {
        var message = string.Empty;

        if (card == null)
        {
            message = "Вы пытаетесь обновить пустой контент !";
        }
        else
        {
            await fuelCardsService?.UpdateCard(card.Item, card.UserName, token);

            message = fuelCardsService is INotify baseServ ? baseServ?.NotifyMessage ?? string.Empty : string.Empty;

            if ((fuelCardsService?.SuccessItems?.Count ?? 0) > 0)
            {
                message += $"Операция успешно завершена. " +
                           $"{fuelCardsService?.SuccessItems?.FirstOrDefault()?.ToString() ?? string.Empty} была обновлена. ";
            }
            else
            {
                message += $"Операция закончилась с ошибкой! " +
                           $"{fuelCardsService?.SuccessItems?.FirstOrDefault()?.ToString() ?? string.Empty} не была обновлена!";
            }
        }

        return new ResponseSingleAction<FuelCardShortResponse>(fuelCardsService?.SuccessItems?.FirstOrDefault(), message);
    }

    /// <summary>
    /// Метод для редактирования коллекции топливных карт (напр. групповое перемещение топл.карт в архив).
    /// </summary>
    /// <param name="cards">Коллекция топливных карт, отредактированная и подлежащая сохранению в БД.</param>
    /// <param name="token">Токен отмены запроса.</param>
    /// <returns>Детализированный результат операции.</returns>
    [HttpPut("cards")]
    [ProducesResponseType(typeof(ResponseGroupActionDetailed<FuelCardShortResponse, FuelCardResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ResponseGroupActionDetailed<FuelCardShortResponse, FuelCardResponse>> MoveCardsToArchive(
        [FromBody] RequestGroupAction<FuelCardRequest> cards, [FromHeader] CancellationToken token = default)
    {
        var message = string.Empty;

        if (cards == null || (cards.Items?.Count ?? 0) == 0)
        {
            message = "Вы пытаетесь обновить пустой контент !";
        }
        else
        {
            await fuelCardsService?.MoveCardsToArchive(cards?.Items, cards.UserName, token);

            if ((fuelCardsService?.SuccessItems?.Count ?? 0) == (cards?.Items?.Count ?? 0))
            {
                message = $"Операция успешно завершена ! " +
                          $"Была обновлена вся партия топливных карт, " +
                          $"состоящая из {cards?.Items?.Count ?? 0} шт.! ";
            }
            else if ((fuelCardsService?.SuccessItems?.Count ?? 0) > 0 && (fuelCardsService?.SuccessItems?.Count ?? 0) != (cards?.Items?.Count ?? 0))
            {
                message = $"Операция закончилась с ошибкой ! " +
                          $"Из партии топливных карт, состоящей из {cards?.Items?.Count ?? 0} шт. " +
                          $"было обновлено только {fuelCardsService?.SuccessItems?.Count ?? 0} шт. ! " +
                          $"Оставшиеся {fuelCardsService?.NotSuccessItems?.Count ?? 0} шт. не были обновлены !";
            }
            else
            {
                message = $"Операция закончилась с ошибкой ! " +
                          $"Ни одна из топливных карт в партии, состоящей из {cards?.Items?.Count ?? 0} шт.,! " +
                          $"не была обновлена !";
            }
        }

        return new ResponseGroupActionDetailed<FuelCardShortResponse, FuelCardResponse>(
            result: message,
            successItems: fuelCardsService?.SuccessItems ?? new (),
            notSuccessItems: fuelCardsService?.NotSuccessItems ?? new ());
    }

    /// <summary>
    /// Метод для обновления в БД альтернативного номера топливной карты.
    /// </summary>
    /// <param name="alternativeNumber">Альтернативный номер топливной карты.</param>
    /// <param name="token">Токен отмены запроса.</param>
    /// <returns>Детализированный результат операции.</returns>
    [HttpPut("alternativenumber")]
    [ProducesResponseType(typeof(ResponseSingleAction<FuelCardsAlternativeNumberResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ResponseSingleAction<FuelCardsAlternativeNumberResponse>> PutCardsAlternativeNumber(
        [FromBody] FuelCardsAlternativeNumberRequest alternativeNumber, [FromHeader] CancellationToken token = default)
    {
        var resUpd = await fuelCardsService?.UpdateCardsAlternativeNumber(alternativeNumber, token);
        var message = fuelCardsService is INotify baseServ ? baseServ?.NotifyMessage ?? string.Empty : string.Empty;

        return new ResponseSingleAction<FuelCardsAlternativeNumberResponse>(resUpd, message);
    }
    #endregion

    #region Post

    /// <summary>
    /// Метод для добавления в БД коллекции топливных карт.
    /// </summary>
    /// <param name="cards">Коллекция топливных карт, подлежащая сохранению в БД.</param>
    /// <param name="token">Токен отмены запроса.</param>
    /// <returns>Детализированный результат операции.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ResponseGroupActionDetailed<FuelCardShortResponse, FuelCardResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ResponseGroupActionDetailed<FuelCardShortResponse, FuelCardResponse>> PostCards(
        [FromBody] RequestGroupAction<FuelCardRequest> cards, [FromHeader] CancellationToken token = default)
    {
        var message = string.Empty;
        
        if (cards == null || (cards.Items?.Count ?? 0) == 0)
        {
            message = "Вы пытаетесь сохранить пустой контент !";
        }
        else
        {
            await fuelCardsService?.CreateCards(cards?.Items, cards.UserName, token);

            if ((fuelCardsService?.SuccessItems?.Count ?? 0) == (cards?.Items?.Count ?? 0))
            {
                message = $"Операция успешно завершена ! " +
                          $"В БД была сохранена вся партия топливных карт, " +
                          $"состоящая из {cards?.Items?.Count ?? 0} шт.! ";
            }
            else if ((fuelCardsService?.SuccessItems?.Count ?? 0) > 0 && (fuelCardsService?.SuccessItems?.Count ?? 0) != (cards?.Items?.Count ?? 0))
            {
                message = $"Операция закончилась с ошибкой ! " +
                          $"Из партии топливных карт, состоящей из {cards?.Items?.Count ?? 0} шт. " +
                          $"было сохранено только {fuelCardsService?.SuccessItems?.Count ?? 0} шт. ! " +
                          $"Оставшиеся {fuelCardsService?.NotSuccessItems?.Count ?? 0} шт. не были сохранены !";
            }
            else
            {
                message = $"Операция закончилась с ошибкой ! " +
                          $"Ни одна из топливных карт в партии, состоящей из {cards?.Items?.Count ?? 0} шт.,! " +
                          $"не была сохранена !";
            }
        }

        return new ResponseGroupActionDetailed<FuelCardShortResponse, FuelCardResponse>(
            result: message,
            successItems: fuelCardsService?.SuccessItems ?? new (),
            notSuccessItems: fuelCardsService?.NotSuccessItems ?? new ());
    }

    /// <summary>
    /// Метод для добавления в БД альтернативного номера топливной карты.
    /// </summary>
    /// <param name="alternativeNumber">Альтернативный номер топливной карты.</param>
    /// <param name="token">Токен отмены запроса.</param>
    /// <returns>Детализированный результат операции.</returns>
    [HttpPost("alternativenumber")]
    [ProducesResponseType(typeof(ResponseSingleAction<FuelCardsAlternativeNumberResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ResponseSingleAction<FuelCardsAlternativeNumberResponse>> PostCardsAlternativeNumber(
        [FromBody] FuelCardsAlternativeNumberRequest alternativeNumber, [FromHeader] CancellationToken token = default)
    {
        FuelCardsAlternativeNumberResponse resUpd = null;
        resUpd = await fuelCardsService?.CreateCardsAlternativeNumber(alternativeNumber, token);
        var message = fuelCardsService is INotify baseServ ? baseServ?.NotifyMessage ?? string.Empty : string.Empty;

        return new ResponseSingleAction<FuelCardsAlternativeNumberResponse>(resUpd, message);
    }
    #endregion

    #region Delete

    /// <summary>
    /// Метод для удаления из БД коллекции топливных карт.
    /// </summary>
    /// <param name="cardsIds">Коллекция идентификаторов топливных карт.</param>
    /// <param name="token">Токен отмены.</param>
    /// <returns>Сообщение об успешности операции.</returns>
    [HttpPost("delete")] // в теле HttpDelete-запроса нельзя передавать контент
    [ProducesResponseType(typeof(ResponseBase), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ResponseBase> DeleteCards([FromBody] List<int> cardsIds, [FromHeader] CancellationToken token = default)
    {
        var result = await fuelCardsService?.DeleteCards(cardsIds, token);
        var message = fuelCardsService is INotify baseService ? baseService?.NotifyMessage : string.Empty;
        return new ResponseBase(result: message);
    }

    /// <summary>
    /// Метод для удаления из БД альтернативного номера топливной карты.
    /// </summary>
    /// <param name="alternativeNumbersId">Идентификатор альтернативного номера топливной карты.</param>
    /// <returns>Сообщение об успешности операции.</returns>
    [HttpDelete("alternativenumbers/{alternativeNumbersId}")]
    [ProducesResponseType(typeof(ResponseBase), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ResponseBase DeleteCardsAlternativeNumbers([FromRoute] int alternativeNumbersId)
    {
        var result = fuelCardsService?.DeleteCardsAlternativeNumbers(new List<int> { alternativeNumbersId }) ?? false;
        var message = fuelCardsService is INotify baseService ? baseService?.NotifyMessage : string.Empty;
        return new ResponseBase(result: message);
    }

    /// <summary>
    /// Метод для удаления из БД коллекции не обнаруженных номеров топливных карт.
    /// </summary>
    /// <param name="notFoundCardsIds">Коллекция идентификаторов не обнаруженных номеров топливных карт.</param>
    /// <param name="token">Токен отмены.</param>
    /// <returns>Сообщение об успешности операции.</returns>
    [HttpPost("notfoundcards/delete")] // в теле HttpDelete-запроса нельзя передавать контент
    [ProducesResponseType(typeof(ResponseBase), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ResponseBase DeleteNotFoundCards([FromBody] List<int> notFoundCardsIds, [FromHeader] CancellationToken token = default)
    {
        var result = fuelCardsService?.DeleteNotFoundCards(notFoundCardsIds, token) ?? false;
        var message = fuelCardsService is INotify baseService ? baseService?.NotifyMessage : string.Empty;
        return new ResponseBase(result: message);
    }
    #endregion
}
