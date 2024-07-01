using Newtonsoft.Json;
using RestSharp;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Models.E100;
using Spedition.Fuel.BusinessLayer.Models.Neftika;

namespace Spedition.Fuel.BusinessLayer.Services.ProvidersApi.Helpers;

public static class NeftikaHelper
{
    public static string baseUrl;

    public static string login;

    public static string password;

    private static string token;

    private static string url;

    public static string Details => $"http-запрос по адресу {url}, {nameof(login)}: {login}, {nameof(password)}: {password}, {nameof(token)}: {token}";

    internal async static Task<bool> GetAuth()
    {
        url = $"{baseUrl}/token";
        RestResponse response;

        var options = new RestClientOptions(url)
        {
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
        };

        var client = new RestClient(options);
        var request = new RestRequest(url, Method.Post);

        request.AddHeader("Content-Type", "text/plain");
        request.AddBody(
            obj: $"username={login}&password={password}&grant_type=password",
            contentType: "text/plain");

        try
        {
            Log.Information(Details);
            response = await client.ExecuteAsync(request); 
            if (!response.ProcessRestResponse())
            {
                Log.Warning(Details);
                return false;
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(NeftikaHelper), nameof(GetAuth), $"Подробности: {Details}");
            throw;
        }

        try
        {
            var result = JsonConvert.DeserializeObject<NeftikaAuth>(response.Content); 
            Log.Information($"Результат авторизации: {result?.ToString() ?? string.Empty}");
            token = result?.access_token ?? string.Empty;
            return !string.IsNullOrEmpty(token);
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(E100Helper), nameof(GetAuth), $"Ошибка при десериализации ответа в тип {nameof(NeftikaAuth)} Подробности: {Details}");
            throw;
        }
    }

    public static async Task<List<NeftikaTransaction>> GetTransactions()
    {
        url = $"{baseUrl ?? string.Empty}/api/TransactionReport";
        RestResponse response;

        var options = new RestClientOptions(url)
        {
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
        };
        var client = new RestClient(options);
        var request = new RestRequest(url, Method.Get);

        // данные забираем за предыдущие 3 дн.
        var dateFrom = DateTime.Today.AddDays(-3).ToString("yyyy-MM-ddTHH:mm:ss");
        var dateTo = DateTime.Today.AddSeconds(-1).ToString("yyyy-MM-ddTHH:mm:ss");

        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddQueryParameter("dateStart", dateFrom);
        request.AddQueryParameter("dateFinish", dateTo);

        try
        {
            Log.Information(Details);
            response = await client.ExecuteAsync(request);
            if (!response.ProcessRestResponse())
            {
                Log.Warning(Details);
                return null;
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(NeftikaHelper), nameof(GetTransactions), $"Подробности: {Details}. ");
            throw;
        }

        try
        {
            var transactions = JsonConvert.DeserializeObject<List<NeftikaTransaction>>(response?.Content);
            Log.Information($"Получена партия транзакций: {transactions?.Count() ?? 0} шт.");
            return transactions;
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(NeftikaHelper), nameof(GetTransactions),
                $"Ошибка при десериализации ответа в тип {nameof(NeftikaTransaction)}. Подробности: {Details}. ");
            throw;
        }
    }
}
