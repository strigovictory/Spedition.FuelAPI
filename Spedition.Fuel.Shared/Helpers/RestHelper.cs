using System.Runtime.CompilerServices;
using RestSharp;
using static System.Net.WebRequestMethods;

namespace Spedition.Fuel.Shared.Helpers;

public static class RestHelper
{
    public static bool ProcessRestResponse(this RestResponse response)
    {
        var isSuccess = false;
        if (response == null)
        {
            Log.Warning($"Пустой ответ.");
        }
        else if(!response.IsSuccessful)
        {
            Log.Warning($"Неудачный запрос по адресу: {response?.ResponseUri?.ToString() ?? string.Empty}. " +
                        $"Подробности: код ошибки «{response?.StatusCode.ToString() ?? string.Empty} - {response?.StatusDescription ?? string.Empty}», " +
                        $"сообщение: «{response?.ErrorMessage?.ToString() ?? string.Empty}. " +
                        $"{response?.ErrorException?.Message ?? string.Empty}. " +
                        $"{response?.ErrorException?.InnerException?.Message ?? string.Empty}.» ");
        }
        else if(response.IsSuccessful)
        {
            if ((response.Content?.Length ?? 0) == 0)
            {
                Log.Warning($"Контент пустой.");
            }
            else if (response.Content.Contains("error", StringComparison.InvariantCultureIgnoreCase))
            {
                Log.Warning($"Ошибка в запросе. Подробности: {response?.Content?.ToString() ?? string.Empty}.");
            }
            else
            {
                isSuccess = true;
            }
        }

        return isSuccess;
    }
}
