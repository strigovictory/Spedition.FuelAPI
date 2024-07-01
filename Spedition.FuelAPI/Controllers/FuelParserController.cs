using System.Net;
using Microsoft.AspNetCore.Mvc;
using Spedition.Fuel.BusinessLayer.Services.Interfaces;
using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.Helpers;

namespace Spedition.FuelAPI.Controllers;

/// <summary>
/// Точка доступа к парсеру топливнных транзакций.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/fuelparser")]
public class FuelParserController : ControllerBase
{
    private ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction> parser;

    private readonly IEnumerable<ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction>> parsers;

    public FuelParserController(IEnumerable<ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction>> parsers)
    {
        this.parsers = parsers;
    }

    /// <summary>
    /// Метод для парсинга файла отчета по топливу.
    /// </summary>
    /// <param name="report">отчет ,который сродержит в себе идентификатор поставщика топлива, идентификатор подразделенияб, файл отчета по топливу.</param>
    /// <param name="token">Токен отмены.</param>
    /// <returns>Детализированный результат парсинга.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>> UploadReport(
        [FromBody] FuelReport report, [FromHeader] CancellationToken token = default)
    {
        try
        {
            if (report?.Content == null)
            {
                return new ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>("Ошибка! Файл не был прикреплен!");
            }

            // Выбрать конкретную реализацию интерфейса
            parser = ChoiceParsersImplementation(report?.ProviderId ?? 0);

            if (parser == null || parser == default)
            {
                throw new NotImplementedException($"Для провайдера топлива с идентификатором {report?.ProviderId ?? 0} парсинг не реализован!");
            }

            parser.ProviderId = report.ProviderId;
            parser.IsMonthly = report.IsMonthly;

            await parser?.ParseFile(report, token);

            return new ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>(
                result: $"Транзакции не были загружены в количестве  {parser?.NotSuccessItems?.Count ?? 0} шт. !" + (parser?.NotifyMessage ?? string.Empty),
                successItems: parser?.SuccessItems ?? new (),
                secondarySuccessItems: parser?.SecondarySuccessItems ?? new (),
                notSuccessItems: parser?.NotSuccessItems ?? new ());
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(UploadReport), GetType().FullName);
            throw;
        }
    }

    /// <summary>
    /// Метод для сохранения в БД не сохраненных ранее транзакций, после дозаполнения пользователем недостающих параметров.
    /// </summary>
    /// <param name="transactions">Коллекция транзакций для последующего сохранения в БД.</param>
    /// <returns>Детализированный результат парсинга.</returns>
    [HttpPost("filled")]
    [ProducesResponseType(typeof(ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>> PostFilledTransactions(
        [FromBody] NotParsedTransactionsFilled transactions)
    {
        try
        {
            if ((transactions?.Transactions?.Count ?? 0) == 0)
            {
                return new ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>(
                    "Коллекция пустая! Нет ни одной транзакции, которая м.б. добавлена в БД!");
            }

            // Выбрать конкретную реализацию интерфейса
            parser = ChoiceParsersImplementation(transactions?.ProviderId ?? 0);

            if (parser == null || parser == default)
            {
                throw new NotImplementedException($"Для провайдера топлива с идентификатором {transactions?.ProviderId ?? 0} парсинг не реализован!");
            }

            parser.ProviderId = transactions.ProviderId;
            parser.IsMonthly = transactions.IsMonthly;

            await parser?.SaveFilledTransactions(transactions.Transactions);

            return new ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>(
                result: parser?.NotifyMessage ?? string.Empty,
                successItems: parser?.SuccessItems ?? new(),
                secondarySuccessItems: parser?.SecondarySuccessItems ?? new(),
                notSuccessItems: parser?.NotSuccessItems ?? new());
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(UploadReport), GetType().FullName);
            throw;
        }
    }

    /// <summary>
    /// Выбор конкретной реализации парсера в зависимости от входного параметра - идентификатора поставщика топлива.
    /// </summary>
    /// <param name="providersId">Идентификатор поставщика топлива.</param>
    private ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction> ChoiceParsersImplementation(int providersId)
    {
        Func<int, ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction>> getParser = (int id) =>
        parsers?.FirstOrDefault(parser => parser?.ProvidersId?.Any(provId => provId == id) ?? false);

        return getParser.Invoke(providersId);
    }
}