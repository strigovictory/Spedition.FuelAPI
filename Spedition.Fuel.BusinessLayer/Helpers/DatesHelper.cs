using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Hosting;
using NPOI.SS.Formula.Functions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Log = Serilog.Log;

namespace Spedition.Fuel.BusinessLayer.Helpers;

public static class DatesHelper
{
    /// <summary>
    /// метод для сверки на идентичность двух дат без учета миллисекунд.
    /// </summary>
    /// <param Name="first">Первая дата.</param>
    /// <param Name="second">Вторая дата.</param>
    /// <returns>истина, если даты равны и наоборот.</returns>
    public static bool IsDatesEquel(this DateTime first, DateTime second)
    {
        return first.Day == second.Day
            && first.Month == second.Month
            && first.Year == second.Year
            && first.Hour == second.Hour
            && first.Minute == second.Minute
            && first.Second == second.Second;
    }

    public static bool IsDatesEquel(this DateTime? first, DateTime? second)
    {
        return (!first.HasValue && !second.HasValue)
            || (first.HasValue && second.HasValue
            && first.Value.Day == second.Value.Day
            && first.Value.Month == second.Value.Month
            && first.Value.Year == second.Value.Year
            && first.Value.Hour == second.Value.Hour
            && first.Value.Minute == second.Value.Minute
            && first.Value.Second == second.Value.Second);
    }

    public static string FormatDateTime(this DateTime date)
    {
        return string.Concat(
            date.Year.AddzeroForYear(),
            "/",
            date.Month.Addzero(),
            "/",
            date.Day.Addzero(),
            " ",
            date.Hour.Addzero(),
            ":",
            date.Minute.Addzero(),
            ":",
            date.Second.Addzero());
    }

    /// <summary>
    /// Метод форматирует число - добавляет ноль впереди числа, если оно меньше десяти.
    /// </summary>
    /// <param Name="datePart">Число, подлежащее форматированию.</param>
    /// <returns>Строковое представление числа.</returns>
    public static string Addzero(this int datePart)
    {
        return (datePart < 10) ? "0" + datePart.ToString() : datePart.ToString();
    }

    /// <summary>
    /// Метод форматирует число - добавляет необходимое число нолей впереди числа.
    /// </summary>
    /// <param Name="datePart">Число, подлежащее форматированию.</param>
    /// <returns>отформатированная строка.</returns>
    public static string AddzeroForYear(this int datePart)
    {
        string result = string.Empty;

        if (datePart < 10)
        {
            result = string.Concat("200", datePart.ToString());
        }
        else if (datePart < 100)
        {
            result = string.Concat("20", datePart.ToString());
        }
        else if (datePart < 1000)
        {
            result = string.Concat("2", datePart.ToString());
        }
        else
        {
            result = datePart.ToString();
        }

        return result;
    }

    public static DateTime FormatToDateTime(this object date, IWebHostEnvironment env)
    {
        var dateString = date?.ToString() ?? string.Empty;

        // 1 - Первый способ преобразовать строку в дату - автоматический
        ReadOnlySpan<char> dateCharArr = new(dateString.ToCharArray());
        if (DateTime.TryParse(dateCharArr, out DateTime resultByCharArray))
        {
            return resultByCharArray;
        }

        // 2 - Первый способ преобразовать строку в дату - мануальный
        var firstDot = dateString.IndexOf('.');
        var secondDot = dateString.LastIndexOf('.');

        if (firstDot == -1 || secondDot == -1)
        {
            return default;
        }

        var day = int.Parse(dateString.Substring(firstDot - 2, 2)).Addzero();
        var month = int.Parse(dateString.Substring(firstDot + 1, 2)).Addzero();
        var year = int.Parse(dateString.Substring(secondDot + 1, 4)).AddzeroForYear();
        var hour = "0";
        var min = "0";
        var sec = "0";

        var firstColon = dateString.IndexOf(':');
        var secondColon = dateString.LastIndexOf(':');

        if(firstColon != -1 && secondColon != -1)
        {
            hour = int.TryParse(dateString.Substring(firstColon - 2, 2), out var hourValue) ? hourValue.Addzero() : "0";
            min = int.TryParse(dateString.Substring(secondColon - 2, 2), out var minValue) ? minValue.Addzero() : "0";
            sec = int.TryParse(dateString.Substring(secondColon + 1, 2), out var secValue) ? secValue.Addzero() : "0";
        }

        string resultString = new(string.Empty);

        if (env.IsDevelopment() || env.IsProduction())
        {
            // Docker
            resultString = string.Concat(month, '/', day, '/', year, ' ', hour, ':', min, ':', sec);
        }
        else
        {
            // IIS , Kestrel
            resultString = string.Concat(day, '/', month, '/', year, ' ', hour, ':', min, ':', sec);
        }

        if (DateTime.TryParse(resultString, out DateTime res))
        {
            Log.Error($"Невозможно преобразовать строку «{date?.ToString() ?? string.Empty}» в тип DateTime.");
            return res;
        }
        else
        {
            return default;
        }
    }

    public static DateTime GetDateTimeFromString(this string dateSource) // '1.06.23'
    {
        var firstDot = dateSource.IndexOf('.');
        var secondDot = dateSource.LastIndexOf('.');

        if (firstDot == -1 || secondDot == -1)
        {
            return default;
        }

        var day = int.Parse(dateSource.Substring(0, firstDot));
        var month = int.Parse(dateSource.Substring(firstDot + 1, secondDot - firstDot - 1));
        var yearString = int.Parse(dateSource.Substring(secondDot + 1, dateSource.Length - secondDot - 1)).AddzeroForYear();

        if(int.TryParse(yearString, out var year))
        {
            return new DateTime(day: day, month: month, year: year);
        }
        else
        {
            return default;
        }
    }
}
