/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Holds database connection information including the connection string, provider name, and additional settings.
    /// </summary>
    public class ConnectionInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionInfo"/> class.
        /// </summary>
        public ConnectionInfo()
        {
            this._settings = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets the database connection string.
        /// </summary>
        public string ConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the database provider name (e.g., "System.Data.SqlClient").
        /// </summary>
        public string ProviderName
        {
            get;
            set;
        }

        Dictionary<string, string> _settings;
        public string this[string key]
        {
            get
            {
                return _settings[key];
            }
            set
            {
                _settings[key] = value;
            }
        }
    }
}
