/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// A container that manages database instances by connection name, initializing them on demand.
    /// </summary>
    public class DatabaseContainer
    {
        readonly Dictionary<string, IDatabase> _databases;        
        public DatabaseContainer()
        {
            this._databases = new Dictionary<string, IDatabase>();
            this.TriedFallback = new List<string>();
        }

        /// <summary>
        /// Gets information about all databases currently managed by this container.
        /// </summary>
        /// <returns>An array of <see cref="DatabaseInfo"/> objects describing each managed database.</returns>
        public DatabaseInfo[] GetInfos()
        {
            List<DatabaseInfo> infos = new List<DatabaseInfo>();
            _databases.Keys.Each(ctx =>
            {
                infos.Add(new DatabaseInfo(_databases[ctx]));
            });
            return infos.ToArray();
        }

        /// <summary>
        /// Gets the Database for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IDatabase For<T>() where T : IDao
        {
            return this[typeof(T)];
        }

        /// <summary>
        /// Gets the Database for the specified connection name.
		/// This correlates to a connection in the default 
		/// app config file
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public IDatabase For(string connectionName)
        {
            return this[connectionName];
        }

        /// <summary>
        /// Gets the database for the specified Dao type.
        /// </summary>
        /// <param name="type">The Dao type to get the database for.</param>
        /// <returns>The database instance.</returns>
        public IDatabase For(Type type)
        {
            return this[type];
        }

        /// <summary>
        /// Begins a new transaction for the database associated with the specified Dao type.
        /// </summary>
        /// <typeparam name="T">The Dao type whose database will host the transaction.</typeparam>
        /// <returns>A new <see cref="IDaoTransaction"/>.</returns>
        public IDaoTransaction BeginTransaction<T>() where T : IDao
        {
            return Db.BeginTransaction<T>();
        }

        /// <summary>
        /// Begins a new transaction for the database associated with the specified Dao type.
        /// </summary>
        /// <param name="type">The Dao type whose database will host the transaction.</param>
        /// <returns>A new <see cref="IDaoTransaction"/>.</returns>
        public IDaoTransaction BeginTransaction(Type type)
        {
            return Db.BeginTransaction(type);
        }
        /// <summary>
        /// Gets the database for the specified type.
        /// </summary>
        /// <param name="daoType"></param>
        /// <returns></returns>
        public IDatabase this[Type daoType]
        {
            get => this[Dao.ConnectionName(daoType)];
            internal set => this[Dao.ConnectionName(daoType)] = value;
        }
        
        public IDatabase this[string connectionName]
        {
            get
            {
                if (!_databases.ContainsKey(connectionName))
                {
                    InitializeDatabase(connectionName, _databases);
                }

                return _databases[connectionName];
            }
            internal set
            {
                if (_databases.ContainsKey(connectionName))
                {
                    _databases[connectionName] = value;
                }
                else
                {
                    _databases.Add(connectionName, value);
                }
            }
        }

        /// <summary>
        /// The Action to execute if initialization fails
        /// </summary>
        public Action<string, Dictionary<string, IDatabase>> FallBack
        {
            get;
            set;
        }

        protected internal List<string> TriedFallback
        {
            get;
            private set;
        }

        internal void InitializeDatabase(string connectionName, Dictionary<string, IDatabase> databases)
        {
            DatabaseInitializationResult dir = DatabaseInitializers.TryInitialize(connectionName);
            if (dir.Success)
            {
                databases.AddMissing(connectionName, dir.Database);
            }
            else
            {
                if (FallBack != null && !TriedFallback.Contains(connectionName))
                {
                    TriedFallback.Add(connectionName);
                    FallBack(connectionName, databases);
                    InitializeDatabase(connectionName, databases);
                }
                else
                {
                    throw dir.Exception;
                }
            }
        }

    }
}
