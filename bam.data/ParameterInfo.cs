namespace Bam.Data
{
    /// <summary>
    /// Holds parameter metadata used when building SQL parameterized queries.
    /// </summary>
    public class ParameterInfo : IParameterInfo
    {
        /// <summary>
        /// Gets or sets the column name associated with this parameter.
        /// </summary>
        public string ColumnName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the function used to format column names.
        /// </summary>
        public Func<string, string> ColumnNameFormatter
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the parameter number used for parameter naming.
        /// </summary>
        public int? Number
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the SQL comparison operator (e.g., "=", "&lt;", "&gt;").
        /// </summary>
        public string Operator
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the parameter prefix (e.g., "@").
        /// </summary>
        public string ParameterPrefix
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the parameter value.
        /// </summary>
        public object Value
        {
            get; set;
        }

        /// <summary>
        /// Sets this parameter's number and returns the next available number.
        /// </summary>
        /// <param name="value">The number to assign.</param>
        /// <returns>The incremented number for the next parameter.</returns>
        public int? SetNumber(int? value)
        {
            Number = value;
            return ++value;
        }
    }
}
