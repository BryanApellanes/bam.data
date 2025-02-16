/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public class DoesntContainComparison : Comparison
    {
        public DoesntContainComparison(string columnName, object value, int? num = null)
            : base(columnName, "NOT LIKE", "%{0}%".Format(value), num)
        { }
    }
}
