/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Represents a SQL comparison expression (e.g., column = value) used as a filter token in query building.
    /// </summary>
    public class Comparison : FilterToken, IParameterInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Comparison"/> class.
        /// </summary>
        /// <param name="column">The column name for the comparison.</param>
        /// <param name="oper">The SQL operator (e.g., "=", "&lt;", "LIKE").</param>
        /// <param name="value">The value to compare against.</param>
        /// <param name="number">An optional parameter number for parameterized queries.</param>
        public Comparison(string column, string oper, object value, int? number = null)
            : base(oper)
        {
            this.ColumnName = column;
            this.Operator = oper;
			this.ColumnNameFormatter = (c) => $"[{c}]";
			this.ParameterPrefix = "@";
            if (value == null)
            {
                value = DBNull.Value;
            }

            this.Value = value;
            if (number != null)
            {
                this.Number = number.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Gets or sets the function that formats column names in the SQL output (e.g., wrapping with brackets).
        /// </summary>
		public Func<string, string> ColumnNameFormatter { get; set; }

        /// <summary>
        /// Gets or sets the parameter prefix used in parameterized queries (e.g., "@" for SQL Server).
        /// </summary>
		public string ParameterPrefix { get; set; }

        /// <summary>
        /// Gets or sets the column name for this comparison.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the value to compare against.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the parameter number used to generate unique parameter names.
        /// </summary>
        public int? Number { get; set; }

        /// <summary>
        /// Sets the parameter number and returns the next available number.
        /// </summary>
        /// <param name="value">The parameter number to assign.</param>
        /// <returns>The next sequential parameter number.</returns>
        public virtual int? SetNumber(int? value)
        {
            Number = value;
            return ++value;
        }

        public override string ToString()
        {
             return string.Format("{0} {1} {2}", ColumnNameFormatter(ColumnName), this.Operator, string.Format("{0}{1}{2}", ParameterPrefix, ColumnName, Number));
        }
    }
}
