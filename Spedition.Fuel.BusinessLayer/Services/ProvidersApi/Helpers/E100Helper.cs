using System.Security.Policy;
using Newtonsoft.Json;
using RestSharp;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Models.E100;
using Spedition.Fuel.BusinessLayer.Models.Tatneft;

namespace Spedition.Fuel.BusinessLayer.Services.ProvidersApi.Helpers;

public class E100Helper
{
    public static string baseUrl;

    public static string login;

    public static string password;

    private static string token;

    private static string url { get; set; }

    public static string Details => $"http-запрос по адресу {url}, {nameof(login)}: {login}, {nameof(password)}: {password}, {nameof(token)}: {token}";

    public async static Task<bool> GetAuth()
    {
        url = $"{baseUrl}/token"; 
        RestResponse response;

        var restClientOptions = new RestClientOptions
        {
            RemoteCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) => true,
        };

        var client = new RestClient(restClientOptions);
        var request = new RestRequest(url, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddBody(
            obj: new
            {
                grant_type = "password",
                username = login,
                password = password,
            },
            contentType: "application/json");

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
            exc.LogError(nameof(E100Helper), nameof(GetAuth), $"Подробности: {Details}. ");
            throw;
        }

        try
        {
            token = JsonConvert.DeserializeObject<E100AuthData>(response.Content)?.data?.access_token ?? string.Empty;
            return !string.IsNullOrEmpty(token);
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(E100Helper), nameof(GetAuth), $"Подробности: ошибка при десериализации ответа в тип {nameof(E100AuthData)}, Подробности: {Details}. ");
            throw;
        }
    }

    internal static async Task<E100TransactionData> GetTransactions(int pagenum = 1)
    {
        url = $"{baseUrl}/transactions";
        RestResponse response;

        var restClientOptions = new RestClientOptions
        {
            RemoteCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            },
            MaxTimeout = 360000, // 6 min
        };

        var client = new RestClient(restClientOptions);
        var request = new RestRequest(url, Method.Post);

        var dateFrom = DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd");
        var dateTo = DateTime.Today.ToString("yyyy-MM-dd");

        request.AddHeader("Access-token", token);
        request.AddHeader("Content-Type", "application/json");
        request.AddParameter(
            "application/json",
            $"{{\"from\": \"{dateFrom}\", \"to\":\"{dateTo}\",\"page_size\": 2000,\"page_number\":{pagenum}}}",
            ParameterType.RequestBody);

        // Получение коллекции транзакций за период
        try
        {
            Log.Information(Details);

            // необходимо вставить задержку по времени, т.к. у АПИ Е100 стоит ограничение по кол-ву запросов в единицу времени,
            // и если не поставить задержку менее 5 сек. будет приходить Код ошибки: TooManyRequests.
            Task.Delay(5000)?.Wait();

            response = await client.ExecuteAsync(request);
            if (!response.ProcessRestResponse())
            {
                Log.Warning(Details);
                return null;
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(E100Helper), nameof(GetTransactions), $"Подробности: page = {pagenum}, {Details}. ");
            throw;
        }

        // Десериализация транзакций
        try
        {
            var transactions = JsonConvert.DeserializeObject<E100TransactionData>(response.Content);
            Log.Information($"Получена партия транзакций: {transactions?.Transactions?.Count() ?? 0} шт. " +
                            $"Page_number = {transactions?.Page_number.ToString() ?? string.Empty}, " +
                            $"Page_count = { transactions?.Page_count.ToString() ?? string.Empty}. " +
                            $"Page_size = {transactions?.Page_size.ToString() ?? string.Empty}. ");
            return transactions;
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(E100Helper), nameof(GetTransactions),
                $"Ошибка при десериализации ответа в тип {nameof(E100TransactionData)}. Подробности: page = {pagenum}, {Details}. ");
            throw;
        }
    }
}
