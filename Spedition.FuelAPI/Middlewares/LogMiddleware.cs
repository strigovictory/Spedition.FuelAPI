using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog.Context;
using Spedition.FuelAPI.Accessors.Interfaces;

namespace Spedition.FuelAPI.Middlewares
{
    /// <summary>
    /// Logging middleware.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LogMiddleware
    {
        private const string TimeFormat = @"hh\:mm\:ss\:fff";
        private const string Version = "Version";
        private const string StatusCodeName = "StatusCode";
        private const string CorrelationId = "CorrelationId";

        private readonly RequestDelegate next;
        private readonly ILogger<LogMiddleware> logger;
        private readonly ICorrelationIdAccessor correlationIdAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMiddleware"/> class.
        /// </summary>
        /// <param Name="next">Next step to provide request.</param>
        /// <param Name="logger">Logger.</param>
        /// <param Name="correlationIdAccessor">Correlation id accessor.</param>
        public LogMiddleware(RequestDelegate next, ILogger<LogMiddleware> logger, ICorrelationIdAccessor correlationIdAccessor)
        {
            this.next = next;
            this.logger = logger;
            this.correlationIdAccessor = correlationIdAccessor;
        }

        /// <summary>
        /// Provides handler for logging.
        /// </summary>
        /// <param Name="context">Context from request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            PushValueIntoContext(context, Version, "1.0");
            PushValueIntoContext(context, CorrelationId, correlationIdAccessor.GetCorrelationId());

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                await next.Invoke(context);
            }
            finally
            {
                stopWatch.Stop();
                var httpMethod = context.Request.Method;
                var url = context.Request.GetDisplayUrl();
                var duration = stopWatch.Elapsed.ToString(TimeFormat, CultureInfo.InvariantCulture);
                var statusCode = (HttpStatusCode)context.Response.StatusCode;
                PushValueIntoContext(context, StatusCodeName, statusCode);
                logger.LogInformation("Request processed: {HttpMethod} {Url}, {Duration}, {StatusCode}", httpMethod, url, duration, statusCode);
            }
        }

        private static void PushValueIntoContext(HttpContext context, string requestName, object requestValue)
        {
            context.Response.RegisterForDispose(LogContext.PushProperty(requestName, requestValue));
        }
    }
}
