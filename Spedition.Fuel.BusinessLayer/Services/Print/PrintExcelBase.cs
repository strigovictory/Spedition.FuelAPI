using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using AutoMapper.Internal;
using Microsoft.AspNetCore.Hosting;
using NPOI.SS.Formula.Functions;
using OfficeOpenXml;
using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Services.BaseServices;
using Spedition.Fuel.Client.Helpers;
using Spedition.Fuel.Shared.Interfaces;
using Log = Serilog.Log;

namespace Spedition.Fuel.BusinessLayer.Services.Print;

public abstract class PrintExcelBase<T>
    : FileBase
    where T : class, IPrint
{
    public PrintExcelBase(IWebHostEnvironment env)
       : base(env)
    {
    }

    public abstract UriSegment UriSegment { get; }

    /// <summary>
    /// Метод для генерации файла excel-отчета на основании коллекции T-экземпляров класса.
    /// </summary>
    /// <param Name="items">Коллекция T-экземпляров класса></param>
    /// <returns>Файл отчета в форме массива байт.</returns>
    public async Task<byte[]> GenerateFile(List<T> items)
    {
        NotifyMessage = string.Empty;
        var fileArray = new byte[0];

        // 1 - Скопировать файл шаблона отчета под новым именем во временную папку
        FilePathSource = Path.Combine(Env.WebRootPath, "src/templates", "ExcelReportTemplate.xlsx");

        if (!File.Exists(FilePathSource))
        {
            Log.Error($"Файл «{FilePathSource ?? string.Empty}» не существует! ");
            return fileArray;
        }
           
        DirectoryPathDestination = Path.Combine(Env.WebRootPath, "src/tmp");

        if (!Directory.Exists(DirectoryPathDestination))
        {
            Log.Error($"Путь «{DirectoryPathDestination ?? string.Empty}» не существует! ");
            return fileArray;
        }

        var resCopy = CopySourceToDestination(FilesExtension.xlsx);

        if (!resCopy)
        {
            NotifyMessage += "Произошла ошибка ! Файл не удалось сгенерировать, возможно, шаблон не был найден !";
            return null;
        }

        // 2 - Заполнить xlsx- файл отчета данными
        try
        {
            FillFile(items);
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(GenerateFile), GetType().FullName, $"Errors source: {nameof(FillFile)}");
            throw;
        }

        // 3 - Преобразовать файл отчета в массив байт
        try
        {
            fileArray = await File.ReadAllBytesAsync(FilePathDestination);
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(GenerateFile), GetType().FullName, $"Errors source: {nameof(File.ReadAllBytesAsync)}");
            throw;
        }

        if ((fileArray?.Length ?? 0) > 0)
        {
            NotifyMessage += "Операция по генеарции отчета успешно завершена !";
        }
        else
        {
            NotifyMessage += "Произошла ошибка ! Файл не удалось преобразовать в байтовый массив !";
            NotifyMessage.LogError(GetType().Name, nameof(GenerateFile));
        }

        // 4 - Удалить временный файл отчета из дирректории
        DeleteFiles(false, true);

        return fileArray;
    }

    protected virtual void FillFile(List<T> tItems)
    {
        FileInfo fi = new(FilePathDestination);
        {
            using ExcelPackage excelPackage = new(fi);

            //Создать лист в книге
            var firstWorksheet = excelPackage?.Workbook?.Worksheets[0] ?? excelPackage?.Workbook?.Worksheets[1];

            if (firstWorksheet == null) return;

            firstWorksheet.Protection.IsProtected = false;

            // Диапазон таблицы
            var start = ExcelColumnsNames.B; 
            var item = tItems.FirstOrDefault();
            var itemsType = item.GetType();
            var properties = itemsType?.GetProperties()?.ToList() ?? new();
            var numFinish = (int)start + (properties?.Count ?? 0);
            var finish = Enum.IsDefined(typeof(ExcelColumnsNames), numFinish) ? (ExcelColumnsNames)numFinish : start; // 1-ый столбец - номер п/п

            // Вторая строка - наименование отчета
            firstWorksheet.Cells[$"{start}2:{finish}2"].Merge = true;
            firstWorksheet.Cells[$"{start}2:{finish}2"].Style.Font.Bold = true;
            firstWorksheet.Cells[$"{start}2:{finish}2"].Style.Font.Size = 16;
            firstWorksheet.Cells[$"{start}2:{finish}2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            firstWorksheet.Cells[$"{start}2:{finish}2"].Value = itemsType.GetDisplayAttributeValueName();

            // Задать стили для шапки таблицы
            firstWorksheet.Cells[$"{start}3:{finish}3"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            firstWorksheet.Cells[$"{start}3:{finish}3"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(83, 105, 83));

            //Задать рамку вокруг шапки таблицы
            firstWorksheet.Cells[$"{start}3:{finish}3"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
            firstWorksheet.Cells[$"{start}3:{finish}3"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
            firstWorksheet.Cells[$"{start}3:{finish}3"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
            firstWorksheet.Cells[$"{start}3:{finish}3"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;

            // Заполнение шапки таблицы
            firstWorksheet.Cells[$"{start}3"].Value = "№ п/п";

            for (var i = 0; i < properties.Count; i++)
            {
                var prop = properties.ElementAt(i);
                var displayVal = prop.GetRequiredAttributeValue<T, DisplayNameAttribute>(DisplayNameAttribute => DisplayNameAttribute.DisplayName);
                firstWorksheet.Cells[$"{start + i + 1}3"].Value = displayVal;
            }

            // Индекс строки 
            var rowNum = 3;

            // Номер порядковый
            var numPP = 0;

            // Переменные, хранящие данные
            var date = string.Empty;
            var user = string.Empty;

            // Построчное заполнение данными
            foreach (var tItem in tItems)
            {
                numPP++;
                rowNum++;

                // Заполнить ячейки строки данными
                firstWorksheet.Cells[string.Concat($"{start}", rowNum)].Value = numPP;

                for (var i = 0; i < properties.Count; i++)
                {
                    var prop = properties.ElementAt(i);

                    object propValue = default;

                    var propType = prop.PropertyType;

                    if (propType.IsNullableType())
                        propType = Nullable.GetUnderlyingType(propType);

                    if (propType == typeof(bool))
                    {
                        propValue = bool.TryParse((prop?.GetValue(tItem) ?? default)?.ToString(), out var val) ? val ? "+": "-" : "-";
                    }
                    else if (propType == typeof(DateTime))
                    {
                        propValue = DateTime.TryParse((prop?.GetValue(tItem) ?? default)?.ToString(), out var val) ? val.FormatDateTime() : string.Empty;
                    }
                    else
                    {
                        propValue = prop?.GetValue(tItem) ?? default;
                    }

                    firstWorksheet.Cells[string.Concat($"{(ExcelColumnsNames)((int)start + i + 1)}", rowNum)].Value = propValue;
                }

                //Задать рамку вокруг строки
                firstWorksheet.Cells[string.Concat($"{start}", rowNum, ':', $"{finish}", rowNum)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                firstWorksheet.Cells[string.Concat($"{start}", rowNum, ':', $"{finish}", rowNum)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
                firstWorksheet.Cells[string.Concat($"{start}", rowNum, ':', $"{finish}", rowNum)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
                firstWorksheet.Cells[string.Concat($"{start}", rowNum, ':', $"{finish}", rowNum)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                // Задать выравнивание по горизонтали в строке
                firstWorksheet.Cells[string.Concat($"{start}", rowNum, ':', $"{finish}", rowNum)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // Если строка последняя сделать нижнюю границу жирной
                if (numPP == (tItems?.Count ?? 0))
                {
                    firstWorksheet.Cells[string.Concat($"{start}", rowNum, ':', $"{finish}", rowNum)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
                }
            }

            // Сохранить сгенерированные в файле данные
            excelPackage.Save();
        }
    }
}
