/*
	Copyright © Bryan Apellanes 2015  
*/

using Bam.DependencyInjection;
using Bam.Services;

namespace Bam.Data
{
    public class FirebirdSqlRegistrarCaller : IRegistrarCaller
    {
        public void Register(IDatabase database)
        {
            FirebirdSqlRegistrar.Register(database);
        }

        public void Register(string connectionName)
        {
            FirebirdSqlRegistrar.Register(connectionName);
        }

        public void Register(Type daoType)
        {
            FirebirdSqlRegistrar.Register(daoType);
        }

        public void Register<T>() where T : IDao
        {
            FirebirdSqlRegistrar.Register<T>();
        }

        public void Register(DependencyProvider incubator)
        {
            FirebirdSqlRegistrar.Register(incubator);
        }
    }
}
