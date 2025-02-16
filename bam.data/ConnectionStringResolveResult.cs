/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Configuration;

namespace Bam.Data
{
    public class ConnectionStringResolveResult
    {
        public ConnectionStringResolveResult(ConnectionStringSettings settings, Exception ex = null)
        {
            this.Settings = settings;
            this.Exception = ex;
            this.Success = ex == null;
        }

        public ConnectionStringSettings Settings { get; private set; }
        public Exception Exception { get; private set; }
        public bool Success { get; private set; }
    }
}
