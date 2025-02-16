/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public class ConnectionInfo
    {
        public ConnectionInfo()
        {
            this._settings = new Dictionary<string, string>();
        }

        public string ConnectionString
        {
            get;
            set;
        }

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
