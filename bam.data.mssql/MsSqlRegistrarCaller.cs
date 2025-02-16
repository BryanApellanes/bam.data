/*
	Copyright © Bryan Apellanes 2015  
*/

using Bam.DependencyInjection;
using Bam.Services;

namespace Bam.Data
{
    public class MsSqlRegistrarCaller: IRegistrarCaller
    {
        public void Register(IDatabase database)
        {
            MsSqlRegistrar.Register(database);
        }

        public void Register(string connectionName)
        {
            MsSqlRegistrar.Register(connectionName);
        }

        public void Register(Type daoType)
        {
            MsSqlRegistrar.Register(daoType);
        }

        public void Register<T>() where T : IDao
        {
            MsSqlRegistrar.Register<T>();
        }

        public void Register(DependencyProvider incubator)
        {
            MsSqlRegistrar.Register(incubator);
        }
    }
}
