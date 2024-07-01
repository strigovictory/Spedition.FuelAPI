using System;
using System.Diagnostics.Contracts;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Serilog;
using Spedition.Fuel.Shared.Settings.Configs;

namespace Spedition.Fuel.BFF.Retrievers;

public abstract class RetrieverBase
{
    protected readonly IMapper mapper;
    protected readonly string apiKey;
    private readonly string apiVersion;
    private readonly Uri baseUri;

    public RetrieverBase(IOptions<ApiConfigs> config, IMapper mapper)
    {
        this.mapper = mapper;
        apiVersion = config.Value.Version;
        apiKey = config.Value.Key;
        baseUri = new Uri(config.Value.BaseUrl);
    }

    protected abstract string UriDomen { get; }

    protected string UriSegment { get; set; } = string.Empty;

    protected string Uri => $"api/v{apiVersion}/{UriDomen}/{UriSegment}";

    public async Task<T> SendRequest<T>(int? id = null, CancellationToken token = default)
    where T : new()
    {
        RestResponse response;
        var details = $"http-запрос по адресу {baseUri}{Uri}, {nameof(apiKey)}: {apiKey}";

        var restClientOptions = new RestClientOptions
        {
            BaseUrl = baseUri,
            RemoteCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) => true,
        };

        var client = new RestClient(restClientOptions);
        var request = new RestRequest(Uri, Method.Get);
        request.AddHeader("x-api-key", apiKey);

        if (id != null)
        {
            request.AddParameter("id", id.Value);
        }

        try
        {
            Log.Information(details);
            response = await client.ExecuteAsync(request, token);
            if (!response.ProcessRestResponse())
            {
                Log.Warning(details);
                return default;
            }
        }
        catch (Exception exc)
        {
            exc.LogError(
                nameof(RetrieverBase),
                nameof(SendRequest),
                $"Ошибка при выполнении запроса. " + details);
            throw;
        }

        try
        {
            var result = JsonConvert.DeserializeObject<T>(response.Content);
            if (result == null)
            {
                $"{details} Ошибка десериализации. Не удалось десериализовать контент в тип {typeof(T).Name}"
                    .LogError(nameof(RetrieverBase), nameof(SendRequest));
                return default;
            }

            return result;
        }
        catch (Exception exc)
        {
            exc.LogError(
                nameof(RetrieverBase),
                nameof(SendRequest),
                $"Ошибка при десериализации ответа в тип {nameof(T)}. " + details);
            throw;
        }
    }
}
