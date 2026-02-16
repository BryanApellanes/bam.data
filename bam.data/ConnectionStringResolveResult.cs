/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Configuration;

namespace Bam.Data
{
    /// <summary>
    /// Encapsulates the result of attempting to resolve a connection string, including success status and any exception.
    /// </summary>
    public class ConnectionStringResolveResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionStringResolveResult"/> class.
        /// </summary>
        /// <param name="settings">The resolved connection string settings, or null if resolution failed.</param>
        /// <param name="ex">The exception that occurred during resolution, or null if successful.</param>
        public ConnectionStringResolveResult(ConnectionStringSettings settings, Exception ex = null)
        {
            this.Settings = settings;
            this.Exception = ex;
            this.Success = ex == null;
        }

        /// <summary>
        /// Gets the resolved connection string settings.
        /// </summary>
        public ConnectionStringSettings Settings { get; private set; }

        /// <summary>
        /// Gets the exception that occurred during resolution, if any.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the connection string was resolved successfully.
        /// </summary>
        public bool Success { get; private set; }
    }
}
