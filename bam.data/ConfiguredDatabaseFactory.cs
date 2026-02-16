/*
	Copyright © Bryan Apellanes 2015  
*/

using Bam.DependencyInjection;
using Bam.Logging;
using Bam.Services;

namespace Bam.Data
{
    /// <summary>
    /// Abstract base class for factories that create configured Database instances using connection info, registrar callers, and schema initializers.
    /// </summary>
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
        /// <summary>
        /// Gets the dictionary of all discovered ConfiguredDatabaseFactory implementations keyed by type.
        /// </summary>
        public static Dictionary<Type, ConfiguredDatabaseFactory> Factories
        {
            get
            {
                return _factoriesSync.DoubleCheckLock(ref _factories, () => new Dictionary<Type, ConfiguredDatabaseFactory>());
            }
        }

        /// <summary>
        /// Creates a new Database instance using the configured connection info and schema initializer.
        /// </summary>
        /// <param name="type">The type to create a database for.</param>
        /// <param name="initializeSchema">If true, validates that a SchemaInitializer is set.</param>
        /// <returns>A configured Database instance.</returns>
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

        /// <summary>
        /// Gets or sets the connection information used to create databases.
        /// </summary>
        public ConnectionInfo ConnectionInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the registrar caller used to register Dao types.
        /// </summary>
        public IRegistrarCaller RegistrarCaller
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the schema initializer used to create database schemas.
        /// </summary>
        public SchemaInitializer SchemaInitializer
        {
            get;
            set;
        }
    }
}
