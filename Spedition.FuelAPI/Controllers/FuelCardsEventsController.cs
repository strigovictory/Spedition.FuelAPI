using Microsoft.AspNetCore.Mvc;
using Spedition.Fuel.BusinessLayer;
using Spedition.Fuel.BusinessLayer.Services;
using Spedition.Fuel.BusinessLayer.Services.Interfaces;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.FuelAPI.Controllers;

/// <summary>
/// Информация по событиям, связанным с топливными картами.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/fuelcardsevents")]
public class FuelCardsEventsController : ControllerBase
{
    private readonly IFuelCardsEventsService fuelCardsEventsService;

    public FuelCardsEventsController(IFuelCardsEventsService fuelCardsEventsService)
        => this.fuelCardsEventsService = fuelCardsEventsService;

    #region Get

    /// <summary>
    /// Метод для получения коллекции событий изменения статуса заданной топливной карты.
    /// </summary>
    /// <returns>Коллекция событий изменения статуса заданной топливной карты.</returns>
    [HttpGet("{cardId}")]
    [ProducesResponseType(typeof(List<FuelCardsEventResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<FuelCardsEventResponse>> GetCardsEvents([FromRoute] int cardId, [FromHeader] CancellationToken token = default)
    {
        return await fuelCardsEventsService?.GetCardsEvents(cardId, token);
    }

    /// <summary>
    /// Метод для получения события, которое предшествует текущему с идентификатором eventId.
    /// </summary>
    /// <param name="eventId">Идентификатор события, которое следует за искомым.</param>
    /// <param name="token">Токен отмены.</param>
    /// <returns> Событие, которое предшествует текущему с идентификатором eventId.</returns>
    [HttpGet("previous/{eventId}")]
    [ProducesResponseType(typeof(FuelCardsEventResponse), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<FuelCardsEventResponse> GetCardsEventPrevious([FromRoute] int eventId, [FromHeader] CancellationToken token = default)
    {
        return await fuelCardsEventsService?.GetCardsEventPrevious(eventId, token);
    }

    /// <summary>
    /// Метод для получения события, которое следует за текущим с идентификатором eventId.
    /// </summary>
    /// <param name="eventId">Идентификатор события, которое предшествовало искомому.</param>
    /// <param name="token">Токен отмены.</param>
    /// <returns> Событие, которое следует за текущим с идентификатором eventId.</returns>
    [HttpGet("next/{eventId}")]
    [ProducesResponseType(typeof(FuelCardsEventResponse), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<FuelCardsEventResponse> GeCardsEventNext([FromRoute] int eventId, [FromHeader] CancellationToken token = default)
    {
        return await fuelCardsEventsService?.GetCardsEventNext(eventId, token);
    }
    #endregion

    #region Put

    /// <summary>
    /// Метод для редактирования события топливной карты.
    /// </summary>
    /// <param name="cardsEvent">Событие топливной карты, отредактированное и подлежащее сохранению в БД.</param>
    /// <param name="token">Токен отмены запроса.</param>
    /// <returns>Событие топливной карты после сохранения в БД вместе со строковым сообщением о результате операции.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(ResponseSingleAction<FuelCardsEventResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ResponseSingleAction<FuelCardsEventResponse>> PutCardsEvent(
        [FromBody] RequestSingleAction<FuelCardsEventRequest> cardsEvent, [FromHeader] CancellationToken token = default)
    {
        var message = string.Empty;
        FuelCardsEventResponse result = new ();

        if (cardsEvent == null)
        {
            message = "Вы пытаетесь обновить пустой контент !";
        }
        else
        {
            result = await fuelCardsEventsService?.UpdateCardsEvent(cardsEvent.Item, cardsEvent.UserName, token);

            if ((result?.Id ?? 0) > 0)
            {
                message += $"Операция успешно завершена. " +
                           $"Событие топливной карты было обновлено. ";
            }
            else
            {
                message += $"Операция закончилась с ошибкой! " +
                           $"Событие топливной карты не было обновлено! ";
            }
        }

        return new ResponseSingleAction<FuelCardsEventResponse>(result, message);
    }

    #endregion

    #region Post

    /// <summary>
    /// Метод для добавления события топливной карты.
    /// </summary>
    /// <param name="cardsEvent">Событие.</param>
    /// <param name="token">Токен отмены запроса.</param>
    /// <returns>Результат операции.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ResponseSingleAction<FuelCardsEventResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ResponseSingleAction<FuelCardsEventResponse>> PostCardsEvent(
    [FromBody] RequestSingleAction<FuelCardsEventRequest> cardsEvent, [FromHeader] CancellationToken token = default)
    {
        var message = string.Empty;
        FuelCardsEventResponse result = new ();

        if (cardsEvent == null)
        {
            message = "Вы пытаетесь обновить пустой контент !";
        }
        else
        {
            result = await fuelCardsEventsService?.CreateCardsEvent(cardsEvent.Item, cardsEvent.UserName, token);

            if ((result?.Id ?? 0) > 0)
            {
                message += $"Операция успешно завершена. " +
                           $"Событие топливной карты было сохранено. ";
            }
            else
            {
                message += $"Операция закончилась с ошибкой! " +
                           $"Событие топливной карты не было сохранено! ";
            }
        }

        return new ResponseSingleAction<FuelCardsEventResponse>(result, message);
    }
    #endregion

    #region Delete

    /// <summary>
    /// Метод для удаления события топливной карты.
    /// </summary>
    /// <param name="cardsEvent">Событие.</param>
    /// <param name="token">Токен отмены запроса.</param>
    /// <returns>Результат операции.</returns>
    [HttpPost("delete")]
    [ProducesResponseType(typeof(ResponseBase), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ResponseBase> DeleteCardsEvent(
    [FromBody] FuelCardsEventRequest cardsEvent, [FromHeader] CancellationToken token = default)
    {
        var result = await fuelCardsEventsService?.DeleteLastCardsEvent(cardsEvent, token);
        var message = fuelCardsEventsService is INotify baseService ? baseService?.NotifyMessage : string.Empty;
        return new ResponseBase(result: message);
    }
    #endregion
}
