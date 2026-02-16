/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Configuration;
using System.Data.Common;

namespace Bam.Data
{
    /// <summary>
    /// Resolves connection strings from the default configuration file or a custom resolver function.
    /// </summary>
    public class DefaultConnectionStringResolver: IConnectionStringResolver
    {
        static DefaultConnectionStringResolver _instance;
        static object _instanceLock = new object();
        /// <summary>
        /// Gets the singleton instance of DefaultConnectionStringResolver.
        /// </summary>
        public static DefaultConnectionStringResolver Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DefaultConnectionStringResolver();
                        }
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// If specified, is used as the default connection string resolver
        /// </summary>
        public Func<string, System.Configuration.ConnectionStringSettings> Resolver
        {
            get;
            set;
        }

        #region IConnectionStringResolver Members

        /// <summary>
        /// Resolves the connection string settings for the specified connection name.
        /// </summary>
        /// <param name="connectionName">The connection name to resolve.</param>
        /// <returns>The resolved ConnectionStringSettings.</returns>
        public System.Configuration.ConnectionStringSettings Resolve(string connectionName)
        {
            if (Resolver != null)
            {
                return Resolver(connectionName);
            }
            else
            {
                return ConfigurationManager.ConnectionStrings[connectionName];
            }
        }


        /// <summary>
        /// Gets a DbConnectionStringBuilder initialized with the default connection string.
        /// </summary>
        /// <returns>A DbConnectionStringBuilder instance.</returns>
        public DbConnectionStringBuilder GetConnectionStringBuilder()
        {
            return new DbConnectionStringBuilder { ConnectionString = Resolve("Default")?.ConnectionString };
        }

        #endregion
    }
}
