using System.Reflection;
using AutoMapper.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.BusinessLayer.Enums.ReportsHeaders;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.Parsers.ParserBaseServices;
using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.Fuel.BusinessLayer.Services.Parsers;

public class ParserGazProm : FuelParserBase<FuelTransactionGazProm, FuelTransactionShortResponse, NotParsedTransaction, FuelTransaction>
{
    public ParserGazProm(
    IWebHostEnvironment env,
    FuelRepositories fuelRepositories,
    IConfiguration configuration,
    IMapper mapper,
    ICountryService countryService,
    ICurrencyService currencyService,
    IEventsTypeService eventsTypeService,
    IDivisionService divisionService,
    ITruckService carService)
    : base(env, fuelRepositories, configuration, mapper, countryService, currencyService, eventsTypeService, divisionService, carService, OuterLibrary.EPPlus)
    {
        ProvidersId = new List<int> { 9 };
    }

    protected enum ReportKind_GazPromBY
    {
        Вариант_1 = 1, // Вариант БЕЗ промежуточных группировок внутри таблицы
        Вариант_2, // Вариант С промежуточными группировками по разновидности топлива и номеру карты внутри таблицы
        Вариант_3, // Вариант с конца 2022 года
        НеПодлежащийПарсингуВариант // на случай, когда книга содержит лист, который не подходит ни под один вариант отчета
    }

    // Разновидность отчета
    private ReportKind_GazPromBY ReportInnerKind { get; set; } = ReportKind_GazPromBY.НеПодлежащийПарсингуВариант;

    protected override Dictionary<PropertyInfo, string> ComparisonPropertyToColumnsName =>
        ReportsItemsNameAttrProps?.ToDictionary(prop => prop, prop =>
        ReportInnerKind switch
        {
            ReportKind_GazPromBY.Вариант_1 => typeof(FuelTransactionGazProm).GetNameAttributeValues(prop.Name).firstName,
            ReportKind_GazPromBY.Вариант_2 => typeof(FuelTransactionGazProm).GetNameAttributeValues(prop.Name).secondName,
            ReportKind_GazPromBY.Вариант_3 => typeof(FuelTransactionGazProm).GetNameAttributeValues(prop.Name).thirdName,
            ReportKind_GazPromBY.НеПодлежащийПарсингуВариант => string.Empty,
            _ => string.Empty,
        });

    private int AllowedEmptyCellsInRow { get; set; }

