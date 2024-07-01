namespace Spedition.Fuel.DataAccess.Infrastructure
{
    /// <inheritdoc />
    public class DatabaseConfigsProvider : IDatabaseConfigsProvider
    {
        private readonly DatabaseConfigs databaseConfigs;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConfigsProvider"/> class.
        /// </summary>
        /// <param Name="databaseConfigs">Database configs options.</param>
        public DatabaseConfigsProvider(IOptions<DatabaseConfigs> databaseConfigs) => this.databaseConfigs = databaseConfigs.Value;

        /// <inheritdoc />
        public DbConnection GetConnection()
        {
            return new SqlConnection(databaseConfigs.ConnectionString);
        }
    }
}
