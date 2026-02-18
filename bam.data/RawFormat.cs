namespace Bam.Data
{
    /// <summary>
    /// A format part that outputs raw SQL text with associated parameters, bypassing structured formatting.
    /// </summary>
    public class RawFormat : FormatPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RawFormat"/> class with raw SQL and a dictionary of parameters.
        /// </summary>
        /// <param name="sql">The raw SQL string.</param>
        /// <param name="parameters">A dictionary mapping parameter names to their values.</param>
        public RawFormat(string sql, Dictionary<string, object> parameters)
        {
            Sql = sql;
            parameters.Each(kvp =>
            {
                AddParameter(new ParameterInfo { ColumnName = kvp.Key, Value = kvp.Value });
            });
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RawFormat"/> class with raw SQL and parameter info objects.
        /// </summary>
        /// <param name="sql">The raw SQL string.</param>
        /// <param name="parameters">The parameter information objects for the query.</param>
        public RawFormat(string sql, params ParameterInfo[] parameters)
        {
            Sql = sql;
            parameters.Each(AddParameter);
        }
        /// <summary>
        /// Gets or sets the raw SQL string.
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// Returns the raw SQL string.
        /// </summary>
        /// <returns>The raw SQL string.</returns>
        public override string Parse()
        {
            return Sql;
        }
    }
}
