/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Holds the default registration action used to register Dao types by connection name.
    /// </summary>
    public class Registrar
    {
        static Action<string> _current;
        static object _currentLock = new object();
        /// <summary>
        /// Gets or sets the default registration action. Throws if not set when accessed.
        /// </summary>
        public static Action<string> Default
        {
            get
            {
                if (_current == null)
                {
                    throw new InvalidOperationException("Default Registrar not specified");
                }

                return _current;
            }
            set
            {
                _current = value;
            }
        }     
    }
}
