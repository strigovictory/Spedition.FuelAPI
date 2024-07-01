namespace Spedition.Fuel.Shared.Helpers;

public static class NumbersHelper
{
    public static string FormatDecimalToString(this decimal? number, int digits = 2)
    {
        var expression = "{" + $"0:f{digits}" + "}";
        return number.HasValue ? string.Format(expression, number) : "0";
    }

    public static string FormatDecimalToString(this decimal number, int digits = 2)
    {
        var expression = "{" + $"0:f{digits}" + "}";
        return string.Format(expression, number);
    }

    public static decimal FormatStringToDecimal(this object num)
    {
        var decimalData = 0M;

        var temp = num?.ToString() ?? string.Empty;

        if (decimal.TryParse(temp, out decimal resPars1))
        {
            decimalData = resPars1;
        }
        else
        {
            if (temp.Contains(','))
            {
                temp = temp.Replace(',', '.');
            }
            if (decimal.TryParse(temp, out decimal resPars2))
            {
                decimalData = resPars2;
            }
        }

        return decimalData;
    }

    /// <summary>
    /// Метод для конвертации числа положительного в отрицательное и наоборот.
    /// </summary>
    /// <param Name="number">Число, знак которого подлежит конвертации.</param>
    /// <returns>Число с конвертированным знаком.</returns>
    public static decimal ConvertNegativePositive(this decimal number)
    {
        if (number < 0)
        {
            return Math.Abs(number);
        }
        else if (number > 0)
        {
            if (decimal.TryParse(string.Concat("-", number.ToString()), out var result))
            {
                return result;
            }
            else
            {
                return number;
            }
        }
        else
        {
            return 0;
        }
    }
}
