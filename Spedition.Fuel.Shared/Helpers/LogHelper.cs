namespace Spedition.Fuel.Shared.Helpers;

public static class LogHelper
{
    public static void LogError(this string message, string sourceName, string methodName)
    {
        Log.Error($"При выполнении метода «{methodName ?? string.Empty}» " +
                  $"в классе «{sourceName ?? string.Empty}» " +
                  $"возникла ошибка - {message ?? string.Empty})");
    }

    public static void LogWarning(this string message, string sourceName, string methodName)
    {
        Log.Warning($"При выполнении метода «{methodName ?? string.Empty}» " +
                    $"в классе «{sourceName ?? string.Empty}» " +
                    $"возникла ошибка - {message ?? string.Empty})");
    }

    public static void LogInfo(this string message, string sourceName, string methodName)
    {
        Log.Information($"Метод: «{methodName ?? string.Empty}» " +
                        $"класс: «{sourceName ?? string.Empty}» " +
                        $"подробности: {message ?? string.Empty})");
    }
}
