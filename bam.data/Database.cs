/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data;
using System.Data.Common;
using Bam.Logging;
using System.Reflection;
using Bam.DependencyInjection;
using Bam.Services;

namespace Bam.Data
{
    /// <summary>
    /// Provides a base implementation of IDatabase that manages connections, executes SQL, and handles schema initialization.
    /// </summary>
    public partial class Database: Loggable, IDatabase
    {
        List<DbConnection> _connections;
        /// <summary>
        /// Initializes a new Database instance with default settings.
        /// </summary>
        public Database()
        {
			_resetEvent = new AutoResetEvent(false);
			_connections = new List<DbConnection>();
			_schemaNames = new HashSet<string>();
            ServiceProvider = DependencyProvider.Default;           
            MaxConnections = 20;
            ConnectionManager = new DefaultDbConnectionManager(this);
        }

        /// <summary>
        /// Initializes a new Database instance with the specified service provider and connection details.
        /// </summary>
        /// <param name="serviceProvider">The dependency injection provider.</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="connectionName">Optional name identifying this connection.</param>
        public Database(DependencyProvider serviceProvider, string connectionString, string connectionName = null!)
            : this()
        {
            ServiceProvider = serviceProvider;
            ConnectionString = connectionString;
            ConnectionName = connectionName;
			ParameterPrefix = "@";
            if (!string.IsNullOrEmpty(ConnectionName))
            {
                Db.For(ConnectionName, this);
            }
        }

        /// <summary>
        /// Initializes a new Database instance with the specified connection string and optional connection name.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="connectionName">Optional name identifying this connection.</param>
        public Database(string connectionString, string? connectionName = null)
            : this(new DependencyProvider(), connectionString, connectionName!)
        {
        }

        static HashSet<DatabaseInfo> _infos = null!;
        static readonly object _infosLock = new object();
        /// <summary>
        /// Gets the set of DatabaseInfo instances tracking all known database connections.
        /// </summary>
        public static HashSet<DatabaseInfo> Infos
        {
            get
            {
                return _infosLock.DoubleCheckLock(ref _infos, () => new HashSet<DatabaseInfo>());
            }
        }

        /// <summary>
        /// Gets a ColumnNameListProvider that returns "*" for SELECT * queries.
        /// </summary>
        public static ColumnNameListProvider Star
        {
            get
            {
                return (type, db) => "*";
            }
        }

        /// <summary>
        /// Gets a ColumnNameListProvider that returns a delimited list of column names for a type.
        /// </summary>
        public static ColumnNameListProvider ColumnNames
        {
            get
            {
                return (type, db) => ColumnAttribute.GetColumns(type).ToDelimited(c => db.ColumnNameProvider(c));
            }
        }

