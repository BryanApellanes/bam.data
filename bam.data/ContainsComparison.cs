/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// A SQL LIKE comparison that matches values containing the specified substring (e.g., LIKE '%value%').
    /// </summary>
    public class ContainsComparison: Comparison
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainsComparison"/> class.
        /// </summary>
        /// <param name="columnName">The column name to filter on.</param>
        /// <param name="value">The substring to search for.</param>
        /// <param name="number">An optional parameter number for parameterized queries.</param>
        public ContainsComparison(string columnName, object value, int? number = null)
            : base(columnName, "LIKE", "%{0}%".Format(value), number)
        { }
    }
}
