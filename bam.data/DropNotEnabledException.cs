/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Exception thrown when a drop operation is attempted but EnableDrop is not set to true on the SchemaWriter.
    /// </summary>
    public class DropNotEnabledException: Exception
    {
        public DropNotEnabledException()
            : base("Drop is not enabled on the SchemaWriter.  Set EnableDrop = true if you really want to")
        { }
    }
}
