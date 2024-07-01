using System;
using System.Net.Http;
using System.Security.Policy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Ocsp;
using RestSharp;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Models.Tatneft;

namespace Spedition.Fuel.BusinessLayer.Services.ProvidersApi.Helpers;

public static class TatneftHelper
{
    public static string baseUrl;

    /// <summary>
    /// Публичный ключ.
    /// </summary>
    public static string appId;

    /// <summary>
    /// Секретный ключ.
    /// </summary>
    public static string privateKey;

    /// <summary>
    /// Общее число записей.
    /// </summary>
    private static int total;

    /// <summary>
    /// Максимальное число записей в одном запросе.
    /// </summary>
    private static int limit;

    /// <summary>
    /// Кол-во записей, которое нужно пропустить.
    /// </summary>
    private static int offset;

    /// <summary>
    /// число уже сохраненных в БД транзакций по заданному провайдеру и подразделению.
    /// </summary>
    public static int dbTransactionsCount;

    private static string Details => 
        $"http-запрос по адресу {baseUrl}, " +
        $"«{nameof(total)}»: {total}, " +
        $"«{nameof(limit)}»: {limit}, " +
        $"«{nameof(offset)}»: {offset}, " +
        $"«{nameof(appId)}»: {appId}, " +
        $"«{nameof(privateKey)}»: {privateKey},";

    private static RestRequest IncludeAuthToQuiery(RestRequest request)
    {
        request.RequestFormat = DataFormat.Json;
        request.AddQueryParameter("public_id", appId);
        request.AddQueryParameter("private_key", privateKey);
        return request;
    }

    private static async Task<RestResponse> SendRequest()
    {
        RestResponse response;

        var restClientOptions = new RestClientOptions
        {
            RemoteCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            },
        };

        var client = new RestClient(restClientOptions);
        var request = new RestRequest(baseUrl, Method.Get);
        request = IncludeAuthToQuiery(request);
        request.AddQueryParameter("method", "getTransactions");
        request.AddQueryParameter("offset", offset);
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

    public static async Task<List<TatneftTransaction>> GetTransactions()
    {
        List<TatneftTransaction> transactions = new();

        // 1 - запрос первый для получения общего числа записей
        var responseFirst = await SendRequest();
        if (responseFirst == null)
        {
            return null;
        }

        try
        {
            var responseDeserializedFirst = JsonConvert.DeserializeObject<TatneftTransactionData>(responseFirst?.Content ?? string.Empty);
            total = responseDeserializedFirst?.total ?? 0;
            limit = responseDeserializedFirst?.limit ?? 0;

            if ((responseDeserializedFirst?.response?.Count ?? 0) > 0)
                transactions?.AddRange(responseDeserializedFirst.response);

            Log.Information($"GetDivisions {nameof(total)}: {total}, {nameof(limit)}: {limit}. ");
            if (total < 1 || limit < 1)
            {
                Details.LogWarning(nameof(TatneftHelper), nameof(GetTransactions));
                return null;
            }

            offset = total > dbTransactionsCount ? total - dbTransactionsCount : 0;

            // всегда проверяется 2000 записей дополнительно из-за большого числа дубликатов в ответе
            offset -= offset > 2000 ? 2000 : offset;
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(TatneftHelper), nameof(GetTransactions), $"Ошибка при десериализации ответа в тип {nameof(TatneftTransactionData)}. Подробности: {Details}. ");
            throw;
        }

        // 2 - последующие запросы для получения списка транзакций
        while (offset >= 0 && offset < total)
        {
            var response = await SendRequest();
            if (response == null)
            {
                return null;
            }

            try
            {
                var responseDeserialized = JsonConvert.DeserializeObject<TatneftTransactionData>(response?.Content ?? string.Empty);
                Log.Information($"Получена партия транзакций: {responseDeserialized?.response?.Count ?? 0}. Подробности: {Details}. ");

                if ((responseDeserialized?.response?.Count ?? 0) > 0)
                    transactions?.AddRange(responseDeserialized.response);
            }
            catch (Exception exc)
            {
                exc.LogError(nameof(TatneftHelper), nameof(GetTransactions), 
                    $"Ошибка при десериализации ответа в тип {nameof(TatneftTransactionData)}. Подробности: {Details}. ");
                throw;
            }

            offset+=limit;
        }

        return transactions;
    }
}
