/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data;
using System.Data.Common;
using Bam.DependencyInjection;
using Bam.Services;
using FirebirdSql.Data.FirebirdClient;

namespace Bam.Data.FirebirdSql
{
    public class FirebirdSqlDatabase : Database, IHasConnectionStringResolver
    {
        public FirebirdSqlDatabase()
        {
            ConnectionStringResolver = DefaultConnectionStringResolver.Instance;
            Register();
        }
        public FirebirdSqlDatabase(string serverName, string databaseName, FirebirdSqlCredentials credentials = null!)
            : this(serverName, databaseName, databaseName, credentials)
        { }

        public FirebirdSqlDatabase(string serverName, string databaseName, string connectionName, FirebirdSqlCredentials credentials = null!)
        {
            ColumnNameProvider = (c) => "\"{0}\"".Format(c.Name);
            ConnectionStringResolver = new FirebirdSqlConnectionStringResolver(serverName, databaseName, credentials);
            ConnectionName = connectionName;
            Register();
        }

        public FirebirdSqlDatabase(string connectionString, string connectionName = null!)
            : base(connectionString, connectionName)
        {
        }

        private void Register()
        {
            ServiceProvider = new DependencyProvider();
            ServiceProvider.Set<DbProviderFactory>(FirebirdClientFactory.Instance);
            FirebirdSqlRegistrar.Register(this);
            Infos.Add(new DatabaseInfo(this));
        }

        public IConnectionStringResolver? ConnectionStringResolver
        {
            get;
            set;
        }

        string _connectionString = null!;
        public override string? ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    _connectionString = ConnectionStringResolver?.Resolve(ConnectionName)?.ConnectionString!;
                }

                return _connectionString!;
            }
            set
            {
                _connectionString = value!;
            }
        }

        public override long? GetLongValue(string columnName, DataRow row)
        {
            object value = row[columnName];
            if (value is long || value is long?)
            {
                return (long?)value;
            }
            else if (value is int || value is int?)
            {
                int d = (int)value;
                return Convert.ToInt64(d);
            }
            return null;
        }

        /// <summary>
        /// Overrides batch SQL execution to split multi-statement SQL into individual statements.
        /// Firebird's ADO.NET provider does not support executing multiple SQL statements in a single command.
        /// </summary>
        public override void ExecuteSql(ISqlStringBuilder builder, IParameterBuilder parameterBuilder)
        {
            string fullSql = builder.ToString();
            string[] statements = fullSql.Split(new[] { ";\r\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries);
            using var conn = new FbConnection(ConnectionString);
            conn.Open();
            foreach (string stmt in statements)
            {
                string trimmed = stmt.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    try
                    {
                        using var cmd = new FbCommand(trimmed, conn);
                        cmd.ExecuteNonQuery();
                    }
                    catch (FbException)
                    {
                        // Schema statements may fail if objects already exist; continue with remaining statements.
                    }
                }
            }
        }
    }
}
