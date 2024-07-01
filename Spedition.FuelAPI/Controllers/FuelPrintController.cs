using Microsoft.AspNetCore.Mvc;
using Serilog;
using Spedition.Fuel.BusinessLayer.Services.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.Parsers;
using Spedition.Fuel.BusinessLayer.Services.Print;
using Spedition.Fuel.Client.Helpers;
using Spedition.Fuel.Shared.DTO.RequestModels.Print;
using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;
using Spedition.Fuel.Shared.DTO.ResponseModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.Helpers;
using Spedition.Fuel.Shared.Interfaces;

namespace Spedition.FuelAPI.Controllers;

/// <summary>
/// Точка доступа к генератору печатной формы отчетов.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/fuelprint")]
public class FuelPrintController : ControllerBase
{
    private PrintExcelBase<IPrint> printGenerator;

    private readonly IEnumerable<PrintExcelBase<IPrint>> printGenerators;

    public FuelPrintController(IEnumerable<PrintExcelBase<IPrint>> printGenerators)
    {
        this.printGenerators = printGenerators;
    }

    /// <summary>
    /// Генерирует печатную форму отчета с топливными тр-ми.
    /// </summary>
    /// <param name="transactions">Коллекция тр-ций.</param>
    /// <returns>Массив байт, представляющий собой сгенерированный файл.</returns>
    [HttpPost("transactions")]
    [ProducesResponseType(typeof(byte[]), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<byte[]> PrintTransactions([FromBody] List<TransactionPrintRequest> transactions)
    {
        try
        {
            // Выбрать конкретную реализацию
            printGenerator = ChoicePrintImplementation(UriSegment.fueltransactions);

            if (printGenerator == null || printGenerator == default)
            {
                Log.Warning($"Для топливных транзакций генератор печатной формы отчета не реализован!");
                return default;
            }

            var file =  await printGenerator?.GenerateFile(transactions?.Cast<IPrint>()?.ToList() ?? new());

            if((file?.Length ?? 0) == 0)
            {
                var message = "Ошибка на уровне сервера! Файл не удалось сгенерировать! ";
                Log.Error(nameof(PrintTransactions), GetType().FullName, message);
                return default;
            }
            else
            {
                return file;
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(PrintTransactions), GetType().FullName);
            throw;
        }
    }

    /// <summary>
    /// Генерирует печатную форму отчета с топливными картами.
    /// </summary>
    /// <param name="cards">Коллекция карт.</param>
    /// <returns>Массив байт, представляющий собой сгенерированный файл.</returns>
    [HttpPost("cards")]
    [ProducesResponseType(typeof(byte[]), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<byte[]> PrintCards([FromBody] List<CardPrintRequest> cards)
    {
        try
        {
            // Выбрать конкретную реализацию
            printGenerator = ChoicePrintImplementation(UriSegment.fuelcards);

            if (printGenerator == null || printGenerator == default)
            {
                Log.Warning($"Для топливных карт генератор печатной формы отчета не реализован!");
                return default;
            }

            var file = await printGenerator?.GenerateFile(cards?.Cast<IPrint>()?.ToList() ?? new());

            if ((file?.Length ?? 0) == 0)
            {
                var message = "Ошибка на уровне сервера! Файл не удалось сгенерировать! ";
                Log.Error(nameof(PrintCards), GetType().FullName, message);
                return default;
            }
            else
            {
                return file;
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(PrintCards), GetType().FullName);
            throw;
        }
    }

    /// <summary>
    /// Генерирует печатную форму отчета с необнаруженными номерами топливных карт.
    /// </summary>
    /// <param name="cards">Коллекция необнаруженных номеров топливных карт.</param>
    /// <returns>Массив байт, представляющий собой сгенерированный файл.</returns>
    [HttpPost("notfoundcards")]
    [ProducesResponseType(typeof(byte[]), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<byte[]> PrintNotFoundCards([FromBody] List<CardNotFoundPrintRequest> cards)
    {
        try
        {
            // Выбрать конкретную реализацию
            printGenerator = ChoicePrintImplementation(UriSegment.fuelcardsnotfound);

            if (printGenerator == null || printGenerator == default)
            {
                Log.Warning($"Для топливных карт генератор печатной формы отчета не реализован!");
                return default;
            }

            var file = await printGenerator?.GenerateFile(cards?.Cast<IPrint>()?.ToList() ?? new());

            if ((file?.Length ?? 0) == 0)
            {
                var message = "Ошибка на уровне сервера! Файл не удалось сгенерировать! ";
                Log.Error(nameof(PrintNotFoundCards), GetType().FullName, message);
                return default;
            }
            else
            {
                return file;
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(PrintNotFoundCards), GetType().FullName);
            throw;
        }
    }

    private PrintExcelBase<IPrint> ChoicePrintImplementation(UriSegment uriSegment)
    {
        Func<UriSegment, PrintExcelBase<IPrint>> getPrintGenerator = (UriSegment uriSegment) =>
        printGenerators?.FirstOrDefault(printGeneratorItem => printGeneratorItem?.UriSegment == uriSegment);

        return getPrintGenerator.Invoke(uriSegment);
    }
}
