/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public class ContainsComparison: Comparison
    {
        public ContainsComparison(string columnName, object value, int? number = null)
            : base(columnName, "LIKE", "%{0}%".Format(value), number)
        { }
    }
}
