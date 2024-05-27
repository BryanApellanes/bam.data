/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bam.Incubation;
using Oracle.ManagedDataAccess;
using Oracle.ManagedDataAccess.Client;
using Bam.Data;
using Bam;
using System.Reflection;
using System.IO;
using Bam.Data;

namespace Bam.Data
{
	public static class OracleRegistrar
    {
        public static void OutputAssemblyQualifiedName()
        {
            System.Console.WriteLine(OracleAssemblyQualifiedName());
        }

        public static string OracleAssemblyQualifiedName()
        {
            return OracleClientFactory.Instance.GetType().AssemblyQualifiedName;
        }

        /// <summary>
        /// Register the Oracle components with the ServiceProvider 
        /// of the specified database.  This Register method will
        /// not call SetInitializerAndConnectionStringResolver
        /// like the other Register methods do.
        /// </summary>
        /// <param name="database"></param>
        public static void Register(IDatabase database)
        {
            Register(database.ServiceProvider);
        }

        /// <summary>
        /// Registers Oracle as the handler for the specified connection name.
        /// This dao handler will register apropriate DatabaseInitializer and
        /// ConnectionStringResolver.  This behavior is different compared to the
        /// SqlClientRegistrar's Register method.
        /// </summary>
        /// <param name="connectionName"></param>
        public static void Register(string connectionName)
        {
            Register(Db.For(connectionName).ServiceProvider);
        }

        public static void Register(Type daoType)
        {
            Register(Db.For(daoType).ServiceProvider);
        }

        public static void Register<T>() where T : IDao
        {
            Register(Db.For<T>().ServiceProvider);
        }

        public static void Register(DependencyProvider incubator)
        {
            incubator.Set<IParameterBuilder>(() => new OracleParameterBuilder());
            incubator.Set<SqlStringBuilder>(() => new OracleSqlStringBuilder());
            incubator.Set<SchemaWriter>(() => new OracleSqlStringBuilder());
            incubator.Set<QuerySet>(() => new OracleQuerySet());
            incubator.Set<IDataTypeTranslator>(() => new DataTypeTranslator());
        }
    }
}