    protected override void ProcessingFile()
    {
        ItemsToMapping = new();
        try
        {
            using FileStream fs = new(FilePathDestination, FileMode.Open, FileAccess.Read);
            using var package = new ExcelPackage(fs);

            using var workbook = package?.Workbook;
            using var worksheets = package?.Workbook?.Worksheets;

            // Число страниц в книге excel
            NumberWorksheets = worksheets?.Count ?? 0;

            if (NumberWorksheets == 0)
            {
                NotifyMessage += "Отчет пустой, не содержит ни одного листа!";
                NotifyMessage.LogError(GetType().Name, nameof(ProcessingFile));
                return;
            }

            // Перебор листов книги
            for (var worksheetInd = 0; worksheetInd < NumberWorksheets; worksheetInd++)
            {
                ComparisonPropertyToColumnsNumber = new();

                #region 1 - Поиск номера строки, содежащей заголовок таблицы

                // Текущая страница
                var worksheet = worksheets[worksheetInd];

                //Число строк на текущем листе книги
                var dimensionWorkSheetRows = worksheet?.Dimension?.Rows ?? 0;

                //Число столбцов на текущем листе книги
                var dimensionWorkSheetColumns = worksheet?.Dimension?.Columns ?? 0;

                // Номер строки, содержащий искомую шапку таблицы
                var numRowTableHeader = 0;

                // Левая граница диапазона, содержащего шапку таблицы
                var leftCellTableHeader = 0;

                // Правая граница диапазона, содержащего шапку таблицы
                var righCellTableHeader = 0;

                // Перебор первых 25-ти  строк либо если диапазон строк меньше -  в пределах диапазона (предположительно, что шапка таблицы располагается в этом диапазоне)
                var numRowsForSearch = dimensionWorkSheetRows <= 25 ? dimensionWorkSheetRows : 25;

                // Счетчик числа выполненных при поиске шапки таблицы условий
                var numCondition = 0;

                // Перебор всех строк
                for (int i = 1; i <= numRowsForSearch; i++)
                {
                    numCondition = 0;

                    // Если значение найдено - остановить поиск и выйти из внешнего цикла
                    if (numRowTableHeader > 0)
                    {
                        break;
                    }

                    // Перебор всех столбцов
                    for (int j = 1; j <= dimensionWorkSheetColumns; j++)
                    {
                        var data = worksheet?.Cells[i, j]?.Value?.ToString()?.Trim() ?? string.Empty;

                        if (!string.IsNullOrEmpty(data))
                        {
                            // Поиск заданных строковых значений в шапке таблицы на листе
                            if (ComparisonPropertyToColumnsName.Any(prop => data.Contains(prop.Value)) 
                                || ComparisonPropertyToColumnsName.Any(prop => prop.Value.Contains(data)))
                            {
                                numCondition++;

                                // если в строке найдены ВСЕ обязательно присутствующие в шапке таблицы значения -
                                // остановить поиск строки в которой находится заголовок таблицы и выйти из вложенного цикла
                                if (numCondition == ComparisonPropertyToColumnsName.Count)
                                {
                                    numRowTableHeader = i;
                                    break;
                                }
                            }
                        }
                    }
                }

                // Если шапка таблицы не была обнаружена - перейти к след.странице, если таковая имеется
                if (numRowTableHeader == 0)
                {
                    continue; // Перейти к след.странице, если таковая имеется
                }

                // Определение левой и правой границ шапки таблицы, содержащей значения
                for (int i = 1; i <= dimensionWorkSheetColumns; i++)
                {
                    if (string.IsNullOrEmpty(worksheet?.Cells[numRowTableHeader, i]?.Value?.ToString()))
                        continue; // Перейти к след.колонке, если ячейка пустая

                    leftCellTableHeader = i;
                    var countEmptyCells = 0;

                    // Определение правой границы шапки таблицы, содержащей значения
                    for (int j = leftCellTableHeader + 1; j <= dimensionWorkSheetColumns; j++)
                    {
                        if (countEmptyCells >= dimensionWorkSheetColumns) 
                            break;

                        if (string.IsNullOrEmpty(worksheet?.Cells[numRowTableHeader, j]?.Value?.ToString()))
                        {
                            countEmptyCells++;
                            righCellTableHeader = j - 1;
                        }

                        if (j == dimensionWorkSheetColumns && !string.IsNullOrEmpty(worksheet?.Cells[numRowTableHeader, j]?.Value?.ToString()))
                        {
                            righCellTableHeader = j;
                        }
                        else
                        {
                            righCellTableHeader = j - 1;
                        }
                    }

                    // Если не найдена пустая ячейка справа, присвоить значение правой границы диапазона значения
                    righCellTableHeader = righCellTableHeader == 0 ? dimensionWorkSheetColumns : righCellTableHeader;
                    break;
                }
                #endregion

                #region 2 - Определение разновидности отчета

                // По первой странице можно определить разновидность отчета
                if (worksheetInd == 0)
                {
                    var cellsRange = worksheet?.Cells[numRowTableHeader, leftCellTableHeader, numRowTableHeader, righCellTableHeader] ?? null;
                    if (cellsRange == null) continue; // Перейти к след.странице, если таковая имеется

                    if (cellsRange.Any(_ => _.Value?.ToString()?.Trim()?.Contains("Валюта" ?? string.Empty, StringComparison.InvariantCultureIgnoreCase) ?? false))
                    {
                        // Поле «Валюта» в шапке таблицы содержит только третий вариант отчета
                        ReportInnerKind = ReportKind_GazPromBY.Вариант_3;
                    }
                    else
                    {
                        if (cellsRange.Any(_ => _.Value?.ToString()?.Trim()?.Contains("Эмитент" ?? string.Empty, StringComparison.InvariantCultureIgnoreCase) ?? false))
                        {
                            // Поле «Эмитент» в шапке таблицы содержит только второй вариант отчета (с промежуточными группировками по разновидности топлива и номеру карты внутри таблицы)
                            ReportInnerKind = ReportKind_GazPromBY.Вариант_2;
                        }
                        else if (cellsRange.Any(_ => _.Value?.ToString()?.Trim()?.Contains("Держатель" ?? string.Empty, StringComparison.InvariantCultureIgnoreCase) ?? false))
                        {
                            ReportInnerKind = ReportKind_GazPromBY.Вариант_1;
                        }
                    }

                    AllowedEmptyCellsInRow =
                        ReportInnerKind == ReportKind_GazPromBY.Вариант_1 ? 2 :
                        ReportInnerKind == ReportKind_GazPromBY.Вариант_2 ? 1 :
                        ReportInnerKind == ReportKind_GazPromBY.Вариант_3 ? 14 : 0;
                }
                #endregion

                #region 3 - Заполнение коллекции,сопоставляющей свойства - номера столбцов
                foreach (var propinfo in ComparisonPropertyToColumnsName ?? new())
                {
                    // Цикл перебора ячеек шапки таблицы
                    for (int i = leftCellTableHeader; i <= righCellTableHeader; i++)
                    {
                        // Текущее знаение проверемой ячейки
                        var dataInCell = worksheet?.Cells[numRowTableHeader, i]?.Value?.ToString()?.Trim() ?? string.Empty;

                        if (!string.IsNullOrEmpty(propinfo.Value.Trim())
                            && dataInCell.Trim().Equals(propinfo.Value.Trim(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (!ComparisonPropertyToColumnsNumber.ContainsKey(propinfo.Key))
                            {
                                ComparisonPropertyToColumnsNumber?.Add(propinfo.Key, i);
                                break; // выход из цикла перебора ячеек шапки таблицы
                            }
                        }
                    }
                }
                #endregion

                // 4 - Проверка того, все ли свойства были сопоставлены сназваниями вшапке excel
                if (IsAllColumnsNamesNotFound(nameof(ProcessingFile)))
                {
                    return;
                }

                #region 5 - Непосредственный парсинг значений ячеек EXCEL в экземпляры модели-транзакции

                // Номер строки, с которой будет начинаться парсинг - следующая за шапкой таблицы
                int startRow = numRowTableHeader + 1;

                for (var row = startRow; row <= dimensionWorkSheetRows; row++)
                {
                    var entry = Activator.CreateInstance<FuelTransactionGazProm>();

                    // Проверка достижения окончания отчета //  public ExcelRange this[int FromRow, int FromCol, int ToRow, int ToCol] { get; }
                    var cellsRange = worksheet?.Cells[row, leftCellTableHeader, row, righCellTableHeader] ?? null;
                    var cellsRangeNum = cellsRange?.Count() ?? 0;

                    if (cellsRange == null || cellsRangeNum == 0)
                        continue; // перейти к след.строке

                    var nonEmptyCellInRow = cellsRange?.Where(_ => _.Value != null && !string.IsNullOrEmpty(_.Value.ToString()))?.Count() ?? 0;
                    var emptyCellsNum = (righCellTableHeader - leftCellTableHeader + 1) - nonEmptyCellInRow;
                    if (emptyCellsNum != 0 && emptyCellsNum > AllowedEmptyCellsInRow)
                    {
                        continue; // перейти к след.строке
                    }

                    foreach (var propColumn in ComparisonPropertyToColumnsNumber ?? new())
                    {
                        try
                        {
                            var valueData = worksheet?.Cells[row, propColumn.Value]?.Value;
                            var prop = propColumn.Key;
                            var propType = prop.PropertyType;

                            if (propType.IsNullableType())
                            {
                                propType = Nullable.GetUnderlyingType(propType);
                            }
                            else if (valueData == null)
                            {
                                if (propType.IsValueType)
                                {
                                    prop.SetValue(entry, Activator.CreateInstance(propType), null);
                                }
                                else
                                {
                                    prop.SetValue(entry, default, null);
                                }

                                continue;
                            }

                            if (propType == typeof(decimal))
                            {
                                if (decimal.TryParse(valueData?.ToString() ?? string.Empty, out var decimalData))
                                {
                                    prop.SetValue(entry, decimalData, null);
                                }
                                else
                                {
                                    var destin = valueData.FormatStringToDecimal();
                                    prop.SetValue(entry, destin, null);
                                }
                            }
                            else if (propType == typeof(DateTime))
                            {
                                DateTime dateDateTime = default;
                                if (DateTime.TryParse(valueData?.ToString() ?? string.Empty, out var datetimeData))
                                {
                                    prop.SetValue(entry, datetimeData, null);
                                }
                                else if (valueData?.GetType() == typeof(double))
                                {
                                    dateDateTime = DateTime.FromOADate((double)valueData);
                                    prop.SetValue(entry, dateDateTime, null);
                                }
                                else
                                {
                                    dateDateTime = valueData.FormatToDateTime(Env);

                                    if (dateDateTime != default)
                                    {
                                        prop.SetValue(entry, dateDateTime, null);
                                    }
                                    else
                                    {
                                        entry = null;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                // Установка значения для текущего свойства модели транзакции entry
                                prop.SetValue(entry, Convert.ChangeType(valueData, prop.PropertyType), null);
                            }
                        }
                        catch (Exception exc)
                        {
                            exc.LogError(nameof(ParseFile), GetType().FullName);
                            throw;
                        }
                    }

                    if (entry != null && entry != default)
                    {
                        entry.CountryWorksheet = worksheet?.Name ?? string.Empty;
                        ItemsToMapping?.Add(entry);
                    }
                }
                #endregion
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(ProcessingFile), GetType().FullName);
            throw;
        }
    }

    protected override async Task<FuelTransaction> MappingParsedToDB(IParsedItem parsedReportsItem)
    {
        FuelTransaction dbTransaction = new();

        if(parsedReportsItem == null || parsedReportsItem is not FuelTransactionGazProm parsedTransaction)
        {
            $"Ошибка - несоответствие транзакции типу {nameof(FuelTransactionGazProm)} !".LogError(GetType().Name, nameof(MappingParsedToDB));
            return null;
        }

        try
        {
            // 1 - Поставщик топлива «Газпромнефть»
            dbTransaction.ProviderId = ProviderId;

            // 2 - Разновидность топлива
            dbTransaction.FuelTypeId = GetFuelType(parsedTransaction.FuelType ?? string.Empty);

            // 3 - Колличество подлежит конвертации, т.к. заправка в отчете отражена со знаком минус «-», а возврат наоборот - положительное число -
            // это относится только к формам 1 и 2, а в отчете с группировкой кол-во суммы не требуют конвертации
            if (ReportInnerKind == ReportKind_GazPromBY.Вариант_1 || ReportInnerKind == ReportKind_GazPromBY.Вариант_3)
            {
                dbTransaction.Quantity = Math.Round(parsedTransaction.Quantity, 2, MidpointRounding.AwayFromZero).ConvertNegativePositive();
            }
            else
            {
                dbTransaction.Quantity = Math.Round(parsedTransaction.Quantity, 2, MidpointRounding.AwayFromZero);
            }

            // 4 - Цена за литр 
            dbTransaction.Cost = Math.Round(parsedTransaction.Cost, 3, MidpointRounding.AwayFromZero);

            // 5 - Общая стоимость подлежит конвертации, т.к. заправка в отчете отражена со знаком минус «-», а возврат наоборот - положительное число - 
            // это относится только к формам 1 и 2, а в отчете с группировкой кол-во суммы не требуют конвертации
            if (ReportInnerKind == ReportKind_GazPromBY.Вариант_1 || ReportInnerKind == ReportKind_GazPromBY.Вариант_3)
            {
                dbTransaction.TotalCost = parsedTransaction.TotalCost.ConvertNegativePositive();
            }
            else
            {
                dbTransaction.TotalCost = parsedTransaction.TotalCost;
            }

            // 6 - По умолчанию ложь
            dbTransaction.IsCheck = false;

            // 7 - Местоположение заправочной станции, где была осуществлена транзакция
            dbTransaction.CountryId = await SearchCountry(parsedTransaction.Country, parsedTransaction.CountryWorksheet);

            // 8 - Валюта
            dbTransaction.CurrencyId = await GetCurrency(parsedTransaction.Currency ?? string.Empty);

            // 9 - Дата и время транзакции
            if (parsedTransaction.OperationDate == default)
            {
                var message = $"{dbTransaction?.ToString() ?? "Транзакция"} не м.б. добавлена в БД, т.к. не удалось определить дату операции";

                notSuccessItems?.Add(
                    new NotSuccessResponseItemDetailed<NotParsedTransaction>(
                        new NotParsedTransaction
                        {
                            CardNumber = string.Empty,
                            CarNumber = string.Empty,
                            NotFuelType = string.Empty,
                            TransactionID = dbTransaction.TransactionID,
                            OperationDate = dbTransaction.OperationDate,
                            Quantity = dbTransaction.Quantity,
                            Cost = dbTransaction.Cost,
                            TotalCost = dbTransaction.TotalCost,
                            IsCheck = dbTransaction.IsCheck,
                            ProviderId = dbTransaction.ProviderId,
                            FuelTypeId = dbTransaction.FuelTypeId,
                            CurrencyId = dbTransaction.CurrencyId,
                            CardId = dbTransaction.CardId,
                            CountryId = dbTransaction.CountryId,
                            DriverReportId = dbTransaction.DriverReportId,
                            IsMonthly = IsMonthly.HasValue && IsMonthly.Value ? true : default,
                            IsDayly = !IsMonthly.HasValue || !IsMonthly.Value ? true : default,
                        }, message));

                return null;
            }

            dbTransaction.OperationDate = parsedTransaction.OperationDate;

            // 10 - Заправочная карта
            // Если топливная карта не внесена в систему - пополнить коллекцию и пропустить транзакцию - не вносить ее в систему
            var foundCardId = await SearchCard(parsedTransaction.Card);

            if (!foundCardId.HasValue || foundCardId == 0)
            {
                var message = $"Номер заправочной карты «{parsedTransaction.Card ?? string.Empty}» не найден в БД";

                notSuccessItems?.Add(
                    new NotSuccessResponseItemDetailed<NotParsedTransaction>(
                        new NotParsedTransaction
                        {
                            CardNumber = parsedTransaction.Card,
                            CarNumber = string.Empty,
                            NotFuelType = string.Empty,
                            TransactionID = dbTransaction.TransactionID,
                            OperationDate = dbTransaction.OperationDate,
                            Quantity = dbTransaction.Quantity,
                            Cost = dbTransaction.Cost,
                            TotalCost = dbTransaction.TotalCost,
                            IsCheck = dbTransaction.IsCheck,
                            ProviderId = dbTransaction.ProviderId,
                            FuelTypeId = dbTransaction.FuelTypeId,
                            CurrencyId = dbTransaction.CurrencyId,
                            CardId = dbTransaction.CardId,
                            CountryId = dbTransaction.CountryId,
                            DriverReportId = dbTransaction.DriverReportId,
                            IsMonthly = IsMonthly.HasValue && IsMonthly.Value ? true : default,
                            IsDayly = !IsMonthly.HasValue || !IsMonthly.Value ? true : default,
                        }, message));

                return null;
            }

            dbTransaction.CardId = foundCardId.Value;
            return dbTransaction;
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(ParseFile), GetType().FullName);
            throw;
        }
    }

    /// <summary>
    /// Метод для поиска идентификатора страны, в которой была осуществлена заправка по ее наименованию.
    /// </summary>
    /// <param Name="country">Наименование страны из отчета.</param>
    /// <returns>Идентификатор страны.</returns>
    private async Task<int?> SearchCountry(string country, string contryWorksheet = default)
    {
        int? result;
        string trimmedCountry = string.Empty;

        if (country != null && country?.Length > 0 && country?.IndexOf(",") != -1)
        {
            trimmedCountry = country?.Substring(0, country.IndexOf(","))?.Trim() ?? string.Empty;
        }
        else if (country != null && country?.Length > 0)
        {
            trimmedCountry = country.Trim();
        }
        else
        {
            trimmedCountry = contryWorksheet;
        }

        // Вызов внутреннего метода для получения TruckId страны
        result = await GetCountryInner(trimmedCountry);

        // Если наименование не найдено в теле таблицы - попробовать найти его в наименовании листа книги excel
        if (!result.HasValue)
            result = await GetCountryInner(contryWorksheet);

        // Метод для нахождения идентификатора страны заправки по ее обрезанному наименованию
        async Task<int?> GetCountryInner(string countryName)
        {
            return countryName switch
            {
                "РБ" => await GetCountry("BY"),
                "BLR" => await GetCountry("BY"),
                "ЮКО" => await GetCountry("KZ"),
                "РФ" => await GetCountry("RUS"),
                _ => await GetCountry(countryName)
            };
        }

        return result;
    }
}
