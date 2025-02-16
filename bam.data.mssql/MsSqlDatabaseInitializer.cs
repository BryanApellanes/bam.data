/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data.Common;
using System.Configuration;

namespace Bam.Data.MsSql
{
    public class MsSqlDatabaseInitializer: DefaultDatabaseInitializer
    {
        public MsSqlDatabaseInitializer()
        {
        }

        public MsSqlDatabaseInitializer(params string[] ignoreConnectionNames)
        {
            this.Ignore(ignoreConnectionNames);
        }

        public MsSqlDatabaseInitializer(params Type[] ignoreConnectionsForTypes)
        {
            this.Ignore(ignoreConnectionsForTypes);
        }

        public override Database GetDatabase(ConnectionStringSettings conn, DbProviderFactory factory)
        {
            Database db = base.GetDatabase(conn, factory);
            MsSqlRegistrar.Register(db.ServiceProvider);
            return db;
        }
    }
}
