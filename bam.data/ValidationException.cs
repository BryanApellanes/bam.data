/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Exception thrown when a Dao validation check fails.
    /// </summary>
    public class ValidationException: Exception
    {
        public ValidationException(string msg) : base(msg) { }
        public ValidationException(Exception inner) : base("An exception occurred", inner) { }
        public ValidationException(string msg, Exception inner) : base(msg, inner) { }
    }
}
