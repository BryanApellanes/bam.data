/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// A SQL comparison against NULL (e.g., column IS NULL or column IS NOT NULL).
    /// </summary>
    public class NullComparison: Comparison
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullComparison"/> class.
        /// </summary>
        /// <param name="columnName">The column name to compare.</param>
        /// <param name="oper">The SQL operator, typically "IS" or "IS NOT".</param>
        public NullComparison(string columnName, string oper)
            : base(columnName, oper, null!)
        { }

        public override string ToString()
        {
            return $"{ColumnNameFormatter(ColumnName)} {this.Operator} NULL";
        }
    }
}
