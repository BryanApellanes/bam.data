/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Exception thrown when multiple entries are found where only one was expected.
    /// </summary>
    public class MultipleEntriesFoundException: Exception
    {
        public MultipleEntriesFoundException()
            : base("Mutliple entries found")
        { }
    }
}
