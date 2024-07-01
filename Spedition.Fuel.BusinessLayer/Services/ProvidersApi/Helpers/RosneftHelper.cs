using System.Security.Policy;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Factorization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using RestSharp;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Models.Neftika;
using Spedition.Fuel.BusinessLayer.Models.Rosneft;
using Log = Serilog.Log;

namespace Spedition.Fuel.BusinessLayer.Services.ProvidersApi.Helpers;

public static class RosneftHelper
{
    public static string baseUrl = string.Empty;
    
    public static string login = string.Empty;

    public static string password = string.Empty;
    
    public static string contract = string.Empty;

    private static string url = string.Empty;

    public static string Details => $"http-запрос по адресу {url}, {nameof(contract)}: {contract}, {nameof(login)}: {login}, {nameof(password)}: {password}";

    public static RestRequest IncludeAuthToQuiery(RestRequest request)
    {
        var pass = Convert.ToBase64String(Encoding.UTF8.GetBytes(password ?? string.Empty));
        request.AddHeader("RnCard-Identity-Account-Pass", pass);
        return request;
    }

    private static async Task<RestResponse> SendRequest(Method method, List<(string name, string value)> parameters = null)
    {
        RestResponse response;

        var options = new RestClientOptions
        {
            RemoteCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            },
        };

        var client = new RestClient(options);
        var request = new RestRequest(url, method);
        request = IncludeAuthToQuiery(request);
        request.AddQueryParameter("u", login);
        request.AddQueryParameter("p", password);
        request.AddQueryParameter("contract", contract);
        request.AddQueryParameter("type", "json");

        if ((parameters?.Count ?? 0) > 0)
        {
            parameters?.ForEach(parameter => request.AddQueryParameter(parameter.name, parameter.value));
        }

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
            exc.LogError(nameof(TatneftHelper), nameof(SendRequest), $"Подробности: {Details}. ");
            throw;
        }

        return response;
    }

    public static async Task<List<RosneftTransaction>> GetTransactions(string begin, string end)
    {
        url = $"{baseUrl}/GetOperByContract";
        var response = await SendRequest(Method.Get, new List<(string name, string value)> { ("begin", begin), ("end", end), });

        if (response == null)
            return null;

        try
        {
            var transactions = JsonConvert.DeserializeObject<RosneftTransactionData>(response?.Content ?? string.Empty);
            Log.Information($"Получена партия транзакций: {transactions?.OperationList?.Count() ?? 0} шт.");
            return transactions?.OperationList ?? new();
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(RosneftHelper), nameof(GetTransactions),
                $"Ошибка при десериализации ответа в тип {nameof(RosneftTransactionData)}. Подробности: {nameof(begin)}: {begin}, {nameof(end)}: {end}, {Details}. ");
            throw;
        }
    }

    public static async Task<List<RosneftServiceStation>> GetServiceStations()
    {
        List<RosneftServiceStation> azs = new();
        url = $"{baseUrl}/GetPOSList";
        var response = await SendRequest(Method.Get);

        if (response == null)
            return null;

        try
        {
            azs = JsonConvert.DeserializeObject<List<RosneftServiceStation>>(response?.Content ?? string.Empty);
            Log.Information($"Получена партия станций: {azs?.Count() ?? 0} шт.");
            return azs;
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(RosneftHelper), nameof(GetServiceStations),
                $"Ошибка при десериализации ответа в тип {nameof(RosneftServiceStation)}. Подробности: {Details}. ");
            throw;
        }
    }

    public static async Task<List<RosneftServiceStationsCountry>> GetServiceStationsCountries()
    {
        List<RosneftServiceStationsCountry> countries = new();
        url = $"{baseUrl}/GetCountry";
        var response = await SendRequest(Method.Get);

        if (response == null)
            return null;

        try
        {
            countries = JsonConvert.DeserializeObject<RosneftServiceStationsCountries>(response?.Content ?? string.Empty)?.CountryList ?? new();
            Log.Information($"Получена партия стран: {countries?.Count() ?? 0} шт.");
            return countries;
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(RosneftHelper), nameof(GetServiceStationsCountries),
                $"Ошибка при десериализации ответа в тип {nameof(RosneftServiceStationsCountries)}. Подробности: {Details}. ");
            throw;
        }
    }
}
