using System.Security.Policy;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Factorization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using RestSharp;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Models.E100;
using Spedition.Fuel.BusinessLayer.Models.GazProm;
using Spedition.Fuel.BusinessLayer.Models.GazProm.Enums;
using Spedition.Fuel.BusinessLayer.Models.GazProm.Interfaces;
using Spedition.Fuel.BusinessLayer.Models.GPN;
using Spedition.Fuel.BusinessLayer.Models.Neftika;
using Spedition.Fuel.BusinessLayer.Models.Rosneft;
using Log = Serilog.Log;

namespace Spedition.Fuel.BusinessLayer.Services.ProvidersApi.Helpers;

public static class GPNHelper
{
    public static string baseUrl = string.Empty;

    public static string login = string.Empty;

    public static string password = string.Empty;

    public static string apiKey = string.Empty;

    public static string sessionId = string.Empty;

    private static string url = string.Empty;

    public static string contractId = string.Empty;

    public static string Details =>
        $"http-запрос по адресу {url}, " +
        $"{nameof(login)}: {login}, " +
        $"{nameof(password)}: {password}, " +
        $"{nameof(apiKey)}: {apiKey}, " +
        $"{nameof(sessionId)}: {sessionId}, " +
        $"{nameof(contractId)}: {contractId}";

    public static async Task<bool> GetAuth()
    {
        url = $"{baseUrl}/authUser";
        RestResponse response;
        var restClientOptions = new RestClientOptions
        {
            RemoteCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) => true,
        };
        var client = new RestClient(restClientOptions);
        var request = new RestRequest(url, Method.Post);
        request.AddHeader("api_key", apiKey);
        request.AddHeader("date_time", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddParameter("login", login);
        request.AddParameter("password", password);

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
            exc.LogError(nameof(GPNHelper), nameof(GetAuth), $"Подробности: {Details}. ");
            throw;
        }

        try
        {
            var result = JsonConvert.DeserializeObject<GPNDataBaseResponse<GPNAuthData>>(response.Content);
            if ((result?.Status?.Code ?? 0) != 200)
            {
                var errors = string.Empty;
                result.Status.Errors?.ToList().ForEach(error => errors += error);
                $"{Details}, errors: {errors}"
                    .LogWarning(nameof(GPNHelper), nameof(GetAuth));
                return false;
            }

            sessionId = result?.Data?.Session_id ?? string.Empty;
            contractId = result?.Data?.Contracts?.FirstOrDefault(_ => _.Cards_count > 0)?.Id ?? string.Empty;
            Log.Information($"Результат авторизации: " +
                            $"код ответа = {result?.Status?.Code ?? 0}, {result?.Status?.Errors} " +
                            $"{nameof(result.Data.Client_id)} = {result.Data.Client_id}, " +
                            $"{nameof(contractId)} = {contractId}, " +
                            $"{nameof(result.Data.Session_id)} = {result.Data.Session_id}");
            return true;
        }
        catch (Exception exc)
        {
            exc.LogError(
                nameof(GPNHelper),
                nameof(SendRequest),
                $"Ошибка при десериализации ответа в тип {nameof(GPNDataBaseResponse<GPNAuthData>)}. Подробности: {Details}. ");
            throw;
        }
    }

    public static async Task<T> SendRequest<T>(string cardid, int? count, bool isContractInclude) 
        where T : IGPNRequestBase, new()
    {
        url = $"{baseUrl}/{new T().Url ?? string.Empty}";
        var details = nameof(GPNHelper) 
            + nameof(SendRequest) 
            + $"{Details}. Подробности: {nameof(cardid)}: {cardid}, {nameof(count)}: {count ?? 0}. ";
        RestResponse response;
        var restClientOptions = new RestClientOptions
        {
            RemoteCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) => true,
        };
        var client = new RestClient(restClientOptions);
        var request = new RestRequest(url, Method.Get);
        request.AddHeader("api_key", apiKey);
        request.AddHeader("date_time", DateTime.Now.ToString());
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddHeader("session_id", sessionId);

        if (count != null)
            request.AddParameter(new T().CountName, count ?? 0);

        if (isContractInclude)
            request.AddParameter(new T().ContractId, contractId);

        if (!string.IsNullOrEmpty(cardid))
            request.AddParameter(new T().CardName, cardid);

        try
        {
            Log.Information(Details);
            response = await client.ExecuteAsync(request); 
            if (!response.ProcessRestResponse())
            {
                Log.Warning(Details);
                return default(T);
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(GPNHelper), nameof(SendRequest), details);
            throw;
        }

        try
        {
            var result = JsonConvert.DeserializeObject<GPNDataBaseResponse<T>>(response.Content);
            Log.Information($"Код ответа: {result?.Status?.Code ?? 0}. {details}. ");
            if ((result?.Status?.Code ?? 0) != 200)
            {
                var errors = string.Empty;
                result.Status.Errors?.ToList().ForEach(error => errors += error + " ");
                $"{details} Errors: {errors}".LogWarning(nameof(GPNHelper), nameof(SendRequest));
                return default(T);
            }

            return result.Data;
        }
        catch (Exception exc)
        {
            exc.LogError(
                nameof(GPNHelper), 
                nameof(SendRequest),
                $"Ошибка при десериализации ответа в тип {nameof(GPNDataBaseResponse<T>)}. " + details);
            throw;
        }
    }

    public static async Task<GPNParameterBase> GetParametersValues(GPNParameters parameter)
    {
        url = $"{baseUrl}/getDictionary?Name={parameter}";
        RestResponse response;
        var restClientOptions = new RestClientOptions
        {
            RemoteCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) => true,
        };
        var client = new RestClient(restClientOptions);
        var request = new RestRequest(url, Method.Get);
        request.AddHeader("api_key", apiKey);
        request.AddHeader("date_time", DateTime.Now.ToString());
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddHeader("session_id", sessionId);
        request.AddParameter("Name", parameter);

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
            exc.LogError(nameof(GPNHelper), nameof(SendRequest), $"Подробности: {Details}. ");
            throw;
        }

        try
        {
            var result = JsonConvert.DeserializeObject<GPNDataBaseResponse<GPNParameterBase>>(response.Content);
            Log.Information($"Код ответа: {result?.Status?.Code ?? 0}. {Details}. ");
            if ((result?.Status?.Code ?? 0) != 200)
            {
                var errors = string.Empty;
                result.Status.Errors?.ToList().ForEach(error => errors += error + " ");
                $"{Details}. Errors: {errors}".LogWarning(nameof(GPNHelper), nameof(GetParametersValues));
                return null;
            }

            return result.Data;
        }
        catch (Exception exc)
        {
            exc.LogError(
                nameof(GPNHelper),
                nameof(SendRequest),
                $"Ошибка при десериализации ответа в тип {nameof(GPNDataBaseResponse<GPNParameterBase>)}. Подробности: {Details}. ");
            throw;
        }
    }
}
