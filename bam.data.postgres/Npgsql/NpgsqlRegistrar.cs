/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bam.Data;
using Bam.Incubation;
using Bam;
using Bam.Data;

namespace Bam.Data
{
    public class NpgsqlRegistrar
    {
        static NpgsqlRegistrar()
        {
            
        }

        /// <summary>
		/// Register the Npgsql implementation of IParameterBuilder, SchemaWriter and QuerySet for the 
        /// database associated with the specified connectionName.
        /// </summary>
        public static void Register(string connectionName)
        {
            Register(Db.For(connectionName).ServiceProvider);
        }

        /// <summary>
		/// Register the Npgsql implementation of IParameterBuilder, SchemaWriter and QuerySet for the 
        /// database associated with the specified type.
        /// </summary>
        public static void Register(Type daoType)
        {
            Register(Db.For(daoType).ServiceProvider);
        }

        /// <summary>
		/// Register the Npgsql implementation of IParameterBuilder, SchemaWriter and QuerySet for the 
        /// database associated with the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Register<T>() where T: IDao
        {
            Register(Db.For<T>().ServiceProvider);
        }

        public static void Register(IDatabase database)
        {
            Register(database.ServiceProvider);
        }

        /// <summary>
        /// Register the Npgsql implementation of IParameterBuilder, SchemaWriter and QuerySet  
        /// into the specified incubator
        /// </summary>
        /// <param name="incubator"></param>
        public static void Register(DependencyProvider incubator)
        {
            NpgsqlParameterBuilder b = new NpgsqlParameterBuilder();
            incubator.Set<IParameterBuilder>(b);

			incubator.Set<SqlStringBuilder>(() => new NpgsqlSqlStringBuilder());
            incubator.Set<SchemaWriter>(() => new NpgsqlSqlStringBuilder());
            incubator.Set<QuerySet>(() => new NpgsqlQuerySet());
            incubator.Set<IDataTypeTranslator>(() => new DataTypeTranslator());
        }
    }
}
