/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data.Common;
using Microsoft.Data.SqlClient;
using Bam.DependencyInjection;
using Bam.Services;

namespace Bam.Data.MsSql
{
	public class MsSqlDatabase: Database, IHasConnectionStringResolver
	{
        public MsSqlDatabase()
        {
            ConnectionStringResolver = DefaultConnectionStringResolver.Instance;
            Register();
        }
		public MsSqlDatabase(string serverName, string databaseName, MsSqlCredentials? credentials = null)
			: this(serverName, databaseName, databaseName, credentials)
		{ }

		public MsSqlDatabase(string serverName, string databaseName, string connectionName, MsSqlCredentials? credentials = null)
			: base()
		{
			ConnectionStringResolver = new MsSqlConnectionStringResolver(serverName, databaseName, credentials);
			ConnectionName = connectionName;
            Register();
		}

        public MsSqlDatabase(MsSqlConnectionStringResolver connectionStringResolver)
        {
            ConnectionName = connectionStringResolver.DatabaseName;
            ConnectionStringResolver = connectionStringResolver;
            Register();
        }

        public MsSqlDatabase(string connectionString, string? connectionName = null) 
            : base(connectionString, connectionName)
        {
            Register();
        }

        private void Register()
        {
            ServiceProvider = new DependencyProvider();
            ServiceProvider.Set<DbProviderFactory>(SqlClientFactory.Instance);
            MsSqlRegistrar.Register(this);
            Infos.Add(new DatabaseInfo(this));
        }

		public IConnectionStringResolver? ConnectionStringResolver
		{
			get;
			set;
		}

		string? _connectionString;
		public override string? ConnectionString
		{
			get
			{
				if (string.IsNullOrEmpty(_connectionString))
				{
					_connectionString = ConnectionStringResolver?.Resolve(ConnectionName)?.ConnectionString;
				}

				return _connectionString;
			}
			set => _connectionString = value;
		}
	}
}
