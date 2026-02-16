/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// A SQL NOT LIKE comparison that excludes values containing the specified substring (e.g., NOT LIKE '%value%').
    /// </summary>
    public class DoesntContainComparison : Comparison
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoesntContainComparison"/> class.
        /// </summary>
        /// <param name="columnName">The column name to filter on.</param>
        /// <param name="value">The substring to exclude.</param>
        /// <param name="num">An optional parameter number for parameterized queries.</param>
        public DoesntContainComparison(string columnName, object value, int? num = null)
            : base(columnName, "NOT LIKE", "%{0}%".Format(value), num)
        { }
    }
}
