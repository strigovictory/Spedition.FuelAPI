namespace Spedition.FuelAPI.Accessors.Interfaces
{
    /// <summary>
    /// Provide access to correlation id.
    /// </summary>
    public interface ICorrelationIdAccessor
    {
        /// <summary>
        /// Returns correlation id header.
        /// </summary>
        /// <returns>Correlation id header string.</returns>
        string GetCorrelationIdHeader();

        /// <summary>
        /// Returns correlation id.
        /// </summary>
        /// <returns>Correlation id string.</returns>
        string GetCorrelationId();
    }
}
