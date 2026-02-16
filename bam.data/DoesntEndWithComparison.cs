/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// A SQL NOT LIKE comparison that excludes values ending with the specified suffix (e.g., NOT LIKE '%value').
    /// </summary>
    public class DoesntEndWithComparison : Comparison
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoesntEndWithComparison"/> class.
        /// </summary>
        /// <param name="columnName">The column name to filter on.</param>
        /// <param name="value">The suffix to exclude.</param>
        /// <param name="num">An optional parameter number for parameterized queries.</param>
        public DoesntEndWithComparison(string columnName, object value, int? num = null)
            : base(columnName, "NOT LIKE", "%{0}".Format(value), num)
        { }
    }
}
