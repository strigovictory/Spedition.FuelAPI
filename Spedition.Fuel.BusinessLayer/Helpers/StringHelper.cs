namespace Spedition.Fuel.BusinessLayer.Helpers;

public static class StringHelper
{
    public static string TrimWhiteSpaces(this string source)
    {
        return source.Trim(' ');
    }

    public static string TrimSomeChars(this string source, List<char> chars)
    {
        if (string.IsNullOrEmpty(source))
        {
            return null;
        }

        foreach (var charItem in chars)
        {
            var ind = source.IndexOf(charItem);

            while (ind != -1)
            {
                source = source.Remove(ind, 1);
                ind = source.IndexOf(charItem);
            }
        }

        return source.ToUpper();
    }

    public static string ConvertCyrillicToLatin(this string source)
    {
        var result = source ?? string.Empty;

        var ruEnChars = new Dictionary<char, char>()
        {
            { 'А', 'A' },
            { 'В', 'B' },
            { 'С', 'C' },
            { 'Е', 'E' },
            { 'Н', 'H' },
            { 'К', 'K' },
            { 'М', 'M' },
            { 'О', 'O' },
            { 'Р', 'P' },
            { 'Т', 'T' },
            { 'У', 'Y' },
            { 'Х', 'X' },
        };

        source?.ToUpper()?.ToList()?.ForEach(charValue =>
        result = result.Replace(charValue, ruEnChars.ContainsKey(charValue) ? ruEnChars[charValue] : charValue));

        return result;
    }

    public static string TrimSomeCharsExceptLastSixth(this string source, List<char> chars)
    {
        var res = string.Empty;

        if (string.IsNullOrEmpty(source))
        {
            return res;
        }

        var lenth = source.Length;
        if (lenth < 7)
        {
            return source;
        }

        var sixthCharIndexFromEnd = lenth - 6 - 1;

        foreach (var charItem in chars)
        {
            for (var i = 0; i <= sixthCharIndexFromEnd; i++)
            {
                if (source.ElementAt(i) != charItem)
                {
                    res += source.ElementAt(i);
                }
            }
        }

        res += source.Substring(sixthCharIndexFromEnd + 1, 6); // из последних 6-х циф номера не обрезаем символы
        return res.ToUpper();
    }

    public static string AddToSixDigits(this string number)
    {
        var result = number ?? string.Empty;

        while (result.Length < 6)
            result = "0" + result;

        return result;
    }
}
