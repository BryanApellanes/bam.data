/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// A SQL LIKE comparison that matches values starting with the specified prefix (e.g., LIKE 'value%').
    /// </summary>
    public class StartsWithComparison: Comparison
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartsWithComparison"/> class.
        /// </summary>
        /// <param name="columnName">The column name to filter on.</param>
        /// <param name="value">The prefix to match against.</param>
        /// <param name="number">An optional parameter number for parameterized queries.</param>
        public StartsWithComparison(string columnName, object value, int? number = null)
            : base(columnName, "LIKE", "{0}%".Format(value), number)
        { }
    }
}
