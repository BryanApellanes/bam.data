/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bam.Incubation;
using System.Data.SQLite;
using Bam.Data;
using Bam;
using System.Reflection;
using System.IO;
using Bam.Data;

namespace Bam.Data
{
    public class SQLiteRegistrar
    {
        static SQLiteRegistrar()
        {
        }

        public static void OutputSQLiteFactoryAssemblyQualifiedName()
        {
            System.Console.WriteLine(SQLiteFactoryAssemblyQualifiedName());
        }

        public static string SQLiteFactoryAssemblyQualifiedName()
        {
            return typeof(SQLiteFactory).AssemblyQualifiedName;
        }

        /// <summary>
        /// Register the SQLite components with the ServiceProvider 
        /// of the specified database.  This Register method wil
        /// not call SetInitializerAndConnectionStringResolver
        /// like the other Register methods do.
        /// </summary>
        /// <param name="database"></param>
        public static void Register(IDatabase database)
        {
            Register(database.ServiceProvider);
        }

        /// <summary>
        /// Registers SQLite as the handler for the specified connection name.
        /// This dao handler will register apropriate DatabaseInitializer and
        /// ConnectionStringResolver. 
        /// </summary>
        /// <param name="connectionName"></param>
        public static void Register(string connectionName)
        {
            SetInitializerAndConnectionStringResolver(connectionName);
            Register(Db.For(connectionName).ServiceProvider);
        }
        /// <summary>
        /// Registers SQLite as the handler for the specified type.
        /// This dao handler will register apropriate DatabaseInitializer and
        /// ConnectionStringResolver.  This behavior is different compared to the
        /// SqlClientRegistrar's Register method.
        /// </summary>
        public static void Register(Type daoType)
        {
            SetInitializerAndConnectionStringResolver(daoType);
            Register(Db.For(daoType).ServiceProvider);
        }
        /// <summary>
        /// Registers SQLite as the handler for the specified generic type T.
        /// This dao handler will register apropriate DatabaseInitializer and
        /// ConnectionStringResolver.  This behavior is different compared to the
        /// SqlClientRegistrar's Register method.
        /// </summary>
        public static void Register<T>() where T : IDao
        {
            SetInitializerAndConnectionStringResolver(typeof(T));
            Register(Db.For<T>().ServiceProvider);
        }

        private static void SetInitializerAndConnectionStringResolver(Type daoType)
        {
            SetInitializerAndConnectionStringResolver(Dao.ConnectionName(daoType));
        }

        private static void SetInitializerAndConnectionStringResolver(string connectionName)
        {
            DatabaseInitializers.Ignore<DefaultDatabaseInitializer>(connectionName);
            DatabaseInitializers.AddInitializer(new SQLiteDatabaseInitializer());
            SQLiteConnectionStringResolver.Register();
        }

        public static void Register(DependencyProvider incubator)
        {
            SQLiteConnectionStringResolver.Register();

            incubator.Set<IParameterBuilder>(() => new SQLiteParameterBuilder());
            incubator.Set<SqlStringBuilder>(() => new SQLiteSqlStringBuilder());
            incubator.Set<SchemaWriter>(() => new SQLiteSqlStringBuilder());
            incubator.Set<QuerySet>(() => new SQLiteQuerySet());
            incubator.Set<IDataTypeTranslator>(() => new DataTypeTranslator());
        }

        /// <summary>
        /// Registers SQLite as the fallback initializer for all databases.
        /// This means that if the default database initializers fail, SQLite
        /// will register itself as the database container and retry database
        /// initialization.
        /// </summary>
        public static void RegisterFallback()
        {
            Bam.Data.Db.DefaultContainer.FallBack = (connectionName, dbDictionary) =>
            {
                Register(connectionName);
            };
        }
    }
}
