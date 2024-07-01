namespace Spedition.Fuel.DataAccess.Infrastructure.Interfaces
{
    /// <summary>
    /// Provides database configs.
    /// </summary>
    public interface IDatabaseConfigsProvider
    {
        /// <summary>
        /// Provides connection.
        /// </summary>
        /// <returns>Database connection.</returns>
        DbConnection GetConnection();
    }
}
