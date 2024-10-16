/*
	Copyright © Bryan Apellanes 2015  
*/

using Bam.Data.Npgsql;

namespace Bam.Data.Postgres
{
	public class PostgresConnectionStringResolver : NpgsqlConnectionStringResolver
	{
		public PostgresConnectionStringResolver(string serverName, string databaseName, NpgsqlCredentials credentials = null) : base(serverName, databaseName, credentials)
		{
		}
	}
}
