using Bam.Configuration;
using Bam.Logging;

namespace Bam.Data
{
    /// <summary>
    /// Sets database properties for a given set of object instances.
    /// </summary>
    /// <typeparam name="T">The type of database to use</typeparam>
    public abstract class DatabaseProvider<T> : IDatabaseProvider where T : IDatabase, new()
    {
        /// <summary>
        /// Gets or sets the logger used for error and diagnostic messages.
        /// </summary>
        public ILogger Logger
        {
            get; set;
        } = null!;

        /// <summary>
        /// Sets all properties on the specified instances, where the property 
        /// is writeable and is of type Database, to an instance of T 
        /// </summary>
        /// <param name="instances"></param>
        public virtual void SetDatabases(params object[] instances)
        {
            instances.Each(instance =>
            {
                Type type = instance.GetType();
                type.GetProperties().Where(pi => pi.CanWrite && pi.PropertyType.Equals(typeof(IDatabase))).Each(new { Instance = instance }, (ctx, pi) =>
                {
                    T db = GetSysDatabaseFor(instance);
                    if (pi.HasCustomAttributeOfType(out SchemasAttribute schemas))
                    {
                        TryEnsureSchemas(db, schemas.DaoSchemaTypes);
                    }
                    pi.SetValue(ctx.Instance, db);
                });
            });
        }

        /// <summary>
        /// Sets the default database for the specified Dao type.
        /// </summary>
        /// <typeparam name="TDao">The Dao type to configure.</typeparam>
        public virtual void SetDefaultDatabaseFor<TDao>() where TDao : IDao
        {
            SetDefaultDatabaseFor<TDao>(out IDatabase db);
        }

        /// <summary>
        /// Sets the default database for the specified Dao type and outputs the database instance.
        /// </summary>
        /// <typeparam name="TDao">The Dao type to configure.</typeparam>
        /// <param name="db">The database instance that was set as default.</param>
        public virtual void SetDefaultDatabaseFor<TDao>(out IDatabase db) where TDao : IDao
        {
            db = GetSysDatabaseFor(typeof(TDao));
            Db.For<TDao>(db);
        }

        /// <summary>
        /// Gets an application database by name using the default application name provider.
        /// </summary>
        /// <param name="databaseName">The database name.</param>
        /// <returns>A database instance of type T.</returns>
        public virtual T GetAppDatabase(string databaseName)
        {
            return GetAppDatabase(new DefaultConfigurationApplicationNameProvider(), databaseName);
        }

        /// <summary>
        /// Gets an application database for the specified name using the default application name provider.
        /// </summary>
        /// <param name="databaseName">The database name.</param>
        /// <returns>A database instance of type T.</returns>
        public virtual T GetAppDatabaseFor(string databaseName)
        {
            return GetAppDatabaseFor(new DefaultConfigurationApplicationNameProvider(), databaseName);
        }

        /// <summary>
        /// Gets an application database by name using the specified application name provider.
        /// </summary>
        /// <param name="appNameProvider">The application name provider.</param>
        /// <param name="databaseName">The database name.</param>
        /// <returns>A database instance of type T.</returns>
        public abstract T GetAppDatabase(IApplicationNameProvider appNameProvider, string databaseName);
        /// <summary>
        /// Gets a system database by name.
        /// </summary>
        /// <param name="databaseName">The database name.</param>
        /// <returns>A database instance of type T.</returns>
        public abstract T GetSysDatabase(string databaseName);
        /// <summary>
        /// Gets an application database for the specified instance using the application name provider.
        /// </summary>
        /// <param name="appNameProvider">The application name provider.</param>
        /// <param name="instance">The object instance to determine the database for.</param>
        /// <returns>A database instance of type T.</returns>
        public abstract T GetAppDatabaseFor(IApplicationNameProvider appNameProvider, object instance);
        /// <summary>
        /// Gets a system database for the specified instance.
        /// </summary>
        /// <param name="instance">The object instance to determine the database for.</param>
        /// <returns>A database instance of type T.</returns>
        public abstract T GetSysDatabaseFor(object instance);
        /// <summary>
        /// Gets an application database for the specified type using the application name provider.
        /// </summary>
        /// <param name="appNameProvider">The application name provider.</param>
        /// <param name="objectType">The type to determine the database for.</param>
        /// <param name="info">Optional additional info for database resolution.</param>
        /// <returns>A database instance of type T.</returns>
        public abstract T GetAppDatabaseFor(IApplicationNameProvider appNameProvider, Type objectType, string info = null!);
        /// <summary>
        /// Gets a system database for the specified type.
        /// </summary>
        /// <param name="objectType">The type to determine the database for.</param>
        /// <param name="info">Optional additional info for database resolution.</param>
        /// <returns>A database instance of type T.</returns>
        public abstract T GetSysDatabaseFor(Type objectType, string info = null!);
        /// <summary>
        /// Gets the file path for an application database for the specified type.
        /// </summary>
        /// <param name="appNameProvider">The application name provider.</param>
        /// <param name="type">The type to determine the database path for.</param>
        /// <param name="info">Optional additional info for path resolution.</param>
        /// <returns>The file path to the application database.</returns>
        public abstract string GetAppDatabasePathFor(IApplicationNameProvider appNameProvider, Type type, string info = null!);
        /// <summary>
        /// Gets the file path for a system database for the specified type.
        /// </summary>
        /// <param name="type">The type to determine the database path for.</param>
        /// <param name="info">Optional additional info for path resolution.</param>
        /// <returns>The file path to the system database.</returns>
        public abstract string GetSysDatabasePathFor(Type type, string info = null!);
        private void TryEnsureSchemas(IDatabase db, params Type[] daoTypes)
        {
            daoTypes.Each(new { Database = db, Logger = Logger }, (daoContext, dao) =>
            {
                daoContext.Database.TryEnsureSchema(dao, daoContext.Logger);
            });
        }

        IDatabase IDatabaseProvider.GetAppDatabase(IApplicationNameProvider appNameProvider, string databaseName)
        {
            return GetAppDatabase(appNameProvider, databaseName);
        }

        IDatabase IDatabaseProvider.GetSysDatabase(string databaseName)
        {
            return GetSysDatabase(databaseName);
        }

        IDatabase IDatabaseProvider.GetAppDatabaseFor(IApplicationNameProvider appNameProvider, object instance)
        {
            return GetAppDatabaseFor(appNameProvider, instance);
        }

        IDatabase IDatabaseProvider.GetSysDatabaseFor(object instance)
        {
            return GetSysDatabaseFor(instance);
        }

        IDatabase IDatabaseProvider.GetAppDatabaseFor(IApplicationNameProvider appNameProvider, Type objectType, string? info)
        {
            return GetAppDatabaseFor(appNameProvider, objectType, info!);
        }

        IDatabase IDatabaseProvider.GetSysDatabaseFor(Type objectType, string? info)
        {
            return GetSysDatabaseFor(objectType, info!);
        }
    }
}
