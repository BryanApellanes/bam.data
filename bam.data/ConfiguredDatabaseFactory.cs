/*
	Copyright © Bryan Apellanes 2015  
*/

using Bam.DependencyInjection;
using Bam.Logging;
using Bam.Services;

namespace Bam.Data
{
    public abstract class ConfiguredDatabaseFactory
    {
        static ConfiguredDatabaseFactory()
        {
            AppDomain.CurrentDomain.GetAssemblies().Each(assembly =>
            {
                try
                {
                    assembly.GetTypes().Each(type =>
                    {
                        if (type.IsSubclassOf(typeof(ConfiguredDatabaseFactory)))
                        {
                            Factories[type] = type.Construct<ConfiguredDatabaseFactory>();
                        }
                    });
                }
                catch (Exception ex)
                {
                    Log.AddEntry("An error occurred in the ConfiguredDatabaseFactory type initializer: {0}", ex, ex.Message);
                }
            });
        }

        static Dictionary<Type, ConfiguredDatabaseFactory> _factories;
        static object _factoriesSync = new object();
        public static Dictionary<Type, ConfiguredDatabaseFactory> Factories
        {
            get
            {
                return _factoriesSync.DoubleCheckLock(ref _factories, () => new Dictionary<Type, ConfiguredDatabaseFactory>());
            }
        }

        public Database Create(Type type, bool initializeSchema = true)
        {
            Args.ThrowIfNull(ConnectionInfo, "Connection");
            Args.ThrowIfNull(RegistrarCaller, "RegistrarCaller");
            if (initializeSchema)
            {
                Args.ThrowIfNull(SchemaInitializer, "SchemaInitializer");
            }

            Database database = new Database();
            database.ServiceProvider = new DependencyProvider();
            
            Configure(database);
            
            return database;
        }

        protected abstract void Configure(Database database);

        public ConnectionInfo ConnectionInfo
        {
            get;
            set;
        }

        public IRegistrarCaller RegistrarCaller
        {
            get;
            set;
        }

        public SchemaInitializer SchemaInitializer
        {
            get;
            set;
        }
    }
}
