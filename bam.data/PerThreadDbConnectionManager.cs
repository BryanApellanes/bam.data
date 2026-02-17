using Bam.Logging;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;

namespace Bam.Data
{
    /// <summary>
    /// Connection manager that tracks one connection per thread, releasing previous connections when threads complete.
    /// </summary>
    public class PerThreadDbConnectionManager : DbConnectionManager
    {
        ConcurrentDictionary<int, DbConnection> _connections;
        /// <summary>
        /// Initializes a new PerThreadDbConnectionManager for the specified database.
        /// </summary>
        /// <param name="database">The database to manage connections for.</param>
        public PerThreadDbConnectionManager(Database database)
        {
            Database = database;
            MaxConnections = 10;
            LifetimeMilliseconds = 3100;
            _connections = new ConcurrentDictionary<int, DbConnection>();
        }
        
        /// <summary>
        /// Gets a database connection for the current thread, releasing any previous connection for this thread.
        /// </summary>
        /// <returns>A new DbConnection instance.</returns>
        public override DbConnection GetDbConnection()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;            
            if (_connections.ContainsKey(threadId))
            {
                GiveThreadAChanceToCompleteBeforeReleasingConnection(threadId);
            }

            if (_connections.Count >= MaxConnections)
            {
                if (!Exec.SleepUntil(() => _connections.Count < MaxConnections, out int slept, LifetimeMilliseconds))
                {
                    Log.Trace("Waited {0} milliseconds for connection count to drop but they were never below {1}, releasing all connections", slept, MaxConnections);
                    ReleaseAllConnections();
                }
            }

            SetConnection(threadId, out DbConnection connection);
            return connection;
        }

        /// <summary>
        /// Releases a database connection by closing and disposing it.
        /// </summary>
        /// <param name="connection">The connection to release.</param>
        public override void ReleaseConnection(DbConnection connection)
        {
            try
            {
                connection.Close();
                connection.Dispose();
                connection = null!;
            }
            catch (Exception ex)
            {
                Log.Trace("{0}: Exception releasing database connection: {1}", ex, nameof(PerThreadDbConnectionManager), ex.Message);
            }
        }

        private void ReleaseAllConnections()
        {
            try
            {
                foreach(int threadId in _connections.Keys)
                {
                    GiveThreadAChanceToCompleteBeforeReleasingConnection(threadId);
                }
            }
            catch (Exception ex)
            {
                Log.Trace("{0}: Exception releasing all connections: {1}", ex, nameof(PerThreadDbConnectionManager), ex.Message);
            }
        }

        private void GiveThreadAChanceToCompleteBeforeReleasingConnection(int threadId)
        {
            if (_connections.TryRemove(threadId, out DbConnection? dbConnection))
            {
                Task.Run(() =>
                {
                    ProcessThread? thread = Exec.GetThread(threadId);
                    if (thread != null)
                    {
                        int slept = Exec.SleepUntil(() => thread.ThreadState == System.Diagnostics.ThreadState.Terminated || thread.ThreadState == System.Diagnostics.ThreadState.Unknown, LifetimeMilliseconds * 2);
                        Exec.After(LifetimeMilliseconds - slept, () => ReleaseConnection(dbConnection!));
                    }
                    else
                    {
                        Exec.After(LifetimeMilliseconds, () => ReleaseConnection(dbConnection!));
                    }
                });
            }
        }

        private void SetConnection(int threadId, out DbConnection connection)
        {
            connection = CreateConnection();
            if(!_connections.TryAdd(threadId, connection))
            {
                DbConnection c = connection;                
                Log.Trace("{0}: Failed to add DbConnection to inner tracking dictionary", nameof(PerThreadDbConnectionManager));
                Exec.After(LifetimeMilliseconds, () => ReleaseConnection(c));
            }
        }
    }
}
