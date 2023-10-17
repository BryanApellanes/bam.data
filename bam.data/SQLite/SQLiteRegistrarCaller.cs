/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net.Data
{
    /// <summary>
    /// Registrar caller used to register SQLite as the 
    /// handler for a database
    /// </summary>
    public class SQLiteRegistrarCaller: IRegistrarCaller
    {
        public void Register(IDatabase database)
        {
            SQLiteRegistrar.Register(database);
        }
        public void Register(string connectionName)
        {
            SQLiteRegistrar.Register(connectionName);
        }

        public void Register(Type daoType)
        {
            SQLiteRegistrar.Register(daoType);
        }

        public void Register<T>() where T : IDao
        {
            SQLiteRegistrar.Register<T>();
        }

        public void Register(Incubation.DependencyProvider incubator)
        {
            SQLiteRegistrar.Register(incubator);
        }
    }
}
