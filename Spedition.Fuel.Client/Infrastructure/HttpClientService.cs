using System.Runtime;
using Spedition.Fuel.Shared.Settings.Configs;

namespace Spedition.Fuel.Client.Infrastructure
{
    public sealed class HttpClientService : IHttpClientService
    {
        private readonly IHttpClientFactory httpClientFactory;

        public HttpClientService(IHttpClientFactory httpClientFactory) => this.httpClientFactory = httpClientFactory;

        private HttpClient HttpClient =>
            httpClientFactory?.CreateClient("fuel-api")
            ?? new HttpClient();

        public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(
            string url,
            HttpMethod httpMethod,
            TRequest content,
            CancellationToken token = default,
            [CallerMemberName] string callerName = default)
        {
            var request = url.GetRequestMessage(httpMethod, content);
            var response = await HttpClient?.SendRequest(request, token);
            if(response != default && response.IsSuccessStatusCode)
            {
                return await response.DeserializeTResponse<TResponse>();
            }
            else
            {
                return default;
            }
        }

        public async Task SendRequestAsync<TRequest>(
            string url,
            HttpMethod httpMethod,
            TRequest content,
            CancellationToken token = default,
            [CallerMemberName] string callerName = default)
        {
            var request = url.GetRequestMessage(httpMethod, content);
            var response = await HttpClient?.SendRequest(request, token);
        }

        public async Task<TResponse> SendRequestAsync<TResponse>(
            string url,
            HttpMethod httpMethod,
            CancellationToken token = default,
            [CallerMemberName] string callerName = null)
        {
            var request = url.GetRequestMessage(httpMethod);
            var response = await HttpClient?.SendRequest(request, token);
            if (response != default && response.IsSuccessStatusCode)
            {
                return await response.DeserializeTResponse<TResponse>();
            }
            else
            {
                return default;
            }
        }
    }
}
