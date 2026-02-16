/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// A SQL NOT LIKE comparison that excludes values starting with the specified prefix (e.g., NOT LIKE 'value%').
    /// </summary>
    public class DoesntStartWithComparison : Comparison
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoesntStartWithComparison"/> class.
        /// </summary>
        /// <param name="columnName">The column name to filter on.</param>
        /// <param name="value">The prefix to exclude.</param>
        /// <param name="number">An optional parameter number for parameterized queries.</param>
        public DoesntStartWithComparison(string columnName, object value, int? number = null)
            : base(columnName, "NOT LIKE", "{0}%".Format(value), number)
        { }
    }
}