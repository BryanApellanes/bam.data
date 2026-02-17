using System.Data;
using System.Data.Common;

namespace Bam.Data
{
    /// <summary>
    /// Abstract base class for managing database connections with configurable pooling and lifetime settings.
    /// </summary>
    public abstract class DbConnectionManager : IDbConnectionManager
    {
        /// <summary>
        /// Gets or sets the database this connection manager serves.
        /// </summary>
        public virtual IDatabase Database { get; set; } = null!;

        /// <summary>
        /// Gets or sets the maximum number of concurrent connections.
        /// </summary>
        public virtual int MaxConnections { get; set; }

        /// <summary>
        /// Gets or sets the lifetime in milliseconds before connections are recycled.
        /// </summary>
        public virtual int LifetimeMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the event handler invoked when a connection's state changes.
        /// </summary>
        public StateChangeEventHandler StateChangeEventHandler { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether to block while releasing connections.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [block on release]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool BlockOnRelease { get; set; }

        /// <summary>
        /// Gets a database connection from the pool or creates a new one.
        /// </summary>
        /// <returns>A DbConnection instance.</returns>
        public abstract DbConnection GetDbConnection();

        /// <summary>
        /// Releases the specified database connection back to the pool or disposes it.
        /// </summary>
        /// <param name="dbConnection">The connection to release.</param>
        public abstract void ReleaseConnection(DbConnection dbConnection);

        protected DbConnection CreateConnection(StateChangeEventHandler stateChangeEventHandler = null!)
        {
            stateChangeEventHandler = stateChangeEventHandler ?? StateChangeEventHandler;
            DbConnection connection = Database.CreateConnection();
            if(StateChangeEventHandler != null)
            {
                connection.StateChange += stateChangeEventHandler;
            }
            return connection;
        }
    }
}
