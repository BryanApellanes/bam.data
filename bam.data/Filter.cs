namespace Bam.Data
{
    /// <summary>
    /// Convenience entry point for contextually readable syntax; the same as Query
    /// </summary>
    public static class Filter
    {
        /// <summary>
        /// Creates a new QueryFilter for the specified column name.
        /// </summary>
        /// <param name="columnName">The column name to filter on.</param>
        /// <returns>A new QueryFilter instance.</returns>
        public static QueryFilter Column(string columnName)
        {
            return new QueryFilter(columnName);
        }
        /// <summary>
        /// Creates a new QueryFilter for a WHERE clause on the specified column.
        /// </summary>
        /// <param name="columnName">The column name to filter on.</param>
        /// <returns>A new QueryFilter instance.</returns>
        public static QueryFilter Where(string columnName)
        {
            return new QueryFilter(columnName);
        }

        /// <summary>
        /// Creates a new QueryValue wrapping the specified value.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <returns>A new QueryValue instance.</returns>
        public static QueryValue Value(object value)
        {
            return new QueryValue(value);
        }
    }
}
