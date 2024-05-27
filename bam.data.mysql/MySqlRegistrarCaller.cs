/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Incubation;

namespace Bam.Data
{
    public class MySqlRegistrarCaller: IRegistrarCaller
    {
        public void Register(IDatabase database)
        {
            MySqlRegistrar.Register(database);
        }

        public void Register(string connectionName)
        {
            MySqlRegistrar.Register(connectionName);
        }

        public void Register(Type daoType)
        {
            MySqlRegistrar.Register(daoType);
        }

        public void Register<T>() where T : IDao
        {
            MySqlRegistrar.Register<T>();
        }

        public void Register(DependencyProvider incubator)
        {
            MySqlRegistrar.Register(incubator);
        }
    }
}
