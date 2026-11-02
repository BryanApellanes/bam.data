namespace Bam.Data
{
    /// <summary>
    /// Class used to report diagnostic information about a database: its type and connection name. It never exposes the
    /// connection string, which carries the credentials.
    /// </summary>
    public class DatabaseInfo
    {
        /// <summary>
        /// Initializes a new DatabaseInfo for the specified database.
        /// </summary>
        /// <param name="database">The database to report information about.</param>
        public DatabaseInfo(IDatabase database)
        {
            Args.ThrowIfNull(database, "database");
            Database = database;
        }
        
        protected IDatabase Database { get; }

        /// <summary>
        /// Gets the full type name of the database implementation.
        /// </summary>
        public string DatabaseType => Database.GetType().FullName!;
        /// <summary>
        /// Gets the database connection name.
        /// </summary>
        public string ConnectionName => Database.ConnectionName!;

        public override string ToString()
        {
            return $"{DatabaseType}:{ConnectionName}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DatabaseType, ConnectionName);
        }

        public override bool Equals(object? obj)
        {
            DatabaseInfo? dbInfo = obj as DatabaseInfo;
            if (dbInfo == null)
            {
                return false;
            }

            
            return ReferenceEquals(dbInfo.Database, this.Database);
        }
    }
}
