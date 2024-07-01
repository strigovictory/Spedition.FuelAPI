using System.Net;
using Microsoft.Extensions.Options;
using Serilog;
using Spedition.Fuel.Shared.Settings.Configs;

namespace Spedition.FuelAPI.Middlewares
{
    /// <summary>
    /// Provides middleware.
    /// </summary>
    public class ApiKeyMiddleware
    {
        private const string ApiKeyHeaderName = "x-api-key";

        private readonly RequestDelegate next;
        private readonly ApiConfigs apiConfigs;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyMiddleware"/> class.
        /// </summary>
        /// <param Name="apiConfigs">API key configs options.</param>
        /// <param Name="next">Next step to provide request.</param>
        public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiConfigs> apiConfigs)
        {
            this.next = next;
            this.apiConfigs = apiConfigs.Value;
        }

        /// <summary>
        ///  Provides handler for check API key.
        /// </summary>
        /// <param Name="context">Context from request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey(ApiKeyHeaderName))
            {
                if (!string.IsNullOrEmpty(apiConfigs.Key) && context.Request.Headers[ApiKeyHeaderName] == apiConfigs.Key)
                {
                    await next.Invoke(context);
                }
                else
                {
                    Log.Warning($"Access denied: incorrect {ApiKeyHeaderName}.");
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Unauthorized");
                }
            }
            else
            {
                Log.Warning($"Access denied: no {ApiKeyHeaderName} header.");
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("Forbidden");
            }
        }
    }
}
