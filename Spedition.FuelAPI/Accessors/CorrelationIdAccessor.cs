using Spedition.FuelAPI.Accessors.Interfaces;

namespace Spedition.FuelAPI.Accessors
{
    public class CorrelationIdAccessor : ICorrelationIdAccessor
    {
        private const string CorrelationIdHeader = "x-correlation-id";

        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationIdAccessor"/> class.
        /// </summary>
        /// <param Name="httpContextAccessor">HTTP context accessor.</param>
        public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public string GetCorrelationIdHeader()
        {
            return CorrelationIdHeader;
        }

        /// <inheritdoc />
        public string GetCorrelationId()
        {
            return httpContextAccessor?
                .HttpContext?
                .Request?
                .Headers?
                .TryGetValue(CorrelationIdHeader, out var correlationId) ?? false
                ? correlationId.ToString()
                : string.Empty;
        }
    }
}
