using System.Net;
using Microsoft.AspNetCore.Mvc;
using Spedition.Fuel.BusinessLayer;
using Spedition.Fuel.BusinessLayer.Services;
using Spedition.Fuel.BusinessLayer.Services.Interfaces;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.FuelAPI.Controllers;

/// <summary>
/// Информация по топливным транзакциям.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/fueltransactions")]
public class FuelTransactionsController : ControllerBase
{
    private readonly IFuelTransactionService fuelTransactionService;

    public FuelTransactionsController(IFuelTransactionService fuelTransactionService)
        => this.fuelTransactionService = fuelTransactionService;

    /// <summary>
    /// Метод для получения общего кол-ва топливных транзакций.
    /// </summary>
    /// <param name="token">Токен отмены.</param>
    /// <returns>Кол-во топливных транзакций.</returns>
    [HttpGet("count")]
    [ProducesResponseType(typeof(long), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public long GetCount([FromHeader] CancellationToken token = default)
    {
        return fuelTransactionService.GetCount(token);
    }

    /// <summary>
    /// Метод для получения топливной транзакции с заданным идентификатором.
    /// </summary>
    /// <param name="transactionId">Идентификатор тр-ции.</param>
    /// <param name="token">Токен отмены.</param>
    /// <returns>Отпагинированная коллекция топливных транзакций.</returns>
    [HttpGet("{transactionId:int}")]
    [ProducesResponseType(typeof(FuelTransactionResponse), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<FuelTransactionResponse> GetTransaction([FromRoute] int transactionId, [FromHeader] CancellationToken token = default)
    {
        return await fuelTransactionService?.GetTransaction(transactionId, token);
    }

    /// <summary>
    /// Метод для получения отпагинированной коллекции топливных транзакций.
    /// </summary>
    /// <param name="token">Токен отмены.</param>
    /// <param name="toTake">Колличество экземпляров, возвращаемое методом.</param>
    /// <param name="toSkip">Колличество экземпляров, пропускаемое методом.</param>
    /// <returns>Отпагинированная коллекция топливных транзакций.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<FuelTransactionFullResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<FuelTransactionFullResponse>> GetTransactions([FromHeader] CancellationToken token = default, [FromQuery] int? toTake = null, [FromQuery] int? toSkip = null)
    {
        return await fuelTransactionService?.GetTransactions(token, toTake, toSkip);
    }

    /// <summary>
    /// Метод для редактирования топливной транзакции.
    /// </summary>
    /// <param name="transaction">Топливная транзакция, отредактированная и подлежащая сохранению в БД.</param>
    /// <returns>Топливная карта после сохранения в БД вместе со строковым сообщением о результате операции.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(ResponseSingleAction<FuelTransactionShortResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ResponseSingleAction<FuelTransactionShortResponse>> PutTransaction(
        [FromBody] FuelTransactionRequest transaction)
    {
        var message = string.Empty;
        FuelTransactionShortResponse result = new ();

        if (transaction == null)
        {
            message = "Вы пытаетесь обновить пустой контент !";
        }
        else
        {
            result = await fuelTransactionService?.UpdateTransaction(transaction);

            message = fuelTransactionService is INotify baseServ ? baseServ?.NotifyMessage ?? string.Empty : string.Empty;

            if ((result?.Id ?? 0) > 0)
            {
                message += $"Операция успешно завершена. " +
                           $"Транзакция {result.ToString() ?? string.Empty} была обновлена. ";
            }
            else
            {
                message += $"Операция закончилась с ошибкой! " +
                           $"Транзакция не была обновлена!";
            }
        }

        return new ResponseSingleAction<FuelTransactionShortResponse>(result, message);
    }

    /// <summary>
    /// Метод для добавления в БД коллекции топливных транзакций.
    /// </summary>
    /// <param name="transactions">Коллекция топливных транзакций, подлежащая сохранению в БД.</param>
    /// <param name="token">Токен отмены запроса.</param>
    /// <returns>Коллекция топливных транзакций, сохраненных в БД, коллекция топливных транзакций, не сохраненных в БД с обьяснением причины этого,
    /// а такжее строковое сообщение о результате операции.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ResponseGroupActionDetailed<FuelTransactionShortResponse, FuelTransactionShortResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ResponseGroupActionDetailed<FuelTransactionShortResponse, FuelTransactionShortResponse>> PostTransactions(
        [FromBody] IEnumerable<FuelTransactionRequest> transactions, [FromHeader] CancellationToken token = default)
    {
        var message = string.Empty;

        if ((transactions?.Count() ?? 0) == 0)
        {
            message = "Вы пытаетесь сохранить пустой контент !";
        }
        else
        {
            await fuelTransactionService?.CreateTransactions(transactions.ToList());

            if ((fuelTransactionService?.SuccessItems?.Count ?? 0) == (transactions?.Count() ?? 0))
            {
                message = $"Операция успешно завершена ! " +
                          $"В БД была сохранена вся партия топливных транзакций, " +
                          $"состоящая из {transactions?.Count() ?? 0} шт.! ";
            }
            else if ((fuelTransactionService?.SuccessItems?.Count ?? 0) > 0 && (fuelTransactionService?.SuccessItems?.Count ?? 0) != (transactions?.Count() ?? 0))
            {
                message = $"Операция закончилась с ошибкой ! " +
                          $"Из партии топливных транзакций, состоящей из {transactions?.Count() ?? 0} шт. " +
                          $"было сохранено только {fuelTransactionService?.SuccessItems?.Count ?? 0} шт. ! " +
                          $"Оставшиеся {fuelTransactionService?.NotSuccessItems?.Count ?? 0} шт. не были сохранены !";
            }
            else
            {
                message = $"Операция закончилась с ошибкой ! " +
                          $"Ни одна из топливных транзакций в партии, состоящей из {transactions?.Count() ?? 0} шт.,! " +
                          $"не была сохранена !";
            }
        }

        return new ResponseGroupActionDetailed<FuelTransactionShortResponse, FuelTransactionShortResponse>(
            result: message,
            successItems: fuelTransactionService?.SuccessItems ?? new (),
            notSuccessItems: fuelTransactionService?.NotSuccessItems ?? new ());
    }

    /// <summary>
    /// Метод для удаления из БД коллекции топливных транзакций.
    /// </summary>
    /// <param name="transactionsIds">Коллекция идентификаторов топливных транзакций.</param>
    /// <returns>Сообщение об успешности операции.</returns>
    [HttpPost("delete")] // в теле HttpDelete-запроса нельзя передавать контент
    [ProducesResponseType(typeof(ResponseBase), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ResponseBase DeleteTransactions([FromBody] List<int> transactionsIds)
    {
        var result = fuelTransactionService?.DeleteTransactions(transactionsIds);
        var message = fuelTransactionService is INotify baseService ? baseService?.NotifyMessage : string.Empty;
        return new ResponseBase(result: message);
    }

    /// <summary>
    /// Метод для удаления из БД топливной транзакции.
    /// </summary>
    /// <param name="transactionsId">Идентификатор топливной транзакции.</param>
    /// <returns>Сообщение об успешности операции.</returns>
    [HttpDelete("{transactionsId}")]
    [ProducesResponseType(typeof(ResponseBase), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ResponseBase DeleteTransaction([FromRoute] int transactionsId)
    {
        var result = fuelTransactionService?.DeleteTransactions(new List<int> { transactionsId }) ?? false;
        var message = fuelTransactionService is INotify baseService ? baseService?.NotifyMessage : string.Empty;
        return new ResponseBase(result: message);
    }

    /// <summary>
    /// Метод для удаления задвоенных топливных транзакций.
    /// </summary>
    /// <param name="token">Токен отмены.</param>
    /// <param name="fuelProviderId">Идентификатор поставщика топлива.</param>
    /// <param name="month">Месяц.</param>
    /// <param name="year">Год.</param>
    /// <returns>Сообщение об успешности операции.</returns>
    [HttpDelete("duplicates")]
    [ProducesResponseType(typeof(ResponseBase), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ResponseBase> DeleteTransactionsDuplicates(
        [FromQuery] int? fuelProviderId, [FromQuery] int? month = null, [FromQuery] int? year = null, [FromHeader] CancellationToken token = default)
    {
        var result = await fuelTransactionService?.DeleteTransactionsDuplicates(fuelProviderId, month, year, token);
        var message = fuelTransactionService is INotify baseService ? baseService?.NotifyMessage : string.Empty;
        return new ResponseBase(result: message);
    }
}
