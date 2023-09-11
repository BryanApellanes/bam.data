/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Net.Incubation;

namespace Bam.Net.Data
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

        public void Register(Incubator incubator)
        {
            NpgsqlRegistrar.Register(incubator);
        }
    }
}
