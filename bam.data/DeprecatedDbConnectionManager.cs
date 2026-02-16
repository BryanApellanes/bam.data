using Bam.Logging;
using System.Data.Common;

namespace Bam.Data
{
    /// <summary>
    /// Deprecated connection manager that uses a HashSet-based pool with an AutoResetEvent for blocking.
    /// </summary>
    [Obsolete]
    public class DeprecatedDbConnectionManager : DbConnectionManager
    {
        HashSet<DbConnection> _connections;
        AutoResetEvent _resetEvent;
        /// <summary>
        /// Initializes a new DeprecatedDbConnectionManager for the specified database.
        /// </summary>
        /// <param name="database">The database to manage connections for.</param>
        public DeprecatedDbConnectionManager(Database database)
        {
            Database = database;
            MaxConnections = 10;
            LifetimeMilliseconds = 3100;
            _connections = new HashSet<DbConnection>();
            _resetEvent = new AutoResetEvent(false);
        }
        
        object _connectionLock = new object();
        /// <summary>
        /// Gets a database connection, blocking if the pool is full until a connection is released.
        /// </summary>
        /// <returns>A new DbConnection instance.</returns>
        public override DbConnection GetDbConnection()
        {
            if (_connections.Count >= MaxConnections)
            {
                if (!_resetEvent.WaitOne(LifetimeMilliseconds))
                {
                    _connections.BackwardsEach(connection => ReleaseConnection(connection));
                }
            }

            DbConnection conn = CreateConnection();
            lock (_connectionLock)
            {
                _connections.Add(conn);
            }
            return conn;
        }

        /// <summary>
        /// Releases a database connection by removing it from the pool, closing, and disposing it.
        /// </summary>
        /// <param name="conn">The connection to release.</param>
        public override void ReleaseConnection(DbConnection conn)
        {
            try
            {
                lock (_connectionLock)
                {
                    if (_connections.Contains(conn))
                    {
                        _connections.Remove(conn);
                    }

                    conn.Close();
                    conn.Dispose();
                    conn = null;
                }
            }
            catch (Exception ex)
            {
                Log.Trace("{0}: Exception releasing database connection: {1}", ex, nameof(DeprecatedDbConnectionManager), ex.Message);
            }

            _resetEvent.Set();
        }
    }
}