        /// <summary>
        /// Gets the appropriate ColumnNameListProvider based on the SelectStar setting.
        /// </summary>
        public ColumnNameListProvider ColumnNameListProvider
        {
            get
            {
                return SelectStar ? Star : ColumnNames;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether to use Star instead of ColumnNames when executing Query instances
        /// </summary>
        public bool SelectStar
        {
            get; set;
        }

        /// <summary>
        /// Begins a new database transaction.
        /// </summary>
        /// <returns>A new IDaoTransaction instance.</returns>
        public IDaoTransaction BeginTransaction()
        {
            return Db.BeginTransaction(this);
        }

        /// <summary>
        /// Gets or sets the connection manager responsible for creating and releasing database connections.
        /// </summary>
        public IDbConnectionManager ConnectionManager
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum number of concurrent database connections.
        /// </summary>
        public int MaxConnections { get; set; }

        /// <summary>
        /// Gets or sets the dependency injection service provider for this database.
        /// </summary>
        public DependencyProvider ServiceProvider { get; set; }

		/// <summary>
		/// Gets or sets the parameter prefix used in SQL parameterized queries (e.g., "@").
		/// </summary>
		public string ParameterPrefix { get; set; } = null!;
		/// <summary>
		/// Used to locate the connection string in the 
		/// configuration file as well as uniquely identify
		/// types that are associated with a specific 
		/// schema.  
		/// </summary>
        public string? ConnectionName { get; set; }

		protected HashSet<string> _schemaNames;
		/// <summary>
		/// Gets the names of schemas that have been initialized on this database.
		/// </summary>
		public string[] SchemaNames => _schemaNames.ToArray();

        /// <summary>
        /// Gets the database name extracted from the connection string.
        /// </summary>
        public virtual string Name
        {
            get
            {
                DbConnectionStringBuilder cb = CreateConnectionStringBuilder();                
                cb.ConnectionString = this.ConnectionString;
                string databaseName = "";
                if (cb.ContainsKey("Initial Catalog"))                
                {
                    databaseName = (cb["Initial Catalog"] as string)!;
                }

                if (cb.ContainsKey("Database"))
                {
                    databaseName = (cb["Database"] as string)!;
                }

                if (cb.ContainsKey("Data Source"))
                {
                    databaseName = (cb["Data Source"] as string)!;
                }

                if (string.IsNullOrEmpty(databaseName))
                {
                    throw new InvalidOperationException($"Unable to determine database name from connection string: {ConnectionString}");
                }
                return databaseName;
            }
        }

        /// <summary>
        /// Hydrates the child collections of the specified Dao instance.
        /// </summary>
        /// <param name="dao">The Dao instance to hydrate.</param>
        public virtual void Hydrate(IDao dao)
        {
            GetHydrator()?.HydrateChildren(dao, this);
        }

        /// <summary>
        /// Gets the hydrator used to load child collections for Dao instances.
        /// </summary>
        /// <returns>The configured IHydrator or the default hydrator.</returns>
        public virtual IHydrator GetHydrator()
        {
            return ServiceProvider?.Get<IHydrator>() ?? Hydrator.DefaultHydrator;
        }

        /// <summary>
        /// Fills a dictionary mapping enum values to Dao instances from the database.
        /// </summary>
        /// <typeparam name="EnumType">The enum type to use as dictionary keys.</typeparam>
        /// <typeparam name="DaoType">The Dao type to retrieve.</typeparam>
        /// <param name="dictionary">The dictionary to fill.</param>
        /// <param name="nameColumn">The column name containing the enum value names.</param>
        /// <returns>The filled dictionary.</returns>
        public Dictionary<EnumType, DaoType> FillEnumDictionary<EnumType, DaoType>(Dictionary<EnumType, DaoType> dictionary, string nameColumn) where DaoType : IDao, new() where EnumType : notnull
        {
            QuerySet query = ExecuteQuery<DaoType>();

            QueryResult result = ((QueryResult)query.Results[0]);
            if (result.DataTable.Rows.Count == 0)
            {
                InitEnumValues<EnumType, DaoType>("Value", nameColumn);
                query = ExecuteQuery<DaoType>();
                result = ((QueryResult)query.Results[0]);
            }

            foreach (DataRow row in result.DataTable.Rows)
            {
                EnumType enumVal = (EnumType)Enum.Parse(typeof(EnumType), (string)row[nameColumn]);
                DaoType inst = new DaoType
                {
                    DataRow = row
                };
                dictionary.TryAdd(enumVal, inst);
            }

            return dictionary;
        }

		/// <summary>
		/// Creates a new query that selects all records of the specified Dao type.
		/// </summary>
		/// <typeparam name="C">The query filter/column type.</typeparam>
		/// <typeparam name="T">The Dao type to query.</typeparam>
		/// <returns>A new query instance.</returns>
		public virtual IQuery<C, T> GetQuery<C, T>()
			where C : IQueryFilter, IFilterToken, new()
			where T: IDao, new()
		{
			return new Query<C,T>();
		}

		/// <summary>
		/// Creates a new query with a WHERE delegate and optional ORDER BY clause.
		/// </summary>
		/// <typeparam name="C">The query filter/column type.</typeparam>
		/// <typeparam name="T">The Dao type to query.</typeparam>
		/// <param name="where">The WHERE clause delegate.</param>
		/// <param name="orderBy">Optional ORDER BY clause.</param>
		/// <returns>A new query instance with the specified filter.</returns>
		public virtual IQuery<C, T> GetQuery<C, T>(WhereDelegate<C> where, IOrderBy<C> orderBy = null!)
			where C : IQueryFilter, IFilterToken, new()
			where T : IDao, new()
		{
			return new Query<C, T>(where, orderBy, this);
		}

		/// <summary>
		/// Creates a new query with a filter function and optional ORDER BY clause.
		/// </summary>
		/// <typeparam name="C">The query filter/column type.</typeparam>
		/// <typeparam name="T">The Dao type to query.</typeparam>
		/// <param name="where">The filter function.</param>
		/// <param name="orderBy">Optional ORDER BY clause.</param>
		/// <returns>A new query instance with the specified filter.</returns>
		public virtual IQuery<C, T> GetQuery<C, T>(Func<C, IQueryFilter<C>> where, IOrderBy<C> orderBy = null!)
			where C : IQueryFilter, IFilterToken, new()
			where T : IDao, new()
		{
			return new Query<C, T>(where, orderBy, this);
		}

		/// <summary>
		/// Creates a new query with a generic delegate for the WHERE clause.
		/// </summary>
		/// <typeparam name="C">The query filter/column type.</typeparam>
		/// <typeparam name="T">The Dao type to query.</typeparam>
		/// <param name="where">The delegate defining the filter.</param>
		/// <returns>A new query instance with the specified filter.</returns>
		public virtual IQuery<C, T> GetQuery<C, T>(Delegate where)
			where C : IQueryFilter, IFilterToken, new()
			where T : IDao, new()
		{
			return new Query<C, T>(where, this);
		}

        /// <summary>
        /// Gets a schema writer from the service provider for generating DDL statements.
        /// </summary>
        /// <returns>An ISchemaWriter instance.</returns>
        public ISchemaWriter GetSchemaWriter()
        {
            return ServiceProvider.Get<ISchemaWriter>();
        }

        /// <summary>
        /// Creates a new SQL string builder instance.
        /// </summary>
        /// <returns>An ISqlStringBuilder instance.</returns>
        public ISqlStringBuilder Sql()
        {
            return GetSqlStringBuilder();
        }

        /// <summary>
        /// Gets a new SQL string builder configured with the current SelectStar setting.
        /// </summary>
        /// <returns>An ISqlStringBuilder instance.</returns>
        public virtual ISqlStringBuilder GetSqlStringBuilder()
        {
            SqlStringBuilder sql = ServiceProvider.Get<SqlStringBuilder>();
            sql.SelectStar = SelectStar;
            return sql;
        }

        /// <summary>
        /// Gets a new query set configured with this database and the current SelectStar setting.
        /// </summary>
        /// <returns>An IQuerySet instance.</returns>
        public virtual IQuerySet GetQuerySet()
        {
            QuerySet sql = ServiceProvider.Get<QuerySet>();
            sql.Database = this;
            sql.SelectStar = SelectStar;
            return sql;
        }

        /// <summary>
        /// Gets the data type translator from the service provider or creates a default one.
        /// </summary>
        /// <returns>An IDataTypeTranslator instance.</returns>
        public virtual IDataTypeTranslator GetDataTypeTranslator()
        {
            if (!ServiceProvider.TryGet<IDataTypeTranslator>(out IDataTypeTranslator dataTypeTranslator))
            {
                return new DataTypeTranslator();
            }
            return dataTypeTranslator;
        }

        /// <summary>
        /// Gets the database parameters from the specified SQL string builder.
        /// </summary>
        /// <param name="sqlStringBuilder">The SQL string builder containing parameter info.</param>
        /// <returns>An array of DbParameter instances.</returns>
        public DbParameter[] GetParameters(ISqlStringBuilder sqlStringBuilder)
        {
            IParameterBuilder paramBuilder = GetService<IParameterBuilder>();
            Args.ThrowIfNull(paramBuilder, "IParameterBuilder");
            return paramBuilder.GetParameters(sqlStringBuilder);
        }

        /// <summary>
        /// Execute the specified SqlStringBuilder using the 
        /// specified generic type to determine which database
        /// to use.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        public virtual void ExecuteSql<T>(ISqlStringBuilder builder) where T : IDao 
        {
            ExecuteSql(builder, ServiceProvider.Get<IParameterBuilder>());
        }

        /// <summary>
        /// Executes the SQL built by the specified ISqlStringBuilder.
        /// </summary>
        /// <param name="builder">The SQL string builder.</param>
        public virtual void ExecuteSql(ISqlStringBuilder builder)
        {
            ExecuteSql(builder, ServiceProvider.Get<IParameterBuilder>());
        }

        /// <summary>
        /// Executes the SQL built by the specified ISqlStringBuilder using the specified parameter builder.
        /// </summary>
        /// <param name="builder">The SQL string builder.</param>
        /// <param name="parameterBuilder">The parameter builder for creating DbParameters.</param>
        public virtual void ExecuteSql(ISqlStringBuilder builder, IParameterBuilder parameterBuilder)
        {
            ExecuteSql(builder.ToString(), CommandType.Text, parameterBuilder.GetParameters(builder));
        }

        /// <summary>
        /// Executes a stored procedure with the specified parameters.
        /// </summary>
        /// <param name="sprocName">The stored procedure name.</param>
        /// <param name="dbParameters">The parameters to pass to the stored procedure.</param>
        public virtual void ExecuteStoredProcedure(string sprocName, params DbParameter[] dbParameters)
        {
            ExecuteSql(sprocName, CommandType.StoredProcedure, dbParameters);
        }

        /// <summary>
        /// Executes a SQL statement with dynamic object parameters.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement to execute.</param>
        /// <param name="dbParameters">Dynamic object whose properties become parameters.</param>
        public virtual void ExecuteSql(string sqlStatement, object dbParameters)
        {
            ExecuteSql(sqlStatement, dbParameters.ToDbParameters(this).ToArray());
        }

        /// <summary>
        /// Executes a SQL text statement with the specified parameters.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement to execute.</param>
        /// <param name="dbParameters">The database parameters.</param>
        public virtual void ExecuteSql(string sqlStatement, params DbParameter[] dbParameters)
        {
            ExecuteSql(sqlStatement, CommandType.Text, dbParameters);
        }

        /// <summary>
        /// Executes a SQL statement with the specified command type and parameters.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement to execute.</param>
        /// <param name="commandType">The type of command (Text, StoredProcedure, etc.).</param>
        /// <param name="dbParameters">The database parameters.</param>
        public virtual void ExecuteSql(string sqlStatement, CommandType commandType, params DbParameter[] dbParameters)
        {
            DbConnection conn = GetOpenDbConnection();
            ExecuteSql(sqlStatement, commandType, dbParameters, conn);
        }

        /// <summary>
        /// Executes a SQL statement on the specified connection with optional connection release.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement to execute.</param>
        /// <param name="commandType">The type of command.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <param name="conn">The database connection to use.</param>
        /// <param name="releaseConnection">Whether to release the connection after execution.</param>
        public virtual void ExecuteSql(string sqlStatement, CommandType commandType, DbParameter[] dbParameters, DbConnection conn, bool releaseConnection = true)
        {
            ExecuteSql(sqlStatement, commandType, dbParameters, conn, (ex) => { }, releaseConnection);
        }

        /// <summary>
        /// Event raised after a command is successfully executed.
        /// </summary>
        public event EventHandler CommandExecuted = null!;
        /// <summary>
        /// Event raised when a command execution throws an exception.
        /// </summary>
        public event EventHandler CommandException = null!;
        /// <summary>
        /// Executes a SQL statement with full control over connection, exception handling, and connection release.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement to execute.</param>
        /// <param name="commandType">The type of command.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <param name="conn">The database connection to use.</param>
        /// <param name="exceptionHandler">The action to invoke if an exception occurs.</param>
        /// <param name="releaseConnection">Whether to release the connection after execution.</param>
        public virtual void ExecuteSql(string sqlStatement, CommandType commandType, DbParameter[] dbParameters, DbConnection conn, Action<Exception> exceptionHandler, bool releaseConnection = true)
        {
            try
            {
                DbCommand cmd = PrepareCommand(sqlStatement, commandType, dbParameters, conn);
                cmd.ExecuteNonQuery();
                FireEvent(CommandExecuted, new DatabaseExecutionEventArgs { Database = this, Command = cmd });
            }
            catch (Exception ex)
            {
                exceptionHandler(ex);
                FireEvent(CommandException, new DatabaseExecutionEventArgs { Database = this, Exception = ex, Message = ex.Message });
            }
            finally
            {
                if (releaseConnection)
                {
                    ReleaseConnection(conn);
                }
            }
        }

        /// <summary>
        /// Executes the SQL and returns results as typed objects using a data reader.
        /// </summary>
        /// <typeparam name="T">The type to map each row to.</typeparam>
        /// <param name="sqlStatement">The SQL string builder.</param>
        /// <param name="onReaderExecuted">Optional callback invoked after the reader completes.</param>
        /// <returns>An enumerable of T instances.</returns>
        public virtual IEnumerable<T> ExecuteReader<T>(ISqlStringBuilder sqlStatement, Action<DbDataReader>? onReaderExecuted = null!) where T : class, new()
        {
            return ExecuteReader<T>(sqlStatement.ToString(), GetParameters(sqlStatement), null, true, onReaderExecuted);
        }

        /// <summary>
        /// Executes a SQL statement with dynamic parameters and returns typed results via a data reader.
        /// </summary>
        /// <typeparam name="T">The type to map each row to.</typeparam>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">Dynamic object whose properties become parameters.</param>
        /// <param name="onReaderExecuted">Optional callback invoked after the reader completes.</param>
        /// <returns>An enumerable of T instances.</returns>
        public virtual IEnumerable<T> ExecuteReader<T>(string sqlStatement, object dbParameters, Action<DbDataReader>? onReaderExecuted = null!) where T : class, new()
        {
            return ExecuteReader<T>(sqlStatement, dbParameters.ToDbParameters(this).ToArray(), null, true, onReaderExecuted);
        }

        /// <summary>
        /// Executes a SQL statement and returns typed results, outputting the connection for caller management.
        /// </summary>
        /// <typeparam name="T">The type to map each row to.</typeparam>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <param name="conn">The connection used, output for caller to manage.</param>
        /// <param name="onReaderExecuted">Optional callback invoked after the reader completes.</param>
        /// <returns>An enumerable of T instances.</returns>
        public virtual IEnumerable<T> ExecuteReader<T>(string sqlStatement, DbParameter[] dbParameters, out DbConnection conn, Action<DbDataReader>? onReaderExecuted = null) where T : class, new()
        {
            conn = GetOpenDbConnection();
            return ExecuteReader<T>(sqlStatement, dbParameters, conn, false, onReaderExecuted);
        }

        /// <summary>
        /// Executes a SQL statement and returns typed results with optional connection and close control.
        /// </summary>
        /// <typeparam name="T">The type to map each row to.</typeparam>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <param name="conn">Optional connection; a new one is created if null.</param>
        /// <param name="closeConnection">Whether to close the connection after reading.</param>
        /// <param name="onReaderExecuted">Optional callback invoked after the reader completes.</param>
        /// <returns>An enumerable of T instances.</returns>
        public virtual IEnumerable<T> ExecuteReader<T>(string sqlStatement, DbParameter[] dbParameters, DbConnection? conn = null, bool closeConnection = true, Action<DbDataReader>? onReaderExecuted = null) where T : class, new()
        {
            return ExecuteReader<T>(sqlStatement, CommandType.Text, dbParameters, conn ?? GetOpenDbConnection(), closeConnection, onReaderExecuted);
        }
        
        /// <summary>
        /// Executes a SQL statement and returns typed results via a data reader.
        /// </summary>
        /// <typeparam name="T">The type to map each row to.</typeparam>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <returns>An enumerable of T instances.</returns>
        public virtual IEnumerable<T> ExecuteReader<T>(string sqlStatement, params DbParameter[] dbParameters) where T : class, new()
        {
            return ExecuteReader<T>(sqlStatement, CommandType.Text, dbParameters, GetOpenDbConnection());
        }

        /// <summary>
        /// Executes a SQL statement as a data reader and yields typed results with full control over all parameters.
        /// </summary>
        /// <typeparam name="T">The type to map each row to.</typeparam>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <param name="conn">The database connection.</param>
        /// <param name="closeConnection">Whether to close the connection after reading.</param>
        /// <param name="onReaderExecuted">Optional callback invoked after the reader completes.</param>
        /// <returns>An enumerable of T instances.</returns>
        public virtual IEnumerable<T> ExecuteReader<T>(string sqlStatement, CommandType commandType, DbParameter[] dbParameters, DbConnection conn, bool closeConnection = true, Action<DbDataReader>? onReaderExecuted = null) where T: class, new()
        {
            DbDataReader reader = ExecuteReader(sqlStatement, commandType, dbParameters, conn);
            onReaderExecuted = onReaderExecuted ?? ((dr) => { });
            if (reader.HasRows)
            {
                List<string> columnNames = GetColumnNames(reader);
                while (reader.Read())
                {
                    T next = new T();
                    foreach(string columnName in columnNames)
                    {
                        ReaderPropertySetter(next, columnName, reader[columnName]);
                    }
                    yield return next;
                }
            }
            if (closeConnection)
            {
                ReleaseConnection(conn);
            }
            onReaderExecuted(reader);
            yield break;
        }

        protected virtual void ReaderPropertySetter(object instance, string propertyName, object propertyValue)
        {
            instance.Property(propertyName, propertyValue);
        }


        /// <summary>
        /// Executes the specified SQL string builder and returns a DbDataReader.
        /// </summary>
        /// <param name="sqlStatement">The SQL string builder.</param>
        /// <returns>A DbDataReader for reading the results.</returns>
        public virtual DbDataReader ExecuteReader(ISqlStringBuilder sqlStatement)
        {
            return ExecuteReader(sqlStatement.ToString(), GetParameters(sqlStatement));
        }

        /// <summary>
        /// Executes a SQL statement with dynamic parameters and returns a DbDataReader.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">Dynamic object whose properties become parameters.</param>
        /// <returns>A DbDataReader for reading the results.</returns>
        public virtual DbDataReader ExecuteReader(string sqlStatement, object dbParameters)
        {
            return ExecuteReader(sqlStatement, dbParameters, out DbConnection ignore);
        }

        /// <summary>
        /// Executes a SQL statement with dynamic parameters, outputting the connection for caller management.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">Dynamic object whose properties become parameters.</param>
        /// <param name="conn">The connection used, output for caller to manage.</param>
        /// <returns>A DbDataReader for reading the results.</returns>
        public virtual DbDataReader ExecuteReader(string sqlStatement, object dbParameters, out DbConnection conn)
        {
            return ExecuteReader(sqlStatement, dbParameters.ToDbParameters(this).ToArray(), out conn);
        }

        /// <summary>
        /// Executes a SQL statement and returns a DbDataReader, outputting the connection for caller management.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <param name="conn">The connection used, output for caller to manage.</param>
        /// <returns>A DbDataReader for reading the results.</returns>
        public virtual DbDataReader ExecuteReader(string sqlStatement, DbParameter[] dbParameters, out DbConnection conn)
        {
            conn = GetOpenDbConnection();
            return ExecuteReader(sqlStatement, CommandType.Text, dbParameters, conn);
        }

        /// <summary>
        /// Executes a SQL statement and returns a DbDataReader with an optional connection.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <param name="conn">Optional connection; a new one is created if null.</param>
        /// <returns>A DbDataReader for reading the results.</returns>
        public virtual DbDataReader ExecuteReader(string sqlStatement, DbParameter[] dbParameters, DbConnection? conn = null!)
        {
            return ExecuteReader(sqlStatement, CommandType.Text, dbParameters, conn ?? GetOpenDbConnection());
        }

        /// <summary>
        /// Event raised after a reader is successfully executed.
        /// </summary>
        public event EventHandler ReaderExecuted = null!;
        /// <summary>
        /// Event raised when a reader execution throws an exception.
        /// </summary>
        public event EventHandler ReaderException = null!;
        /// <summary>
        /// Executes a SQL statement and returns a DbDataReader with full control over command type and connection.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <param name="conn">The database connection.</param>
        /// <returns>A DbDataReader for reading the results.</returns>
        public virtual DbDataReader ExecuteReader(string sqlStatement, CommandType commandType, DbParameter[] dbParameters, DbConnection conn)
        {
            DbDataReader? reader = null;
            try
            {
                DbCommand cmd = PrepareCommand(sqlStatement, commandType, dbParameters, conn);
                reader = cmd.ExecuteReader();
                FireEvent(ReaderExecuted, new DatabaseExecutionEventArgs { Database = this, DataReader = reader });
            }
            catch (Exception ex)
            {
                FireEvent(ReaderException, new DatabaseExecutionEventArgs { Database = this, Exception = ex, Message = ex.Message });
            }
            return reader!;
        }

        // -- start datatable readers
        /// <summary>
        /// Executes the SQL and returns results as a DataTable using a data reader.
        /// </summary>
        /// <param name="sqlStatement">The SQL string builder.</param>
        /// <returns>A DataTable containing the results.</returns>
        public virtual DataTable GetDataTableFromReader(ISqlStringBuilder sqlStatement)
        {
            return GetDataTableFromReader(sqlStatement.ToString(), GetParameters(sqlStatement));
        }
        /// <summary>
        /// Executes a SQL statement with dynamic parameters and returns results as a DataTable using a data reader.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">Dynamic object whose properties become parameters.</param>
        /// <returns>A DataTable containing the results.</returns>
        public virtual DataTable GetDataTableFromReader(string sqlStatement, object dbParameters)
        {
            DbConnection ignore;
            return GetDataTableFromReader(sqlStatement, dbParameters, out ignore);
        }
        /// <summary>
        /// Executes a SQL statement with dynamic parameters, returning a DataTable and outputting the connection.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">Dynamic object whose properties become parameters.</param>
        /// <param name="conn">The connection used, output for caller to manage.</param>
        /// <returns>A DataTable containing the results.</returns>
        public virtual DataTable GetDataTableFromReader(string sqlStatement, object dbParameters, out DbConnection conn)
        {
            return GetDataTableFromReader(sqlStatement, dbParameters.ToDbParameters(this).ToArray(), out conn);
        }

        /// <summary>
        /// Executes a SQL statement and returns a DataTable, outputting the connection for caller management.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <param name="conn">The connection used, output for caller to manage.</param>
        /// <returns>A DataTable containing the results.</returns>
        public virtual DataTable GetDataTableFromReader(string sqlStatement, DbParameter[] dbParameters, out DbConnection conn)
        {
            conn = GetOpenDbConnection();
            return GetDataTableFromReader(sqlStatement, CommandType.Text, dbParameters, conn, false);
        }

        /// <summary>
        /// Executes a SQL statement and returns a DataTable with an optional connection.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <param name="conn">Optional connection; a new one is created if null.</param>
        /// <returns>A DataTable containing the results.</returns>
        public virtual DataTable GetDataTableFromReader(string sqlStatement, DbParameter[] dbParameters, DbConnection conn = null!)
        {
            return GetDataTableFromReader(sqlStatement, CommandType.Text, dbParameters, conn ?? GetOpenDbConnection(), false);
        }

        /// <summary>
        /// Executes a SQL statement and returns a DataTable with full control over command type, connection, and close behavior.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <param name="conn">The database connection.</param>
        /// <param name="closeConnection">Whether to close the connection after reading.</param>
        /// <returns>A DataTable containing the results.</returns>
        public virtual DataTable GetDataTableFromReader(string sqlStatement, CommandType commandType, DbParameter[] dbParameters, DbConnection conn, bool closeConnection = true)
        {
            DbDataReader reader = ExecuteReader(sqlStatement, commandType, dbParameters, conn);
            DataTable table = new DataTable(8.RandomLetters());
            if (reader.HasRows)
            {
                table = table ?? new DataTable(8.RandomLetters());
                List<string> columnNames = GetColumnNames(reader);
                columnNames.Each(new { Table = table }, (ctx, cn) =>
                {
                    ctx.Table.Columns.Add(new DataColumn(cn));
                });
                while (reader.Read())
                {
                    DataRow row = table.NewRow();
                    columnNames.Each(new { Row = row, Reader = reader }, (ctx, cn) =>
                    {
                        ctx.Row[cn] = ctx.Reader[cn];
                    });
                    table.Rows.Add(row);
                }
            }
            if (closeConnection)
            {
                ReleaseConnection(conn);
            }
            return table;
        }

        /// <summary>
        /// Executes the SQL and returns results as an enumerable of DataRow using a data reader.
        /// </summary>
        /// <param name="sqlStatement">The SQL string builder.</param>
        /// <returns>An enumerable of DataRow instances.</returns>
        public virtual IEnumerable<DataRow> GetDataRowsFromReader(ISqlStringBuilder sqlStatement)
        {
            DbConnection ignore;
            return GetDataRowsFromReader(sqlStatement.ToString(), GetParameters(sqlStatement), out ignore);
        }

        /// <summary>
        /// Executes the SQL and returns results as an enumerable of DataRow, outputting the connection.
        /// </summary>
        /// <param name="sqlStatement">The SQL string builder.</param>
        /// <param name="conn">The connection used, output for caller to manage.</param>
        /// <returns>An enumerable of DataRow instances.</returns>
        public virtual IEnumerable<DataRow> GetDataRowsFromReader(ISqlStringBuilder sqlStatement, out DbConnection conn)
        {
            return GetDataRowsFromReader(sqlStatement.ToString(), GetParameters(sqlStatement), out conn);
        }

        /// <summary>
        /// Executes a SQL statement and returns data rows, outputting the connection for caller management.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <param name="conn">The connection used, output for caller to manage.</param>
        /// <returns>An enumerable of DataRow instances.</returns>
        public virtual IEnumerable<DataRow> GetDataRowsFromReader(string sqlStatement, DbParameter[] dbParameters, out DbConnection conn)
        {
            conn = GetOpenDbConnection();
            return GetDataRowsFromReader(sqlStatement, dbParameters, conn);
        }

        /// <summary>
        /// Executes a SQL statement on the specified connection and returns data rows.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <param name="conn">The database connection to use.</param>
        /// <returns>An enumerable of DataRow instances.</returns>
        public virtual IEnumerable<DataRow> GetDataRowsFromReader(string sqlStatement, DbParameter[] dbParameters, DbConnection conn)
        {
            return GetDataRowsFromReader(sqlStatement, CommandType.Text, dbParameters, conn);
        }

        /// <summary>
        /// Executes a SQL statement with the specified command type and returns data rows.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <param name="conn">The database connection.</param>
        /// <returns>An enumerable of DataRow instances.</returns>
        public virtual IEnumerable<DataRow> GetDataRowsFromReader(string sqlStatement, CommandType commandType, DbParameter[] dbParameters, DbConnection conn)
        {
            DbDataReader reader = ExecuteReader(sqlStatement, commandType, dbParameters, conn);
            return GetDataRowsFromReader(reader);
        }

        protected virtual IEnumerable<DataRow> GetDataRowsFromReader(DbDataReader reader, DataTable table = null!)
        {
            if (reader.HasRows)
            {
                table = table ?? new DataTable(8.RandomLetters());
                List<string> columnNames = GetColumnNames(reader);
                columnNames.Each(new { Table = table }, (ctx, cn) =>
                {
                    ctx.Table.Columns.Add(new DataColumn(cn));
                });
                while (reader.Read())
                {
                    DataRow row = table.NewRow();                    
                    columnNames.Each(new { Row = row, Reader = reader }, (ctx, cn) =>
                    {
                        ctx.Row[cn] = ctx.Reader[cn];
                    });
                    table.Rows.Add(row);
                    yield return row;
                }
            }
            yield break;
        }
        // -- end datatable readers

        /// <summary>
        /// Executes a query and returns a single scalar value cast to type T.
        /// </summary>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <param name="sql">The SQL string builder.</param>
        /// <returns>The scalar value cast to T, or default if not found.</returns>
        public virtual T QuerySingle<T>(ISqlStringBuilder sql)
        {
            return QuerySingle<T>(sql.ToString(), GetService<IParameterBuilder>().GetParameters(sql));
        }

        /// <summary>
        /// Executes a query with dynamic parameters and returns a single scalar value cast to type T.
        /// </summary>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <param name="singleValueQuery">The SQL query returning a single value.</param>
        /// <param name="dynamicParamters">Dynamic object whose properties become parameters.</param>
        /// <returns>The scalar value cast to T, or default if not found.</returns>
        public virtual T QuerySingle<T>(string singleValueQuery, object dynamicParamters)
        {
            return QuerySingle<T>(singleValueQuery, dynamicParamters.ToDbParameters(this).ToArray());
        }

        /// <summary>
        /// Executes a query and returns a single scalar value cast to type T.
        /// </summary>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <param name="singleValueQuery">The SQL query returning a single value.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <returns>The scalar value cast to T, or default if not found.</returns>
        public virtual T QuerySingle<T>(string singleValueQuery, params DbParameter[] dbParameters)
        {
            DataRow row = GetFirstRow(singleValueQuery, dbParameters);
            if(row.Table.Columns.Count > 0 && row[0] != DBNull.Value)
            {                
                return (T)row[0];
            }
            return default(T)!;
        }

        /// <summary>
        /// Executes a query returning a single column and casts each value to type T.
        /// </summary>
        /// <typeparam name="T">The column value type.</typeparam>
        /// <param name="singleColumnQuery">The SQL query returning a single column.</param>
        /// <param name="dynamicParameters">Dynamic object whose properties become parameters.</param>
        /// <returns>An enumerable of T values from the single column.</returns>
        public virtual IEnumerable<T> QuerySingleColumn<T>(string singleColumnQuery, object dynamicParameters)
        {
            return QuerySingleColumn<T>(singleColumnQuery, dynamicParameters.ToDbParameters(this).ToArray());
        }
        /// <summary>
        /// Execute a query that returns a single column of results casting
        /// each to the specified generic type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="singleColumnQuery"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> QuerySingleColumn<T>(string singleColumnQuery, params DbParameter[] dbParameters)
        {
            return Query<T>(singleColumnQuery, (row) => (T)row[0], dbParameters);
        }

        /// <summary>
        /// Execute the specified sqlQuery and return results as an Enumerable of
        /// dynamic object instances.  Property access can be done using column names 
        /// directly.
        /// </summary>
        /// <param name="sqlQuery"></param>
        /// <param name="dynamicDbParameters"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public IEnumerable<dynamic> Query(string sqlQuery, object dynamicDbParameters, string typeName = null!)
        {
            DbParameter[] dbParameters = dynamicDbParameters.ToDbParameters(this).ToArray();            
            return Query(sqlQuery, dbParameters, typeName);
        }

        /// <summary>
        /// Executes a SQL query with dictionary parameters and returns results as dynamic objects.
        /// </summary>
        /// <param name="sqlQuery">The SQL query.</param>
        /// <param name="dictDbParameters">Dictionary of parameter names and values.</param>
        /// <param name="typeName">Optional type name for the dynamic result.</param>
        /// <returns>An enumerable of dynamic objects.</returns>
        public IEnumerable<dynamic> Query(string sqlQuery, Dictionary<string, object> dictDbParameters, string typeName = null!)
        {
            DbParameter[] dbParameters = dictDbParameters.ToDbParameters(this).ToArray();
            return Query(sqlQuery, dbParameters, typeName);
        }

        /// <summary>
        /// Executes a SQL query with dynamic parameters and returns typed results.
        /// </summary>
        /// <typeparam name="T">The type to map each row to.</typeparam>
        /// <param name="sqlQuery">The SQL query.</param>
        /// <param name="dynamicDbParameters">Dynamic object whose properties become parameters.</param>
        /// <returns>An enumerable of T instances.</returns>
        public IEnumerable<T> Query<T>(string sqlQuery, object dynamicDbParameters)
        {
            return Query<T>(sqlQuery, dynamicDbParameters.ToDbParameters(this).ToArray());
        }
        /// <summary>
        /// Executes a SQL query with dictionary parameters and returns typed results.
        /// </summary>
        /// <typeparam name="T">The type to map each row to.</typeparam>
        /// <param name="sqlQuery">The SQL query.</param>
        /// <param name="dbParameters">Dictionary of parameter names and values.</param>
        /// <returns>An enumerable of T instances.</returns>
        public IEnumerable<T> Query<T>(string sqlQuery, Dictionary<string, object> dbParameters)
        {
            return Query<T>(sqlQuery, dbParameters.ToDbParameters(this).ToArray());
        }
        /// <summary>
        /// Executes a SQL query with parameters and returns typed results.
        /// </summary>
        /// <typeparam name="T">The type to map each row to.</typeparam>
        /// <param name="sqlQuery">The SQL query.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <returns>An enumerable of T instances.</returns>
        public IEnumerable<T> Query<T>(string sqlQuery, params DbParameter[] dbParameters)
        {
            return Query<T>(sqlQuery, (row) => row.ToInstanceOf<T>(), dbParameters);
        }

        /// <summary>
        /// Executes a SQL query and processes each row with a custom function.
        /// </summary>
        /// <typeparam name="T">The type returned by the row processor.</typeparam>
        /// <param name="sqlQuery">The SQL query.</param>
        /// <param name="rowProcessor">Function to convert each DataRow to T.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <returns>An enumerable of T instances.</returns>
        public IEnumerable<T> Query<T>(string sqlQuery, Func<DataRow, T> rowProcessor, params DbParameter[] dbParameters)
        {
            DataTable table = GetDataTable(sqlQuery, dbParameters);
            foreach(DataRow row in table.Rows)
            {
                yield return rowProcessor(row);
            }
        }

		/// <summary>
		/// Gets the first row from the result of executing a SQL statement with the specified command type.
		/// </summary>
		/// <param name="sqlStatement">The SQL statement.</param>
		/// <param name="commandType">The command type.</param>
		/// <param name="dbParameters">The database parameters.</param>
		/// <returns>The first DataRow from the result set.</returns>
		public virtual DataRow GetFirstRow(string sqlStatement, CommandType commandType, params DbParameter[] dbParameters)
		{
			return GetDataTable(sqlStatement, commandType, dbParameters).Rows[0];
		}

        /// <summary>
        /// Gets the first row from the result of executing a SQL statement with dictionary parameters.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">Dictionary of parameter names and values.</param>
        /// <returns>The first DataRow from the result set.</returns>
        public virtual DataRow GetFirstRow(string sqlStatement, Dictionary<string, object> dbParameters)
        {
            return GetFirstRow(sqlStatement, dbParameters.ToDbParameters(this).ToArray());
        }

        /// <summary>
        /// Gets the first row from the result of executing a SQL statement, or a new empty row if no results.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <returns>The first DataRow from the result set, or an empty DataRow.</returns>
        public virtual DataRow GetFirstRow(string sqlStatement, params DbParameter[] dbParameters)
        {
            DataTable table = GetDataTable(sqlStatement, CommandType.Text, dbParameters);
            if(table.Rows.Count > 0)
            {
                return table.Rows[0];
            }
            return table.NewRow();
        }

        /// <summary>
        /// Executes a SQL statement with dynamic parameters and returns the results as a DataTable.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dynamicParameters">Dynamic object whose properties become parameters.</param>
        /// <returns>A DataTable containing the results.</returns>
        public virtual DataTable GetDataTable(string sqlStatement, object dynamicParameters)
        {
            return GetDataTable(sqlStatement, dynamicParameters.ToDbParameters(this).ToArray());
        }

        /// <summary>
        /// Executes a SQL statement with dictionary parameters and returns the results as a DataTable.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="parameters">Dictionary of parameter names and values.</param>
        /// <returns>A DataTable containing the results.</returns>
        public virtual DataTable GetDataTable(string sqlStatement, Dictionary<string, object> parameters)
        {
            return GetDataTable(sqlStatement, parameters.ToDbParameters(this).ToArray());
        }

        /// <summary>
        /// Executes the SQL from the specified ISqlStringBuilder and returns the results as a DataTable.
        /// </summary>
        /// <param name="sqlStringBuilder">The SQL string builder.</param>
        /// <returns>A DataTable containing the results.</returns>
        public virtual DataTable GetDataTable(ISqlStringBuilder sqlStringBuilder)
        {
            return GetDataTable(sqlStringBuilder.ToString(), GetDbParameters(sqlStringBuilder));
        }
        
        /// <summary>
        /// Executes a SQL statement and returns the results as a DataTable.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <returns>A DataTable containing the results.</returns>
        public virtual DataTable GetDataTable(string sqlStatement, params DbParameter[] dbParameters)
        {
            return GetDataTable(sqlStatement, CommandType.Text, dbParameters);
        }

        /// <summary>
        /// Executes a SQL statement with the specified command type and returns the results as a DataTable.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="dbParameters">The database parameters.</param>
        /// <returns>A DataTable containing the results.</returns>
        public virtual DataTable GetDataTable(string sqlStatement, CommandType commandType, params DbParameter[] dbParameters)
        {
            DbProviderFactory providerFactory = ServiceProvider.Get<DbProviderFactory>();
            DbConnection conn = GetDbConnection();
            DataTable table = new DataTable();
            try
            {
				DbCommand command = BuildCommand(sqlStatement, commandType, dbParameters, providerFactory, conn);
                FillTable(table, command);
            }
            finally
            {
                ReleaseConnection(conn);
            }

            return table;
        }

		/// <summary>
		/// Creates a new Dao instance of type T associated with this database.
		/// </summary>
		/// <typeparam name="T">The Dao type to create.</typeparam>
		/// <returns>A new instance of T.</returns>
		public T New<T>() where T : IDao, new()
		{
			return typeof(T).Construct<T>(this);
		}

		/// <summary>
		/// Saves the specified Dao instance to this database and returns it.
		/// </summary>
		/// <typeparam name="T">The Dao type.</typeparam>
		/// <param name="dao">The Dao instance to save.</param>
		/// <returns>The saved Dao instance.</returns>
		public T Save<T>(T dao) where T: IDao, new()
		{
			dao.Save(this);
			return dao;
		}

		/// <summary>
		/// Gets a service of type T from the service provider.
		/// </summary>
		/// <typeparam name="T">The service type to resolve.</typeparam>
		/// <returns>The resolved service instance.</returns>
		public T GetService<T>()
		{
			return ServiceProvider.Get<T>();
		}
		
		/// <summary>
		/// Gets the primary key ID value from the specified Dao instance.
		/// </summary>
		/// <param name="dao">The Dao instance.</param>
		/// <returns>The ID value as a nullable long.</returns>
		public virtual long? GetIdValue(IDao dao)
		{
			string keyColumnName = dao.KeyColumnName;
			DataRow row = dao.DataRow;
			return GetLongValue(keyColumnName, row);
		}

		/// <summary>
		/// Gets a long value from the specified column in the given DataRow.
		/// </summary>
		/// <param name="columnName">The column name.</param>
		/// <param name="row">The DataRow to read from.</param>
		/// <returns>The long value, or null if the value is null or DBNull.</returns>
		public virtual long? GetLongValue(string columnName, DataRow row)
		{
			object value = row[columnName];
			if (value != null && value != DBNull.Value)
			{
                return new long?(Convert.ToInt64(value));
			}

            return new long?();
		}

		/// <summary>
		/// Gets the primary key ID value for the specified Dao type from a DataRow.
		/// </summary>
		/// <typeparam name="T">The Dao type whose key column to use.</typeparam>
		/// <param name="row">The DataRow to read from.</param>
		/// <returns>The ID value as a nullable long.</returns>
		public virtual long? GetIdValue<T>(DataRow row) where T: IDao, new()
		{
			return this.GetLongValue(Dao.GetKeyColumnName(typeof(T)), row);
		}

        /// <summary>
        /// Creates a new database connection using the configured provider factory and connection string.
        /// </summary>
        /// <returns>A new DbConnection instance.</returns>
        public virtual DbConnection CreateConnection()
        {
            DbConnection? conn = ServiceProvider.Get<DbProviderFactory>().CreateConnection();
            conn!.ConnectionString = ConnectionString;
            return conn;
        }

        /// <summary>
        /// Gets database parameters from the SQL string builder using the configured parameter builder.
        /// </summary>
        /// <param name="sqlStringBuilder">The SQL string builder containing parameter information.</param>
        /// <returns>An array of DbParameter instances.</returns>
        public virtual DbParameter[] GetDbParameters(ISqlStringBuilder sqlStringBuilder)
        {
            return GetService<IParameterBuilder>().GetParameters(sqlStringBuilder);
        }
        
        /// <summary>
        /// Creates a new DbCommand using the configured provider factory.
        /// </summary>
        /// <returns>A new DbCommand instance.</returns>
        public virtual DbCommand CreateCommand()
        {
            return ServiceProvider.Get<DbProviderFactory>().CreateCommand()!;
        }

        /// <summary>
        /// Creates a connection string builder cast to the specified type.
        /// </summary>
        /// <typeparam name="T">The connection string builder type.</typeparam>
        /// <returns>A connection string builder instance of type T.</returns>
        public virtual T CreateConnectionStringBuilder<T>() where T : DbConnectionStringBuilder, new()
        {
            return (T)CreateConnectionStringBuilder();
        }

        /// <summary>
        /// Creates a new database parameter with the specified name and value.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>A new DbParameter instance.</returns>
        public virtual DbParameter CreateParameter(string name, object value)
        {
            return ServiceProvider.Get<IParameterBuilder>().BuildParameter(name, value);
        }

        /// <summary>
        /// Creates a new connection string builder using the configured provider factory.
        /// </summary>
        /// <returns>A new DbConnectionStringBuilder instance.</returns>
        public virtual DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return ServiceProvider.Get<DbProviderFactory>().CreateConnectionStringBuilder()!;
        }

        /// <summary>
        /// Executes a SQL statement and returns the results as a DataSet.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="dbParamaters">The database parameters.</param>
        /// <returns>A DataSet containing the results.</returns>
        public virtual DataSet GetDataSetFromSql(string sqlStatement, CommandType commandType, params DbParameter[] dbParamaters)
        {
            return GetDataSetFromSql(sqlStatement, commandType, true, dbParamaters);
        }

        /// <summary>
        /// Executes a SQL statement and returns a DataSet with optional connection release control.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="releaseConnection">Whether to release the connection after execution.</param>
        /// <param name="dbParamaters">The database parameters.</param>
        /// <returns>A DataSet containing the results.</returns>
        public virtual DataSet GetDataSetFromSql(string sqlStatement, CommandType commandType, bool releaseConnection, params DbParameter[] dbParamaters)
        {
            DbConnection conn = GetDbConnection();
            return GetDataSetFromSql(sqlStatement, commandType, releaseConnection,  conn, dbParamaters);
        }

        /// <summary>
        /// Executes a SQL statement on the specified connection and returns a DataSet.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="releaseConnection">Whether to release the connection after execution.</param>
        /// <param name="conn">The database connection.</param>
        /// <param name="dbParamaters">The database parameters.</param>
        /// <returns>A DataSet containing the results.</returns>
        public virtual DataSet GetDataSetFromSql(string sqlStatement, CommandType commandType, bool releaseConnection, DbConnection conn, params DbParameter[] dbParamaters)
        {
            return GetDataSetFromSql(sqlStatement, commandType, releaseConnection, conn, null!, dbParamaters);
        }

		/// <summary>
		/// Executes a SQL statement with a transaction and returns a DataSet.
		/// </summary>
		/// <param name="sqlStatement">The SQL statement.</param>
		/// <param name="commandType">The command type.</param>
		/// <param name="releaseConnection">Whether to release the connection after execution.</param>
		/// <param name="conn">The database connection.</param>
		/// <param name="tx">The database transaction, or null.</param>
		/// <param name="dbParamaters">The database parameters.</param>
		/// <returns>A DataSet containing the results.</returns>
		public virtual DataSet GetDataSetFromSql(string sqlStatement, CommandType commandType, bool releaseConnection, DbConnection conn, DbTransaction tx, params DbParameter[] dbParamaters)
		{
			return GetDataSetFromSql<object>(sqlStatement, commandType, releaseConnection, conn, tx, dbParamaters);
		}

        /// <summary>
        /// Executes a SQL statement with a transaction and returns a DataSet named after the Dao type's connection name.
        /// </summary>
        /// <typeparam name="T">The Dao type used for naming the DataSet.</typeparam>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="releaseConnection">Whether to release the connection after execution.</param>
        /// <param name="conn">The database connection.</param>
        /// <param name="tx">The database transaction, or null.</param>
        /// <param name="dbParamaters">The database parameters.</param>
        /// <returns>A DataSet containing the results.</returns>
        public virtual DataSet GetDataSetFromSql<T>(string sqlStatement, CommandType commandType, bool releaseConnection, DbConnection conn, DbTransaction tx, params DbParameter[] dbParamaters)
        {
            DbProviderFactory providerFactory = ServiceProvider.Get<DbProviderFactory>();
            string dataSetName = Dao.ConnectionName<T>().Or(8.RandomLetters());
			DataSet set = new DataSet(dataSetName);
            try
            {
                DbCommand command = BuildCommand(sqlStatement, commandType, dbParamaters, providerFactory, conn, tx);
                FillDataSet(set, command);
            }
            finally
            {
                if (releaseConnection)
                {
                    ReleaseConnection(conn);
                }
            }

            return set;
        }

		protected internal virtual AssignValue GetAssignment(string keyColumn, object value, Func<string, string> columnNameformatter = null!)
		{
			return new AssignValue(keyColumn, value, columnNameformatter);
		}

        // TODO: refactor all calls that require a connection, remove the connection parameter and set the connection right before executing the command
        protected internal virtual DbCommand PrepareCommand(string sqlStatement, CommandType commandType, DbParameter[] dbParameters, DbConnection conn)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            DbProviderFactory providerFactory = ServiceProvider.Get<DbProviderFactory>();
            DbCommand cmd = BuildCommand(sqlStatement, commandType, dbParameters, providerFactory, conn);
            return cmd;
        }

