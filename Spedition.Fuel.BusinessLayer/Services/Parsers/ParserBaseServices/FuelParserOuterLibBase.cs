using System.Reflection;
using AutoMapper.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using Spedition.Fuel.BFF.Constants;
using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;

namespace Spedition.Fuel.BusinessLayer.Services.Parsers.ParserBaseServices;

public abstract class FuelParserOuterLibBase<TParsed, TSuccess, TNotSuccess, TSearch>
    : ParserBase<TParsed, TSuccess, TNotSuccess, TSearch>
    where TParsed : class, IParsedItem
{
    public FuelParserOuterLibBase(
    IWebHostEnvironment env,
    FuelRepositories fuelRepositories,
    IConfiguration configuration,
    IMapper mapper,
    OuterLibrary outerLibrary)
    : base(env, fuelRepositories, configuration, mapper, outerLibrary)
    {
    }

    protected override void ChoiceOuterLib()
    {
        choiceOuterLib = OuterLibraryValue switch
        {
            OuterLibrary.NPOI => Path.GetExtension(FilePathDestination) == ".xlsx" ? ProcessingFileNpoiXssf : ProcessingFileNpoiHssf,
            OuterLibrary.EPPlus => ProcessingFileEPPlus,
            OuterLibrary.EPPlusShort => ProcessingFileEPPlusShort,
            _ => ProcessingFileEPPlus
        };
    }

    protected bool IsAllColumnsNamesNotFound(string method)
    {
        var predicate = () => (ComparisonPropertyToColumnsName?.Count ?? 0) != (ComparisonPropertyToColumnsNumber?.Count ?? 0);
        if (predicate())
        {
            NotifyMessage += $"Форма отчета поменялась! Сообщите об этом в службу разработки! " +
                $"Из «{ComparisonPropertyToColumnsName?.Count ?? 0}» колонок " +
                $"было найдено только «{ComparisonPropertyToColumnsNumber?.Count ?? 0}»";
            NotifyMessage.LogWarning(GetType().Name, method);
        }

        return predicate();
    }

    protected void ProcessingFileNpoiHssf()
    {
        ItemsToMapping = new();

        try
        {
            using FileStream fs = new(FilePathDestination, FileMode.Open, FileAccess.Read);

            // Открыть книгу
            using var workbook = new HSSFWorkbook(fs);

            // Вычислить все формулы в книге - для того, чтобы можно было считывать результаты вычислений
            HSSFFormulaEvaluator formula = new(workbook);
            formula.EvaluateAll();

            // Число страниц в книге
            NumberWorksheets = workbook?.NumberOfSheets ?? 0;

            if (NumberWorksheets == 0)
            {
                NotifyMessage += "Отчет пустой, не содержит ни одного листа!";
                NotifyMessage.LogError(GetType().Name, nameof(ProcessingFileNpoiHssf));
                return;
            }

            // Перебор листов книги
            for (int n = 0; n < NumberWorksheets; n++)
            {
                ComparisonPropertyToColumnsNumber = new();

                #region 1 - Поиск номера строки, содежащей заголовок таблицы
                var worksheet = workbook?.GetSheetAt(n);

                // Первая заполненная строка на текущем листе книги
                var firstRowNum = worksheet?.FirstRowNum ?? 0;

                // Число строк на текущем листе книги
                var lastRowNum = worksheet?.LastRowNum ?? 0;

                // Номер строки, содержащий искомую шапку таблицы
                var numRowTableHeader = -1;

                // Левая граница диапазона, содержащего шапку таблицы
                var leftCellTableHeader = 0;

                // Правая граница диапазона, содержащего шапку таблицы
                var righCellTableHeader = 0;

                // Перебор первых 20-ти  строк либо если диапазон строк меньше -  в пределах диапазона
                // (предположительно, что шапка таблицы располагается в этом диапазоне)
                var numRowsForSearch = lastRowNum <= 20 ? lastRowNum : 20;

                // Счетчик числа выполненных при поиске шапки таблицы условий
                var numCondition = 0;

                // Перебор всех строк текущего листа книги
                for (int i = firstRowNum; i < numRowsForSearch; i++)
                {
                    var t = i == 3 ? true : false;

                    // Если значение найдено - остановить поиск и перейти к следующему листу
                    if (numRowTableHeader > -1)
                    {
                        break;
                    }
                    else
                    {
                        var row = worksheet?.GetRow(i);

                        leftCellTableHeader = row?.FirstCellNum ?? 0;
                        righCellTableHeader = row?.LastCellNum ?? 0;

                        // Перебор всех ячеек текущей строки
                        for (int j = leftCellTableHeader; j < righCellTableHeader; j++)
                        {
                            var cellValue = row?.GetCell(j);
                            var cellType = cellValue?.CellType ?? CellType.Unknown;

                            if (cellType == CellType.String)
                            {
                                var data = cellValue.StringCellValue;
                                if (ColumnsNames.Any(column => data.ToLower().Contains(column.ToLower()))
                                    || ColumnsNames.Any(column => column.ToLower().Contains(data.ToLower())))
                                {
                                    numCondition++;

                                    // если в строке найдены все обязательно присутствующие в шапке таблицы значения -
                                    // остановить поиск строки в которой находится заголовок таблицы и выйти из вложенного цикла
                                    if (numCondition == ColumnsNames.Count)
                                    {
                                        numRowTableHeader = i;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                // Если на текущем листе не обнаружен заголовок таблицы - перейти к следующему листу
                if (numRowTableHeader == -1)
                {
                    continue;
                }

                #region 2 - Заполнение коллекции,сопоставляющей свойства - номера столбцов
                if ((ComparisonPropertyToColumnsName?.Keys?.Count ?? 0) == 0)
                {
                    NotifyMessage += "Ошибка на уровне сервера, обратитесь в службу разработки!";
                    $"ComparisonPropertyToColumnsName.Count = 0".LogError(GetType().Name, nameof(ProcessingFileNpoiHssf));
                    return;
                }

                // Поиск соответствия значения атрибута текущего перебираемого свойства модели значению в ячейке шапки таблицы 
                foreach (var propinfo in ComparisonPropertyToColumnsName ?? new())
                {
                    // Значение кастомного атрибута Name у свойства propinfo
                    var nameAtrValue = propinfo.Value;

                    // Ряд в котором содержится заголовок
                    var row = worksheet.GetRow(numRowTableHeader);

                    // Цикл перебора ячеек шапки таблицы
                    for (int i = leftCellTableHeader; i < righCellTableHeader; i++)
                    {
                        var cellVal = row?.GetCell(i);
                        var cellType = cellVal?.CellType;
                        var valString = string.Empty;

                        if (cellType != null && cellType == CellType.String)
                        {
                            valString = cellVal?.StringCellValue?.Trim() ?? string.Empty;
                        }

                        if (!string.IsNullOrEmpty(nameAtrValue)
                            && !string.IsNullOrEmpty(valString)
                            && (valString.Contains(nameAtrValue, StringComparison.InvariantCultureIgnoreCase)
                            || nameAtrValue.Contains(valString, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            if (!ComparisonPropertyToColumnsNumber.ContainsKey(propinfo.Key))
                            {
                                ComparisonPropertyToColumnsNumber.Add(propinfo.Key, i);
                                break; // выход из цикла перебора ячеек шапки таблицы
                            }
                        }
                    }
                }
                #endregion

                // 3 - Проверка того, все ли свойства были сопоставлены с названиями в шапке excel
                if (IsAllColumnsNamesNotFound(nameof(ProcessingFileNpoiHssf)))
                {
                    return;
                }

                #region 4 - Непосредственный парсинг значений ячеек EXCEL в экземпляры модели-транзакции
                int startRow = numRowTableHeader + 1; // Номер строки, с которой будет начинаться парсинг - следующая за шапкой таблицы

                for (var rowInd = startRow; rowInd <= lastRowNum; rowInd++)
                {
                    var entry = Activator.CreateInstance<TParsed>();
                    var row = worksheet?.GetRow(rowInd);

                    // Коллекция ячеек в текущей строке
                    var cellsInRow = row?.Cells ?? new();

                    // Проверка достижения окончания отчета
                    if ((cellsInRow?.Count ?? 0) < ComparisonPropertyToColumnsNumber.Count
                        || (cellsInRow?.Where(cell => cell.ColumnIndex < righCellTableHeader)?
                        .Where(cell => cell.CellType != CellType.Blank && cell.CellType != CellType.Error && cell.CellType != CellType.Unknown)?
                        .Count() ?? 0) < ComparisonPropertyToColumnsNumber.Count - 3)
                    {
                        continue; // перейти к следующей строке
                    }

                    // Последовательное считывание значений свойств модели транзакции
                    foreach (var propColumn in ComparisonPropertyToColumnsNumber)
                    {
                        // Значение ячейки
                        var cellVal = row?.GetCell(propColumn.Value);
                        var cellType = cellVal?.CellType ?? CellType.Error;

                        try
                        {
                            var property = propColumn.Key;
                            var propType = property.PropertyType;
                            var isNullable = propType.IsNullableType();

                            if (isNullable)
                            {
                                propType = Nullable.GetUnderlyingType(propType);
                            }

                            if (cellType == CellType.Blank || cellType == CellType.Error || cellType == CellType.Unknown)
                            {
                                if (isNullable)
                                {
                                    property.SetValue(entry, null, null);
                                }
                                else
                                {
                                    if (propType.IsValueType)
                                    {
                                        property.SetValue(entry, Activator.CreateInstance(propType), null);
                                    }
                                    else
                                    {
                                        property.SetValue(entry, default, null);
                                    }
                                }
                            }
                            else if (cellType == CellType.Formula)
                            {
                                try
                                {
                                    var resCalculate = cellVal.NumericCellValue.ToString();

                                    if (!string.IsNullOrEmpty(resCalculate))
                                        property.SetValue(entry, Convert.ChangeType(resCalculate, propType), null);
                                }
                                catch (Exception exc)
                                {
                                    continue;
                                }
                            }
                            else if (propType == typeof(DateTime) && cellType == CellType.Numeric)
                            {
                                try
                                {
                                    var dateDateTime = cellVal.DateCellValue;
                                    property.SetValue(entry, dateDateTime, null);
                                }
                                catch
                                {
                                    try
                                    {
                                        var dateDateTime = cellVal.NumericCellValue;
                                        property.SetValue(entry, dateDateTime, null);
                                    }
                                    catch
                                    {
                                        property.SetValue(entry, DateTime.FromOADate(Convert.ToDouble(cellVal.NumericCellValue)), null);
                                    }
                                }
                            }
                            else if (propType == typeof(DateTime) && cellType == CellType.String)
                                property.SetValue(entry, cellVal.StringCellValue.GetDateTimeFromString(), null);
                            else if (propType == typeof(decimal) && cellType == CellType.Numeric && decimal.TryParse(cellVal.NumericCellValue.ToString(), out var resDec))
                                property.SetValue(entry, resDec, null);
                            else if (propType == typeof(decimal) && cellType == CellType.String && decimal.TryParse(cellVal.StringCellValue.ToString(), out var resDecStr))
                                property.SetValue(entry, resDecStr, null);
                            else if (propType == typeof(decimal) && cellType == CellType.String && !decimal.TryParse(cellVal.StringCellValue.ToString(), out var resDecStrNotParsed))
                                property.SetValue(entry, isNullable ? null : default, null);
                            else if (propType == typeof(double) && cellType == CellType.Numeric && double.TryParse(cellVal.NumericCellValue.ToString(), out var resDouble))
                                property.SetValue(entry, resDouble, null);
                            else if (propType == typeof(double) && cellType == CellType.String && double.TryParse(cellVal.StringCellValue, out var resDoubleStr))
                                property.SetValue(entry, resDoubleStr, null);
                            else if (propType == typeof(double) && cellType == CellType.String && !double.TryParse(cellVal.StringCellValue, out var resDoubleStrNotParsed))
                                property.SetValue(entry, isNullable ? null : default, null);
                            else if (propType == typeof(long) && cellType == CellType.Numeric && long.TryParse(cellVal.NumericCellValue.ToString(), out var resLong))
                                property.SetValue(entry, resLong, null);
                            else if (propType == typeof(long) && cellType == CellType.String && long.TryParse(cellVal.StringCellValue, out var resLongStr))
                                property.SetValue(entry, resLongStr, null);
                            else if (propType == typeof(long) && cellType == CellType.String && !long.TryParse(cellVal.StringCellValue, out var resLongStrNotParsed))
                                property.SetValue(entry, isNullable ? null : default, null);
                            else if (propType == typeof(int) && cellType == CellType.Numeric && int.TryParse(cellVal.NumericCellValue.ToString(), out var resInt))
                                property.SetValue(entry, resInt, null);
                            else if (propType == typeof(int) && cellType == CellType.String && int.TryParse(cellVal.StringCellValue, out var resIntStr))
                                property.SetValue(entry, resIntStr, null);
                            else if (propType == typeof(int) && cellType == CellType.String && !int.TryParse(cellVal.StringCellValue, out var resIntStrNotParsed))
                                property.SetValue(entry, isNullable ? null : default, null);
                            else if (propType == typeof(short) && cellType == CellType.Numeric && short.TryParse(cellVal.NumericCellValue.ToString(), out var resShort))
                                property.SetValue(entry, resShort, null);
                            else if (propType == typeof(short) && cellType == CellType.String && short.TryParse(cellVal.StringCellValue, out var resShortStr))
                                property.SetValue(entry, resShortStr, null);
                            else if (propType == typeof(short) && cellType == CellType.String && !short.TryParse(cellVal.StringCellValue, out var resShortStrNotParsed))
                                property.SetValue(entry, isNullable ? null : default, null);
                            else if (propType == typeof(byte) && cellType == CellType.Numeric && byte.TryParse(cellVal.NumericCellValue.ToString(), out var resByte))
                                property.SetValue(entry, resByte, null);
                            else if (propType == typeof(byte) && cellType == CellType.String && byte.TryParse(cellVal.StringCellValue, out var resByteStr))
                                property.SetValue(entry, resByteStr, null);
                            else if (propType == typeof(byte) && cellType == CellType.String && !byte.TryParse(cellVal.StringCellValue, out var resByteStrNotParsed))
                                property.SetValue(entry, isNullable ? null : default, null);
                            else if (propType == typeof(bool) && cellType == CellType.Boolean)
                                property.SetValue(entry, cellVal.BooleanCellValue, null);
                            else if (propType == typeof(string) && cellType == CellType.String)
                                property.SetValue(entry, cellVal.StringCellValue, null);
                            else if (propType == typeof(string) && cellType == CellType.Numeric)
                                property.SetValue(entry, cellVal.NumericCellValue.ToString(), null);
                            else if (propType != typeof(string) && cellType == CellType.String)
                                property.SetValue(entry, Convert.ChangeType(cellVal.StringCellValue, propType), null);
                            else
                            {
                                if (isNullable)
                                {
                                    property.SetValue(entry, null, null);
                                }
                                else
                                {
                                    if (propType.IsValueType)
                                    {
                                        property.SetValue(entry, Activator.CreateInstance(propType), null);
                                    }
                                    else
                                    {
                                        property.SetValue(entry, default, null);
                                    }
                                }
                            }
                        }
                        catch (Exception exc)
                        {
                            exc.LogError(nameof(ParseFile), GetType().FullName);
                            throw;
                        }
                    }

                    if (entry != null && entry != default)
                        ItemsToMapping?.Add(entry);
                }
                #endregion
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(ParseFile), GetType().FullName);
            throw;
        }
    }

    protected void ProcessingFileNpoiXssf()
    {
        ItemsToMapping = new();

        try
        {
            using FileStream fs = new(FilePathDestination, FileMode.Open, FileAccess.Read);

            // Открыть книгу
            using var workbook = new XSSFWorkbook(fs);

            // Вычислить все формулы в книге - для того, чтобы можно было считывать результаты вычислений
            XSSFFormulaEvaluator formula = new(workbook);
            formula.EvaluateAll();

            // Число страниц в книге
            NumberWorksheets = workbook?.NumberOfSheets ?? 0;

            if (NumberWorksheets == 0)
            {
                NotifyMessage += "Отчет пустой, не содержит ни одного листа!";
                NotifyMessage.LogError(GetType().Name, nameof(ProcessingFileNpoiXssf));
                return;
            }

            // Перебор листов книги
            for (int n = 0; n < NumberWorksheets; n++)
            {
                ComparisonPropertyToColumnsNumber = new();

                #region 1 - Поиск номера строки, содежащей заголовок таблицы
                var worksheet = workbook?.GetSheetAt(n);

                // Первая заполненная строка на текущем листе книги
                var firstRowNum = worksheet?.FirstRowNum ?? 0;

                // Число строк на текущем листе книги
                var lastRowNum = worksheet?.LastRowNum ?? 0;

                // Номер строки, содержащий искомую шапку таблицы
                var numRowTableHeader = -1;

                // Левая граница диапазона, содержащего шапку таблицы
                var leftCellTableHeader = 0;

                // Правая граница диапазона, содержащего шапку таблицы
                var righCellTableHeader = 0;

                // Перебор первых 20-ти  строк либо если диапазон строк меньше -  в пределах диапазона
                // (предположительно, что шапка таблицы располагается в этом диапазоне)
                var numRowsForSearch = lastRowNum <= 20 ? lastRowNum : 20;

                // Счетчик числа выполненных при поиске шапки таблицы условий
                var numCondition = 0;

                // Перебор всех строк текущего листа книги
                for (int i = firstRowNum; i < numRowsForSearch; i++)
                {
                    // Если значение найдено - остановить поиск и перейти к следующему листу
                    if (numRowTableHeader > -1)
                    {
                        break;
                    }
                    else
                    {
                        var row = worksheet?.GetRow(i);

                        leftCellTableHeader = row?.FirstCellNum ?? 0;
                        righCellTableHeader = row?.LastCellNum ?? 0;

                        // Перебор всех ячеек текущей строки
                        for (int j = leftCellTableHeader; j < righCellTableHeader; j++)
                        {
                            var cellValue = row?.GetCell(j);
                            var cellType = cellValue?.CellType ?? CellType.Unknown;

                            if (cellType == CellType.String)
                            {
                                var data = cellValue.StringCellValue;
                                if (ColumnsNames.Any(column => data.ToLower().Contains(column.ToLower()))
                                    || ColumnsNames.Any(column => column.ToLower().Contains(data.ToLower())))
                                {
                                    numCondition++;

                                    // если в строке найдены все обязательно присутствующих в шапке таблицы значения -
                                    // остановить поиск строки в которой находится заголовок таблицы и выйти из вложенного цикла
                                    if (numCondition == ColumnsNames.Count)
                                    {
                                        numRowTableHeader = i;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                // Если на текущем листе не обнаружен заголовок таблицы - перейти к следующему листу
                if (numRowTableHeader == -1)
                {
                    continue;
                }

                #region 2 - Заполнение коллекции,сопоставляющей свойства - номера столбцов
                if ((ComparisonPropertyToColumnsName?.Keys?.Count ?? 0) == 0)
                {
                    NotifyMessage += "Ошибка на уровне сервера, обратитесь в службу разработки!";
                    $"ComparisonPropertyToColumnsName.Count = 0".LogError(GetType().Name, nameof(ProcessingFileNpoiXssf));
                    return;
                }

                // Поиск соответствия значения атрибута текущего перебираемого свойства модели значению в ячейке шапки таблицы 
                foreach (var propinfo in ComparisonPropertyToColumnsName ?? new())
                {
                    // Значение кастомного атрибута Name у свойства propinfo
                    var nameAtrValue = propinfo.Value;

                    // Ряд в котором содержится заголовок
                    var row = worksheet.GetRow(numRowTableHeader);

                    // Цикл перебора ячеек шапки таблицы
                    for (int i = leftCellTableHeader; i < righCellTableHeader; i++)
                    {
                        var cellVal = row?.GetCell(i);
                        var cellType = cellVal?.CellType;
                        var valString = string.Empty;

                        if (cellType != null && cellType == CellType.String)
                        {
                            valString = cellVal?.StringCellValue?.Trim() ?? string.Empty;
                        }

                        if (!string.IsNullOrEmpty(nameAtrValue)
                            && !string.IsNullOrEmpty(valString)
                            && (valString.Contains(nameAtrValue, StringComparison.InvariantCultureIgnoreCase)
                            || nameAtrValue.Contains(valString, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            if (!ComparisonPropertyToColumnsNumber.ContainsKey(propinfo.Key))
                            {
                                ComparisonPropertyToColumnsNumber.Add(propinfo.Key, i);
                                break; // выход из цикла перебора ячеек шапки таблицы
                            }
                        }
                    }
                }
                #endregion

                // 3 - Проверка того, все ли свойства были сопоставлены с названиями в шапке excel
                if (IsAllColumnsNamesNotFound(nameof(ProcessingFileNpoiXssf)))
                {
                    return;
                }

                #region 4 - Непосредственный парсинг значений ячеек EXCEL в экземпляры модели-транзакции
                int startRow = numRowTableHeader + 1; // Номер строки, с которой будет начинаться парсинг - следующая за шапкой таблицы

                var emptyRowsNumber = 0; // счетчик пустых строк

                for (var rowInd = startRow; rowInd <= lastRowNum; rowInd++)
                {
                    var entry = Activator.CreateInstance<TParsed>();
                    var row = worksheet?.GetRow(rowInd);

                    // Коллекция ячеек в текущей строке
                    var cellsInRow = row?.Cells ?? new();

                    // Проверка достижения окончания отчета
                    if ((cellsInRow?.Count ?? 0) < ComparisonPropertyToColumnsNumber.Count
                        || (cellsInRow?.Where(cell => cell.ColumnIndex < righCellTableHeader)?
                        .Where(cell => cell.CellType != CellType.Blank && cell.CellType != CellType.Error && cell.CellType != CellType.Unknown)?
                        .Count() ?? 0) < ComparisonPropertyToColumnsNumber.Count - 3)
                    {
                        emptyRowsNumber++;

                        if(emptyRowsNumber < 6)
                        {
                            continue; // перейти к следующей строке
                        }
                        else
                        {
                            break; // закончить парсинг отчета - внизу м.б. промежуточные итоги, после отступа не менее 5 строк
                        }
                    }

                    // Последовательное считывание значений свойств модели транзакции
                    foreach (var propColumn in ComparisonPropertyToColumnsNumber)
                    {
                        // Значение ячейки
                        var cellVal = row?.GetCell(propColumn.Value);
                        var cellType = cellVal?.CellType ?? CellType.Error;

                        try
                        {
                            var property = propColumn.Key;
                            var propType = property.PropertyType;
                            var isNullable = propType.IsNullableType();

                            if (isNullable)
                            {
                                propType = Nullable.GetUnderlyingType(propType);
                            }

                            if (cellType == CellType.Blank || cellType == CellType.Error || cellType == CellType.Unknown)
                            {
                                if (isNullable)
                                {
                                    property.SetValue(entry, null, null);
                                }
                                else
                                {
                                    if (propType.IsValueType)
                                    {
                                        property.SetValue(entry, Activator.CreateInstance(propType), null);
                                    }
                                    else
                                    {
                                        property.SetValue(entry, default, null);
                                    }
                                }
                            }
                            else if (cellType == CellType.Formula)
                            {
                                try
                                {
                                    var resCalculate = cellVal.NumericCellValue.ToString();

                                    if (!string.IsNullOrEmpty(resCalculate))
                                        property.SetValue(entry, Convert.ChangeType(resCalculate, propType), null);
                                }
                                catch (Exception exc)
                                {
                                    continue;
                                }
                            }
                            else if (propType == typeof(DateTime) && cellType == CellType.Numeric)
                            {
                                try
                                {
                                    var dateDateTime = cellVal.DateCellValue;
                                    property.SetValue(entry, dateDateTime, null);
                                }
                                catch
                                {
                                    try
                                    {
                                        var dateDateTime = cellVal.NumericCellValue;
                                        property.SetValue(entry, dateDateTime, null);
                                    }
                                    catch
                                    {
                                        property.SetValue(entry, DateTime.FromOADate(Convert.ToDouble(cellVal.NumericCellValue)), null);
                                    }
                                }
                            }
                            else if (propType == typeof(DateTime) && cellType == CellType.String)
                                property.SetValue(entry, cellVal.StringCellValue.GetDateTimeFromString(), null);
                            else if (propType == typeof(decimal) && cellType == CellType.Numeric && decimal.TryParse(cellVal.NumericCellValue.ToString(), out var resDec))
                                property.SetValue(entry, resDec, null);
                            else if (propType == typeof(decimal) && cellType == CellType.String && decimal.TryParse(cellVal.StringCellValue.ToString(), out var resDecStr))
                                property.SetValue(entry, resDecStr, null);
                            else if (propType == typeof(decimal) && cellType == CellType.String && !decimal.TryParse(cellVal.StringCellValue.ToString(), out var resDecStrNotParsed))
                                property.SetValue(entry, isNullable ? null : default, null);
                            else if (propType == typeof(double) && cellType == CellType.Numeric && double.TryParse(cellVal.NumericCellValue.ToString(), out var resDouble))
                                property.SetValue(entry, resDouble, null);
                            else if (propType == typeof(double) && cellType == CellType.String && double.TryParse(cellVal.StringCellValue, out var resDoubleStr))
                                property.SetValue(entry, resDoubleStr, null);
                            else if (propType == typeof(double) && cellType == CellType.String && !double.TryParse(cellVal.StringCellValue, out var resDoubleStrNotParsed))
                                property.SetValue(entry, isNullable ? null : default, null);
                            else if (propType == typeof(long) && cellType == CellType.Numeric && long.TryParse(cellVal.NumericCellValue.ToString(), out var resLong))
                                property.SetValue(entry, resLong, null);
                            else if (propType == typeof(long) && cellType == CellType.String && long.TryParse(cellVal.StringCellValue, out var resLongStr))
                                property.SetValue(entry, resLongStr, null);
                            else if (propType == typeof(long) && cellType == CellType.String && !long.TryParse(cellVal.StringCellValue, out var resLongStrNotParsed))
                                property.SetValue(entry, isNullable ? null : default, null);
                            else if (propType == typeof(int) && cellType == CellType.Numeric && int.TryParse(cellVal.NumericCellValue.ToString(), out var resInt))
                                property.SetValue(entry, resInt, null);
                            else if (propType == typeof(int) && cellType == CellType.String && int.TryParse(cellVal.StringCellValue, out var resIntStr))
                                property.SetValue(entry, resIntStr, null);
                            else if (propType == typeof(int) && cellType == CellType.String && !int.TryParse(cellVal.StringCellValue, out var resIntStrNotParsed))
                                property.SetValue(entry, isNullable ? null : default, null);
                            else if (propType == typeof(short) && cellType == CellType.Numeric && short.TryParse(cellVal.NumericCellValue.ToString(), out var resShort))
                                property.SetValue(entry, resShort, null);
                            else if (propType == typeof(short) && cellType == CellType.String && short.TryParse(cellVal.StringCellValue, out var resShortStr))
                                property.SetValue(entry, resShortStr, null);
                            else if (propType == typeof(short) && cellType == CellType.String && !short.TryParse(cellVal.StringCellValue, out var resShortStrNotParsed))
                                property.SetValue(entry, isNullable ? null : default, null);
                            else if (propType == typeof(byte) && cellType == CellType.Numeric && byte.TryParse(cellVal.NumericCellValue.ToString(), out var resByte))
                                property.SetValue(entry, resByte, null);
                            else if (propType == typeof(byte) && cellType == CellType.String && byte.TryParse(cellVal.StringCellValue, out var resByteStr))
                                property.SetValue(entry, resByteStr, null);
                            else if (propType == typeof(byte) && cellType == CellType.String && !byte.TryParse(cellVal.StringCellValue, out var resByteStrNotParsed))
                                property.SetValue(entry, isNullable ? null : default, null);
                            else if (propType == typeof(bool) && cellType == CellType.Boolean)
                                property.SetValue(entry, cellVal.BooleanCellValue, null);
                            else if (propType == typeof(string) && cellType == CellType.String)
                                property.SetValue(entry, cellVal.StringCellValue, null);
                            else if (propType == typeof(string) && cellType == CellType.Numeric)
                                property.SetValue(entry, cellVal.NumericCellValue.ToString(), null);
                            else if (propType != typeof(string) && cellType == CellType.String)
                                property.SetValue(entry, Convert.ChangeType(cellVal.StringCellValue, propType), null);
                            else
                            {
                                if (isNullable)
                                {
                                    property.SetValue(entry, null, null);
                                }
                                else
                                {
                                    if (propType.IsValueType)
                                    {
                                        property.SetValue(entry, Activator.CreateInstance(propType), null);
                                    }
                                    else
                                    {
                                        property.SetValue(entry, default, null);
                                    }
                                }
                            }
                        }
                        catch (Exception exc)
                        {
                            exc.LogError(nameof(ParseFile), GetType().FullName);
                            throw;
                        }
                    }

                    if (entry != null && entry != default)
                        ItemsToMapping?.Add(entry);
                }
                #endregion
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(ParseFile), GetType().FullName);
            throw;
        }
    }

    protected void ProcessingFileEPPlus()
    {
        ItemsToMapping = new();
        try
        {
            using FileStream fs = new(FilePathDestination, FileMode.Open, FileAccess.Read);

            using var package = new ExcelPackage(fs);

            using var workbook = package?.Workbook;

            // Ссылка на все страницы в книге
            using var worksheets = workbook?.Worksheets;

            // Число страниц в книге excel
            NumberWorksheets = worksheets?.Count ?? 0;

            if (NumberWorksheets == 0)
            {
                NotifyMessage += "Отчет пустой, не содержит ни одного листа!";
                NotifyMessage.LogError(GetType().Name, nameof(ProcessingFileEPPlus));
                return;
            }

            // Перебор листов книги
            for (var worksheetInd = 0; worksheetInd < NumberWorksheets; worksheetInd++)
            {
                ComparisonPropertyToColumnsNumber = new();

                #region 1 - Поиск номера строки, содежащей заголовок таблицы

                // Текущая страница
                var worksheet = worksheets[worksheetInd];

                // Число строк на текущем листе книги
                var dimensionWorkSheetRows = worksheet?.Dimension?.Rows ?? 0;

                // Число столбцов на текущем листе книги
                var dimensionWorkSheetColumns = worksheet?.Dimension?.Columns ?? 0;

                // Номер строки, содержащий искомую шапку таблицы
                var numRowTableHeader = 0;

                // Левая граница диапазона, содержащего шапку таблицы
                var leftCellTableHeader = 0;

                // Правая граница диапазона, содержащего шапку таблицы
                var righCellTableHeader = 0;

                // Перебор первых 20-ти  строк либо если диапазон строк меньше -  в пределах диапазона
                // (предположительно, что шапка таблицы располагается в этом диапазоне)
                var numRowsForSearch = dimensionWorkSheetRows <= 20 ? dimensionWorkSheetRows : 20;

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
                            if (ColumnsNames.Any(column => data.Contains(column)) 
                                || ColumnsNames.Any(column => column.Contains(data)))
                            {
                                numCondition++;

                                // если в строке найдены ВСЕ обязательно присутствующие в шапке таблицы значения -
                                // остановить поиск строки в которой находится заголовок таблицы и выйти из вложенного цикла
                                if (numCondition == ColumnsNames.Count)
                                {
                                    numRowTableHeader = i;
                                    break;
                                }
                            }
                        }
                    }
                }

                // Если шапка таблицы не была обнаружена - вернуть статус с сообщением об ошибке
                if (numRowTableHeader == 0)
                {
                    continue; // Перейти к след.странице, если таковая имеется
                }

                // Определение левой границы шапки таблицы, содержащей значения
                for (int i = 1; i <= dimensionWorkSheetColumns; i++)
                {
                    if (!string.IsNullOrEmpty(worksheet?.Cells[numRowTableHeader, i]?.Value?.ToString()))
                    {
                        leftCellTableHeader = i;
                        var countEmptyCells = 0;

                        // Определение правой границы шапки таблицы, содержащей значения
                        for (int j = leftCellTableHeader + 1; j <= dimensionWorkSheetColumns; j++)
                        {
                            if (countEmptyCells > 1) break;

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

                        // Если не найдена первая пустая ячейка справа, присвоить значение правой границы диапазона значения
                        righCellTableHeader = righCellTableHeader == 0 ? dimensionWorkSheetColumns : righCellTableHeader;
                        break;
                    }
                }
                #endregion

                #region 2 - Заполнение коллекции,сопоставляющей свойства - номера столбцов

                // Поиск соответствия значения атрибута UserName текущего перебираемого свойства модели-транзакции значению в ячейке шапки таблицы
                foreach (var propinfo in ComparisonPropertyToColumnsName ?? new())
                {
                    // Значение кастомного атрибута у свойства propinfo - в зависимости от вида отчета
                    var nameAtrValue = propinfo.Value;

                    // Цикл перебора ячеек шапки таблицы
                    for (int i = leftCellTableHeader; i <= righCellTableHeader; i++)
                    {
                        // Текущее знаение проверяемой ячейки
                        var dataInCell = worksheet?.Cells[numRowTableHeader, i]?.Value?.ToString()?.Trim() ?? string.Empty;

                        // Если у столбца в шапке таблицы нет наименования (так у Аднок) - пустая ячейка
                        if (string.IsNullOrEmpty(dataInCell) && propinfo.Key.Equals("Place"))
                        {
                            ComparisonPropertyToColumnsNumber.Add(propinfo.Key, i);
                            break; // выход из цикла перебора ячеек шапки таблицы
                        }

                        if (!string.IsNullOrEmpty(nameAtrValue)
                            && !string.IsNullOrEmpty(dataInCell)
                            && dataInCell.Contains(nameAtrValue, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (!ComparisonPropertyToColumnsNumber.ContainsKey(propinfo.Key))
                            {
                                ComparisonPropertyToColumnsNumber.Add(propinfo.Key, i);
                                break; // выход из цикла перебора ячеек шапки таблицы
                            }
                        }
                    }
                }
                #endregion

                // 3 - Проверка того, все ли свойства были сопоставлены сназваниями вшапке excel
                if (IsAllColumnsNamesNotFound(nameof(ProcessingFileEPPlus)))
                {
                    return;
                }

                #region 4 - Непосредственный парсинг значений ячеек EXCEL в экземпляры модели-транзакции

                // Номер строки, с которой будет начинаться парсинг - следующая за шапкой таблицы
                int startRow = numRowTableHeader + 1;
                for (var row = startRow; row <= dimensionWorkSheetRows; row++)
                {
                    var entry = Activator.CreateInstance<TParsed>();

                    // Проверка достижения окончания отчета this[int FromRow, int FromCol, int ToRow, int ToCol]
                    if (worksheet.Cells[row, leftCellTableHeader, row, righCellTableHeader]?.Count() == 0
                        || worksheet.Cells[row, leftCellTableHeader, row, righCellTableHeader]?.Count() < righCellTableHeader - leftCellTableHeader + 1
                        || worksheet.Cells[row, leftCellTableHeader, row, righCellTableHeader]?
                        .Where(range => !string.IsNullOrEmpty(range.GetValue<string>()))?.Count() < ComparisonPropertyToColumnsNumber.Count - 3)
                    {
                        continue;
                    }
                    else
                    {
                        foreach (var propColumn in ComparisonPropertyToColumnsNumber ?? new())
                        {
                            try
                            {
                                var valueData = worksheet?.Cells[row, propColumn.Value]?.Value;
                                var property = propColumn.Key;
                                var propType = property.PropertyType;

                                if (propType.IsNullableType())
                                {
                                    propType = Nullable.GetUnderlyingType(propType);
                                }
                                else if (valueData == null)
                                {
                                    if (propType.IsValueType)
                                    {
                                        property.SetValue(entry, Activator.CreateInstance(propType), null);
                                    }
                                    else
                                    {
                                        property.SetValue(entry, default, null);
                                    }

                                    continue;
                                }

                                if (propType == typeof(decimal))
                                {
                                    if (decimal.TryParse(valueData?.ToString() ?? string.Empty, out var decimalData))
                                    {
                                        property.SetValue(entry, decimalData, null);
                                    }
                                    else
                                    {
                                        var destin = valueData.FormatStringToDecimal();
                                        property.SetValue(entry, destin, null);
                                    }
                                } 
                                else if (propType == typeof(DateTime))
                                {
                                    DateTime dateDateTime = default;

                                    if (DateTime.TryParse(valueData?.ToString() ?? string.Empty, out var datetimeData))
                                    {
                                        property.SetValue(entry, datetimeData, null);
                                    }
                                    else if (valueData?.GetType() == typeof(double))
                                    {
                                        dateDateTime = DateTime.FromOADate((double)valueData);
                                        property.SetValue(entry, dateDateTime, null);
                                    }
                                    else
                                    {
                                        dateDateTime = valueData.FormatToDateTime(Env);
                                        property.SetValue(entry, dateDateTime, null);
                                    }
                                }
                                else
                                {
                                    // Установка значения для текущего свойства модели транзакции entry
                                    property.SetValue(entry, Convert.ChangeType(valueData, property.PropertyType), null);
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
                            ItemsToMapping?.Add(entry);
                        }
                    }
                }

                #endregion
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(ParseFile), GetType().FullName);
            throw;
        }
    }

    protected void ProcessingFileEPPlusShort()
    {
        ItemsToMapping = new();
        ConcurrentBag<TParsed> temp = new();
        try
        {
            using FileStream fs = new(FilePathDestination, FileMode.Open, FileAccess.Read);
            using ExcelPackage package = new(fs);
            using var worksheets = package.Workbook.Worksheets;

            if ((worksheets?.Count() ?? 0) == 0)
            {
                NotifyMessage += "Отчет пустой, не содержит ни одного листа!";
                NotifyMessage.LogError(GetType().Name, nameof(ProcessingFileEPPlusShort));
                return;
            }

            foreach (var worksheet in worksheets)
            {
                // Число строк на текущем листе книги
                var dimensionWorkSheetRows = worksheet?.Dimension?.Rows ?? 0;

                // Число столбцов на текущем листе книги
                var dimensionWorkSheetColumns = worksheet?.Dimension?.Columns ?? 0;

                // this[int FromRow, int FromCol, int ToRow, int ToCol]
                var range = worksheet.Cells[1, 1, dimensionWorkSheetRows, dimensionWorkSheetColumns];

                var rowsData = GetRowsData(range);
                ConcurrentBag<Dictionary<string, object>> GetRowsData(ExcelRange cells)
                {
                    ConcurrentBag<Dictionary<string, object>> res = new();
                    var addr = cells.Address;
                    var firstCol = cells.Start.Column;
                    var lastCol = cells.End.Column;
                    var firstRow = cells.Start.Row;
                    var lastRow = cells.End.Row;

                    for (int row = firstRow; row <= lastRow; row++)
                    {
                        var rowsVals = new Dictionary<string, object>();
                        for (int col = firstCol; col <= lastCol; col++)
                        {
                            rowsVals.Add(cells[row, col].Address, cells[row, col].Value);
                        }

                        if (rowsVals.Values.All(rowsVal => rowsVal != null)
                            || rowsVals.Values.Where(rowsVal => rowsVal != null)?.Count() >= (ComparisonPropertyToColumnsName?.Count ?? 0))
                        {
                            res.Add(rowsVals);
                        }
                    }

                    return res;
                }

                var resCircle = Parallel.ForEach(
                    rowsData,
                    new ParallelOptions { MaxDegreeOfParallelism = ConstantsList.MaxDegreeOfParallelism },
                    rowData =>
                    {
                        var entry = Activator.CreateInstance(TItemType);
                        foreach (var property in new ConcurrentDictionary<PropertyInfo, string>(ComparisonPropertyToColumnsName ?? new()))
                        {
                            var nameAttrValue = property.Value;
                            var isEquelKey = (string key) => new string(key.Where(charValue => !char.IsDigit(charValue)).ToArray()).Equals(nameAttrValue);
                            var key = rowData.Keys.FirstOrDefault(keyValue => isEquelKey(keyValue));
                            var cellValue = rowData.TryGetValue(key, out var val) ? val : string.Empty;

                            if (cellValue == null || string.IsNullOrEmpty(cellValue?.ToString() ?? null)) continue;

                            var cellType = cellValue?.GetType();
                            var propertyType = property.Key.PropertyType;
                            var isNullable = propertyType.IsNullableType();

                            if (isNullable)
                            {
                                propertyType = Nullable.GetUnderlyingType(propertyType);
                            }

                            try
                            {
                                entry = ParseCell(entry: entry, property: property.Key, cellValue: cellValue, cellType: cellType, propertyType: propertyType, isNullable: isNullable);
                            }
                            catch (Exception exc)
                            {
                                exc.LogError(nameof(ParseFile), GetType().FullName);
                                throw;
                            }
                        }

                        temp?.Add((TParsed)entry);
                    });

                if (!resCircle.IsCompleted)
                {
                    $"Цикл не был выполнен до конца (из {rowsData.Count()} итераций выполнено {resCircle.LowestBreakIteration ?? 0}.)"
                        .LogError(GetType().Name, nameof(ProcessingFileEPPlusShort));
                }
                else
                {
                    ItemsToMapping = new(temp?.Where(item => item != default)?.ToList() ?? new());
                }
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(ParseFile), GetType().FullName);
            throw;
        }
    }

    private object ParseCell(object entry, PropertyInfo property, object cellValue, Type cellType, Type propertyType, bool isNullable)
    {
        if (propertyType == typeof(DateTime) && cellType == propertyType && DateTime.TryParse(cellValue.ToString(), out var resTime))
            property.SetValue(entry, resTime, null);
        else if (propertyType == typeof(DateTime) && cellType == typeof(double))
            property.SetValue(entry, DateTime.FromOADate(Convert.ToDouble(cellValue)), null);
        else if (propertyType == typeof(DateTime) && cellType == typeof(string) && double.TryParse(cellValue.ToString(), out var resDate))
            property.SetValue(entry, resDate, null);
        else if (propertyType == typeof(DateTime) && cellType == typeof(string) && !double.TryParse(cellValue.ToString(), out var resDateNotParsed))
            property.SetValue(entry, isNullable ? null : default, null);
        else if (propertyType == typeof(decimal) && decimal.TryParse(cellValue.ToString(), out var resDec))
            property.SetValue(entry, resDec, null);
        else if (propertyType == typeof(decimal) && !decimal.TryParse(cellValue.ToString(), out var resDecNotParsed))
            property.SetValue(entry, isNullable ? null : default, null);
        else if (propertyType == typeof(double) && double.TryParse(cellValue.ToString(), out var resDouble))
            property.SetValue(entry, resDouble, null);
        else if (propertyType == typeof(double) && !double.TryParse(cellValue.ToString(), out var resDoubleNotParsed))
            property.SetValue(entry, isNullable ? null : default, null);
        else if (propertyType == typeof(long) && long.TryParse(cellValue.ToString(), out var resLong))
            property.SetValue(entry, resLong, null);
        else if (propertyType == typeof(long) && !long.TryParse(cellValue.ToString(), out var resLongNotParsed))
            property.SetValue(entry, isNullable ? null : default, null);
        else if (propertyType == typeof(int) && int.TryParse(cellValue.ToString(), out var resInt))
            property.SetValue(entry, resInt, null);
        else if (propertyType == typeof(int) && !int.TryParse(cellValue.ToString(), out var resIntNotParsed))
            property.SetValue(entry, isNullable ? null : default, null);
        else if (propertyType == typeof(short) && short.TryParse(cellValue.ToString(), out var resShort))
            property.SetValue(entry, resShort, null);
        else if (propertyType == typeof(short) && !short.TryParse(cellValue.ToString(), out var resShortNotParsed))
            property.SetValue(entry, isNullable ? null : default, null);
        else if (propertyType == typeof(byte) && byte.TryParse(cellValue.ToString(), out var resByte))
            property.SetValue(entry, resByte, null);
        else if (propertyType == typeof(byte) && !byte.TryParse(cellValue.ToString(), out var resByteNotParsed))
            property.SetValue(entry, isNullable ? null : default, null);
        else if (propertyType == typeof(string))
            property.SetValue(entry, cellValue.ToString(), null);
        else
            property.SetValue(entry, Convert.ChangeType(cellValue, propertyType), null);

        return entry;
    }
}
