/*
	Copyright © Bryan Apellanes 2015  
*/

using Bam.DependencyInjection;
using Bam.Services;

namespace Bam.Data
{
    public class NpgsqlRegistrarCaller: IRegistrarCaller
    {
        public void Register(IDatabase database)
        {
            NpgsqlRegistrar.Register(database);
        }

        public void Register(string connectionName)
        {
            NpgsqlRegistrar.Register(connectionName);
        }

        public void Register(Type daoType)
        {
            NpgsqlRegistrar.Register(daoType);
        }

        public void Register<T>() where T : IDao
        {
            NpgsqlRegistrar.Register<T>();
        }

        public void Register(DependencyProvider incubator)
        {
            NpgsqlRegistrar.Register(incubator);
        }
    }
}