        protected internal virtual DbCommand BuildCommand(string sqlStatement, CommandType commandType, DbParameter[] dbParameters, DbProviderFactory providerFactory, DbConnection conn, DbTransaction tx  =null!)
        {
            DbCommand? command = providerFactory.CreateCommand();
            command!.Connection = conn;
            if (tx != null)
            {
                command.Transaction = tx;
            }
            command.CommandText = sqlStatement;
            command.CommandType = commandType;
            command.CommandTimeout = 10000;            
            command.Parameters.AddRange(dbParameters);
            return command;
        }

        protected void FillTable(DataTable table, DbCommand command)
        {
            DbDataAdapter? adapter = ServiceProvider.Get<DbProviderFactory>().CreateDataAdapter();
            adapter!.SelectCommand = command;
            adapter.Fill(table);
        }

        protected void FillDataSet(DataSet dataSet, DbCommand command)
        {
            DbProviderFactory factory = ServiceProvider.Get<DbProviderFactory>();
            DbDataAdapter? adapter = factory.CreateDataAdapter();
            adapter!.SelectCommand = command;
            adapter.Fill(dataSet);
        }

        /// <summary>
        /// Gets or sets the connection string used to connect to the database.
        /// </summary>
        public virtual string? ConnectionString
        {
            get;
            set;
        }

