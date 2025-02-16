/*
	Copyright © Bryan Apellanes 2015  
*/

using Bam.DependencyInjection;
using Bam.Services;

namespace Bam.Data
{
    /// <summary>
    /// Registrar caller used to register Oracle as the 
    /// handler for a database
    /// </summary>
	public class OracleRegistrarCaller : IRegistrarCaller
    {
        public void Register(IDatabase database)
        {
            OracleRegistrar.Register(database);
        }
        public void Register(string connectionName)
        {
            OracleRegistrar.Register(connectionName);
        }

        public void Register(Type daoType)
        {
            OracleRegistrar.Register(daoType);
        }

        public void Register<T>() where T : IDao
        {
            OracleRegistrar.Register<T>();
        }

        public void Register(DependencyProvider incubator)
        {
            OracleRegistrar.Register(incubator);
        }
    }
}
