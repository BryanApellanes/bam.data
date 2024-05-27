/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bam.Data;
using System.Data;
using System.Data.Common;
using System.Configuration;

namespace Bam.Data
{
    public class SQLiteDatabaseInitializer: DefaultDatabaseInitializer
    {
        public SQLiteDatabaseInitializer()
        {
        }

        public SQLiteDatabaseInitializer(params string[] ignoreConnectionNames)
        {
            this.Ignore(ignoreConnectionNames);
        }

        public SQLiteDatabaseInitializer(params Type[] ignoreConnectionsForTypes)
        {
            this.Ignore(ignoreConnectionsForTypes);
        }

        public override Database GetDatabase(ConnectionStringSettings conn, DbProviderFactory factory)
        {
            Database db = base.GetDatabase(conn, factory);
            SQLiteRegistrar.Register(db.ServiceProvider);
            return db;
        }
    }
}
