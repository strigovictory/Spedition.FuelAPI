namespace Spedition.Fuel.Client.Infrastructure.Interfaces
{
    public interface IHttpClientService
    {
        Task<TResponse> SendRequestAsync<TRequest, TResponse>(
            string url,
            HttpMethod httpMethod,
            TRequest content,
            CancellationToken token = default,
            [CallerMemberName] string callerName = default);

        Task SendRequestAsync<TRequest>(
            string url,
            HttpMethod httpMethod,
            TRequest content,
            CancellationToken token = default,
            [CallerMemberName] string callerName = default);

        Task<TResponse> SendRequestAsync<TResponse>(
            string url,
            HttpMethod httpMethod,
            CancellationToken token = default,
            [CallerMemberName] string callerName = default);
    }
}
