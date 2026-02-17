/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Configuration;

namespace Bam.Data
{
    /// <summary>
    /// Manages a chain of connection string resolvers, trying each in sequence until one succeeds.
    /// </summary>
    public abstract class ConnectionStringResolvers
    {
        static List<IConnectionStringResolver> _resolvers;
        static ConnectionStringResolvers()
        {
            _resolvers = new List<IConnectionStringResolver>();
            _resolvers.Add(DefaultConnectionStringResolver.Instance);
        }

        /// <summary>
        /// Adds a connection string resolver to the chain if it is not already registered.
        /// </summary>
        /// <param name="resolver">The resolver to add.</param>
        public static void AddResolver(IConnectionStringResolver resolver)
        {
            if (!_resolvers.Contains(resolver))
            {
                _resolvers.Add(resolver);
            }
        }

        /// <summary>
        /// Removes the specified resolver from the chain.
        /// </summary>
        /// <param name="resolver">The resolver to remove.</param>
        public static void Remove(IConnectionStringResolver resolver)
        {
            _resolvers.Remove(resolver);
        }

        /// <summary>
        /// Removes all resolvers from the chain.
        /// </summary>
        public static void Clear()
        {
            _resolvers.Clear();
        }

        /// <summary>
        /// Resolves a connection string by trying each registered resolver in order until one succeeds.
        /// </summary>
        /// <param name="connectionName">The connection name to resolve.</param>
        /// <returns>The resolved connection string settings, or null if no resolver succeeded.</returns>
        public static ConnectionStringSettings Resolve(string connectionName)
        {
            ConnectionStringSettings? settings = null;
            foreach (IConnectionStringResolver resolver in _resolvers)
            {
                settings = resolver.Resolve(connectionName);
                if (settings != null)
                {
                    break;
                }
            }
            return settings!;
        }

        /// <summary>
        /// Attempts to resolve a connection string, catching any exceptions that occur.
        /// </summary>
        /// <param name="connectionName">The connection name to resolve.</param>
        /// <returns>A <see cref="ConnectionStringResolveResult"/> indicating success or failure.</returns>
        public static ConnectionStringResolveResult TryResolve(string connectionName)
        {
            try
            {
                return new ConnectionStringResolveResult(Resolve(connectionName));
            }
            catch (Exception ex)
            {
                return new ConnectionStringResolveResult(null!, ex);
            }
        }
    }

}
