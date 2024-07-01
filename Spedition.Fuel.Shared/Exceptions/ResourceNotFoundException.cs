namespace Spedition.Fuel.Shared.Exceptions
{
    /// <summary>
    ///     The exception that is thrown when resource cannot be found in the underlying storage.
    /// </summary>
    [Serializable]
    public class ResourceNotFoundException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ResourceNotFoundException" /> class.
        /// </summary>
        public ResourceNotFoundException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResourceNotFoundException" /> class.
        /// </summary>
        /// <param Name="message">The error message.</param>
        public ResourceNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResourceNotFoundException" /> class.
        /// </summary>
        /// <param Name="message">The error message.</param>
        /// <param Name="inner">Inner exception.</param>
        public ResourceNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResourceNotFoundException" /> class.
        /// </summary>
        /// <param Name="serializationInfo">Serialization information.</param>
        /// <param Name="streamingContext">Streaming context.</param>
        protected ResourceNotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
