using Spedition.FuelAPI.Accessors.Interfaces;

namespace Spedition.FuelAPI.Middlewares
{
    /// <summary>
    /// Correlation id middleware.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ICorrelationIdAccessor correlationIdAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationIdMiddleware"/> class.
        /// </summary>
        /// <param Name="next">Next step to provide request.</param>
        /// <param Name="correlationIdAccessor">Correlation id accessor.</param>
        public CorrelationIdMiddleware(RequestDelegate next, ICorrelationIdAccessor correlationIdAccessor)
        {
            this.next = next;
            this.correlationIdAccessor = correlationIdAccessor;
        }

        /// <summary>
        /// Provides handler for correlation id providing.
        /// </summary>
        /// <param Name="context">Context from request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId = correlationIdAccessor.GetCorrelationId();
            string correlationIdHeader = correlationIdAccessor.GetCorrelationIdHeader();
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers.Add(correlationIdHeader, correlationId);
            }

            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Add(correlationIdHeader, correlationId);
                return Task.CompletedTask;
            });

            await next.Invoke(context);
        }
    }
}
