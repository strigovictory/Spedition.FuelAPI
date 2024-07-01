using System.Collections.Concurrent;
using System.Net;
using System.Transactions;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Spedition.Fuel.BFF.Constants;
using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;
using Spedition.Fuel.Shared.DTO.ResponseModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Spedition.Fuel.BusinessLayer.Services.Parsers.ParserBaseServices;

public abstract class ParserBase<TParsed, TSuccess, TNotSuccess, TSearch>
    : ParserOuterLibBase<TParsed, TSuccess, TNotSuccess, TSearch>
    where TParsed : class, IParsedItem
{
    public ParserBase(
        IWebHostEnvironment env,
        FuelRepositories fuelRepositories,
        IConfiguration configuration,
        IMapper mapper,
        OuterLibrary outerLibrary)
        : base(env, fuelRepositories, configuration, mapper, outerLibrary)
    {
    }

    public override async Task ParseFile(UploadedContent content, CancellationToken token = default)
    {
        UserName = content?.UserName ?? string.Empty;

        var uploadResult = UploadFile(content as UploadedContent, token);

        if(uploadResult)
            await ParseFile();
    }

    protected virtual async Task ParseFile()
    {
        try
        {
            NotifyMessage = string.Empty;
            ItemsToMapping = new();
            ItemsToSaveInDB = new();
            NewItemsToAddInDB = new();
            ExistedInstances = new();

            // 1 - Проверка файла на соответствие заданному формату
            var isFilesFormatRequired = CheckFileExtension();
            if (!isFilesFormatRequired)
            {
                var formats = string.Empty;
                FilesFormatsRequired?.ForEach(_ => formats += _ + "; ");
                NotifyMessage += $"Формат отчета не поддерживается. Файл должен иметь формат: {formats} !";
                return;
            }

            // 2 - Парсинг файла
            ChoiceOuterLib();
            ProcessingFile();

            if ((ItemsToMapping?.Count ?? 0) == 0)
            {
                NotifyMessage += "Файл не удалось обработать, возможно, были неверно заданы параметры, влияющие на парсинг отчета!";
                return;
            }

            // 3 - Маппинг моделей отчета в ДБ-модели
            await MappingParsedToDB();

            if ((ItemsToSaveInDB?.Count ?? 0) == 0)
            {
                NotifyMessage += $"После обработки очета подлежало преобразованию {ItemsToMapping?.Count ?? 0} шт. транзакций, " +
                           $"но ни одна из них не была сохранена в БД ! ";
                return;
            }

            // 4 - Сохранение измененных / новых экземпляров в БД
            await SaveItemsChanges();
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(ParseFile), GetType().FullName);
            throw;
        }
        finally
        {
            DeleteFiles();
        }
    }

    /// <summary>
    /// Метод для проверки принадлежности файла к одному из поддерживаемых парсером форматов.
    /// </summary>
    /// <returns>Истина, если парсер поддерживает заданный формат.</returns>
    protected bool CheckFileExtensionInAllowedRange()
    {
        string extension = Path.GetExtension(FilePathSource ?? string.Empty) ?? string.Empty;

        var isAllowed = FilesFormatsRequired.Any(_ => extension.ToLower().Equals(_.ToLower()));

        if (isAllowed)
        {
            FilePathDestination = FilePathSource;
        }

        return isAllowed;
    }

    /// <summary>
    /// Метод для проверки расширения файла на допустимость парсинга данным сервисом.
    /// </summary>
    /// <returns>Результат проверки соответствия пути расширения файла требуемым условиям.</returns>
    protected bool CheckFileExtension()
    {
        string extension = Path.GetExtension(Path.GetFullPath(FilePathSource ?? string.Empty)) ?? string.Empty;

        if (!FilesFormatsRequired.Any(_ => extension.ToLower().Equals(_.ToLower())))
        {
            return false;
        }
        else
        {
            FilePathDestination = FilePathSource;
            return true;
        }
    }

    /// <summary>
    /// Метод форматирует дату.
    /// </summary>
    /// <returns>Отформатированная строка на t-sql для даты.</returns>
    protected string DateTimeFormat(DateTime date)
    {
        var formattingDate = date.FormatDateTime();
        return $"CONVERT(datetime, '{formattingDate}')";
    }
}
