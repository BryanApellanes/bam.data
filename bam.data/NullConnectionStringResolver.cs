/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data.Common;

namespace Bam.Data
{
	/// <summary>
	/// A connection string resolver that always throws, indicating no resolver was configured.
	/// </summary>
	public class NullConnectionStringResolver: IConnectionStringResolver
	{
		/// <summary>
		/// Initializes a new NullConnectionStringResolver with no database.
		/// </summary>
		public NullConnectionStringResolver() { }
		/// <summary>
		/// Initializes a new NullConnectionStringResolver for the specified database.
		/// </summary>
		/// <param name="database">The database associated with this resolver.</param>
		public NullConnectionStringResolver(Database database)
		{
			this.Database = database;
		}
		/// <summary>
		/// Gets or sets the database associated with this resolver.
		/// </summary>
		public Database Database { get; set; } = null!;

        #region IConnectionStringResolver Members

        /// <summary>
        /// Always throws InvalidOperationException because no resolver was configured.
        /// </summary>
        /// <param name="connectionName">The connection name to resolve.</param>
        /// <returns>Never returns; always throws.</returns>
        public System.Configuration.ConnectionStringSettings Resolve(string connectionName)
		{
			string db = Database == null ? "null": Database.GetType().Name;
			throw new InvalidOperationException("No ConnectionStringResolver was specified: Database={0}, ConnectionName={1}".Format(db, connectionName));
		}

        /// <summary>
        /// Gets a connection string builder, which will throw because Resolve always throws.
        /// </summary>
        /// <returns>Never returns; always throws via Resolve.</returns>
        public DbConnectionStringBuilder GetConnectionStringBuilder()
        {
            return new DbConnectionStringBuilder { ConnectionString = Resolve("Default")?.ConnectionString };
        }
        #endregion
    }
}
