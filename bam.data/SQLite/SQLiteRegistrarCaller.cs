/*
	Copyright © Bryan Apellanes 2015  
*/

using Bam.DependencyInjection;
using Bam.Services;

namespace Bam.Data
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

        public void Register(DependencyProvider incubator)
        {
            SQLiteRegistrar.Register(incubator);
        }
    }
}
