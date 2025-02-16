/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public class MultipleEntriesFoundException: Exception
    {
        public MultipleEntriesFoundException()
            : base("Mutliple entries found")
        { }
    }
}