		/// <summary>
		/// Determines whether this database equals another by comparing connection strings.
		/// </summary>
		/// <param name="obj">The object to compare.</param>
		/// <returns>True if the connection strings match.</returns>
		public override bool Equals(object? obj)
		{
			if (obj!.GetType() == this.GetType() && 
				!string.IsNullOrEmpty(ConnectionString))
			{
				Database? db = obj as Database;
				return db!.ConnectionString!.Equals(this.ConnectionString);
			}
			else
			{
				return base.Equals(obj);
			}
		}

		/// <summary>
		/// Returns a hash code based on the connection string.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode()
		{
			if(!string.IsNullOrEmpty(ConnectionString))
			{
				return ConnectionString.GetHashCode();
			}
			else
			{
				return base.GetHashCode();
			}
		}

        /// <summary>
        /// Gets a new database connection in the open state.
        /// </summary>
        /// <returns>An open DbConnection.</returns>
        public DbConnection GetOpenDbConnection()
        {
            DbConnection conn = GetDbConnection();
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Gets a database connection from the connection manager.
        /// </summary>
        /// <returns>A DbConnection instance.</returns>
        public DbConnection GetDbConnection()
        {
            return ConnectionManager.GetDbConnection();
        }
        
        AutoResetEvent _resetEvent;
        protected readonly object connectionLock = new object();
        /// <summary>
        /// Releases a database connection back to the connection manager.
        /// </summary>
        /// <param name="conn">The connection to release.</param>
        public virtual void ReleaseConnection(DbConnection conn)
        {
            ConnectionManager.ReleaseConnection(conn);
        }

        /// <summary>
        /// Attempts to ensure the schema for Dao types in the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly containing Dao types.</param>
        /// <param name="logger">Optional logger for error reporting.</param>
        /// <returns>The schema initialization status.</returns>
        public EnsureSchemaStatus TryEnsureSchema(Assembly assembly, ILogger logger = null!)
        {
            Type? daoType = assembly.GetTypes().FirstOrDefault(d => d.IsSubclassOf(typeof(Dao)));
            if (daoType == null)
            {
                return EnsureSchemaStatus.Invalid;
            }
            else
            {
                return TryEnsureSchema(daoType, logger);
            }
        }

        /// <summary>
        /// Attempts to ensure the schema for the specified Dao type T.
        /// </summary>
        /// <typeparam name="T">The Dao type whose schema to ensure.</typeparam>
        /// <param name="logger">Optional logger for error reporting.</param>
        /// <returns>The schema initialization status.</returns>
        public EnsureSchemaStatus TryEnsureSchema<T>(ILogger logger = null!)
        {
            return TryEnsureSchema(typeof(T), logger);
        }

		/// <summary>
		/// Attempts to ensure the schema for the specified type.
		/// </summary>
		/// <param name="type">The Dao type whose schema to ensure.</param>
		/// <param name="logger">Optional logger for error reporting.</param>
		/// <returns>The schema initialization status.</returns>
		public EnsureSchemaStatus TryEnsureSchema(Type type, ILogger logger = null!)
		{
            return TryEnsureSchema(type, out Exception e, logger);
        }

		/// <summary>
		/// Attempts to ensure the schema for the specified type, outputting any exception.
		/// </summary>
		/// <param name="type">The Dao type whose schema to ensure.</param>
		/// <param name="ex">Output parameter receiving any exception that occurred.</param>
		/// <param name="logger">Optional logger for error reporting.</param>
		/// <returns>The schema initialization status.</returns>
		public EnsureSchemaStatus TryEnsureSchema(Type type, out Exception ex, ILogger logger = null!)
		{
			return TryEnsureSchema(type, false, out ex, logger);
		}

		/// <summary>
		/// Attempts to ensure the schema for the specified type with optional force re-creation.
		/// </summary>
		/// <param name="type">The Dao type whose schema to ensure.</param>
		/// <param name="force">If true, re-creates the schema even if already done.</param>
		/// <param name="ex">Output parameter receiving any exception that occurred.</param>
		/// <param name="logger">Optional logger for error reporting.</param>
		/// <returns>The schema initialization status.</returns>
		public virtual EnsureSchemaStatus TryEnsureSchema(Type type, bool force, out Exception ex, ILogger logger = null!)
		{
            ex = null!;
            EnsureSchemaStatus result;
            try
            {
                string schemaName = Dao.RealConnectionName(type);
                if (!SchemaNames.Contains(schemaName) || force)
                {
                    _schemaNames.Add(schemaName);
                    SchemaWriter schema = ServiceProvider.Get<SchemaWriter>();
                    schema.WriteSchemaScript(type);
                    ExecuteSql(schema, ServiceProvider.Get<IParameterBuilder>());
                    result = EnsureSchemaStatus.Success;
                }
                else
                {
                    result = EnsureSchemaStatus.AlreadyDone;
                }
            }
            catch (Exception e)
            {
                ex = e;
                result = EnsureSchemaStatus.Error;
                logger = (logger ?? Log.Default)!;
                logger!.AddEntry("Non fatal error occurred trying to write schema for type {0}: {1}", LogEventType.Warning, ex, type.Name, ex.Message);
            }
            return result;
		}

		Func<ColumnAttribute, string> _columnNameProvider = null!;
		/// <summary>
		/// Gets or sets the function used to format column names in SQL statements.
		/// </summary>
		public virtual Func<ColumnAttribute, string> ColumnNameProvider
		{
			get
			{
				if (_columnNameProvider == null)
				{
					_columnNameProvider = (c) => $"[{c.Name}]";
				}

				return _columnNameProvider;
			}
			set => _columnNameProvider = value;
        }

        protected List<DbConnection> Connections
        {
            get
            {
                return _connections.ToList();
            }
        }
		
		private QuerySet ExecuteQuery<T>() where T : IDao, new()
		{
			QuerySet query = new QuerySet();
			query.Select<T>();
			query.Execute(this);
			return query;
		}

		static readonly object initEnumLock = new object();
		private void InitEnumValues<EnumType, T>(string valueColumn, string nameColumn) where T : IDao, new()
		{
			FieldInfo[] fields = typeof(EnumType).GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach (FieldInfo field in fields)
			{
				T entry = new T();
				entry.SetValue(valueColumn, field.GetRawConstantValue()!);
				entry.SetValue(nameColumn, field.Name);
				entry.Save();
			}
		}

        private static List<string> GetColumnNames(DbDataReader reader)
        {
            List<string> columnNames = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames.Add(reader.GetName(i));
            }

            return columnNames;
        }
    }
}
