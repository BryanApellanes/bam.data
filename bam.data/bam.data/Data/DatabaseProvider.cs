using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Net.Configuration;
using Bam.Net.Data;
using Bam.Net.Logging;

namespace Bam.Net.Data
{
    /// <summary>
    /// Sets database properties for a given set of object instances.
    /// </summary>
    /// <typeparam name="T">The type of database to use</typeparam>
    public abstract class DatabaseProvider<T> : IDatabaseProvider where T : IDatabase, new()
    {
        public ILogger Logger
        {
            get; set;
        }

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

        public virtual void SetDefaultDatabaseFor<TDao>() where TDao : IDao
        {
            SetDefaultDatabaseFor<TDao>(out IDatabase db);
        }

        public virtual void SetDefaultDatabaseFor<TDao>(out IDatabase db) where TDao : IDao
        {
            db = GetSysDatabaseFor(typeof(TDao));
            Db.For<TDao>(db);
        }

        public virtual T GetAppDatabase(string databaseName)
        {
            return GetAppDatabase(new DefaultConfigurationApplicationNameProvider(), databaseName);
        }

        public virtual T GetAppDatabaseFor(string databaseName)
        {
            return GetAppDatabaseFor(new DefaultConfigurationApplicationNameProvider(), databaseName);
        }

        public abstract T GetAppDatabase(IApplicationNameProvider appNameProvider, string databaseName);
        public abstract T GetSysDatabase(string databaseName);
        public abstract T GetAppDatabaseFor(IApplicationNameProvider appNameProvider, object instance);
        public abstract T GetSysDatabaseFor(object instance);
        public abstract T GetAppDatabaseFor(IApplicationNameProvider appNameProvider, Type objectType, string info = null);
        public abstract T GetSysDatabaseFor(Type objectType, string info = null);
        public abstract string GetAppDatabasePathFor(IApplicationNameProvider appNameProvider, Type type, string info = null);
        public abstract string GetSysDatabasePathFor(Type type, string info = null);
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

        IDatabase IDatabaseProvider.GetAppDatabaseFor(IApplicationNameProvider appNameProvider, Type objectType, string info)
        {
            return GetAppDatabaseFor(appNameProvider, objectType, info);
        }

        IDatabase IDatabaseProvider.GetSysDatabaseFor(Type objectType, string info)
        {
            return GetSysDatabaseFor(objectType, info);
        }
    }
}
