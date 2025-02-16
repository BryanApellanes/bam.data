/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public class DoesntStartWithComparison : Comparison
    {
        public DoesntStartWithComparison(string columnName, object value, int? number = null)
            : base(columnName, "NOT LIKE", "{0}%".Format(value), number)
        { }
    }
}