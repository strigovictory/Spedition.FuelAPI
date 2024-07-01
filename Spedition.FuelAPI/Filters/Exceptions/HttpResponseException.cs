using System.Net;
using System.Runtime.Serialization;

namespace Spedition.FuelAPI.Filters.Exceptions
{
    /// <summary>
    /// Represents http response error that occur during application execution.
    /// </summary>
    [Serializable]
    public class HttpResponseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseException"/> class.
        /// </summary>
        public HttpResponseException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseException"/> class with a specified error notifyMessage.
        /// </summary>
        public HttpResponseException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseException"/> class with a specified error notifyMessage and inner exception.
        /// </summary>
        public HttpResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseException"/> class with a specified error notifyMessage and status of the HTTP output.
        /// </summary>
        public HttpResponseException(string message, HttpStatusCode status)
            : base(message)
        {
            Status = status;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseException"/> class.
        /// </summary>
        /// <param Name="serializationInfo">Serialization information.</param>
        /// <param Name="streamingContext">Streaming context.</param>
        protected HttpResponseException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        /// <summary>
        /// Gets the status of the HTTP output.
        /// </summary>
        public HttpStatusCode Status { get; } = HttpStatusCode.BadRequest;
    }
}
