/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data.Common;
using System.Configuration;

namespace Bam.Data.MsSql
{
    public class MySqlDatabaseInitializer: DefaultDatabaseInitializer
    {
        public MySqlDatabaseInitializer()
        {
        }

        public MySqlDatabaseInitializer(params string[] ignoreConnectionNames)
        {
            this.Ignore(ignoreConnectionNames);
        }

		public MySqlDatabaseInitializer(params Type[] ignoreConnectionsForTypes)
        {
            this.Ignore(ignoreConnectionsForTypes);
        }

        public override Database GetDatabase(ConnectionStringSettings conn, DbProviderFactory factory)
        {
            Database db = base.GetDatabase(conn, factory);
            MySqlRegistrar.Register(db.ServiceProvider);
            return db;
        }
    }
}
