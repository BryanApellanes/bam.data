/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// A SQL LIKE comparison that matches values ending with the specified suffix (e.g., LIKE '%value').
    /// </summary>
    public class EndsWithComparison: Comparison
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EndsWithComparison"/> class.
        /// </summary>
        /// <param name="columnName">The column name to filter on.</param>
        /// <param name="value">The suffix to match against.</param>
        /// <param name="number">An optional parameter number for parameterized queries.</param>
        public EndsWithComparison(string columnName, object value, int? number = null)
            : base(columnName, "LIKE", "%{0}".Format(value), number)
        { }
    }
}
