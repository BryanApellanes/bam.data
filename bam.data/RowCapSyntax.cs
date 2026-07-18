namespace Bam.Data
{
    /// <summary>
    /// Describes how a SQL dialect renders a row cap: where the cap text belongs
    /// (SELECT clause or statement end) and the format used to render it.
    /// SqlStringBuilder consults its RowCap property (an instance of this type)
    /// when SelectTop is called; providers override that property to select
    /// their dialect.
    /// </summary>
    public sealed class RowCapSyntax
    {
        /// <summary>
        /// T-SQL row cap: TOP {n} interpolated into the SELECT clause.  Valid for
        /// Microsoft SQL Server; the base SqlStringBuilder default.
        /// </summary>
        public static readonly RowCapSyntax TSqlTop = new RowCapSyntax(RowCapPlacement.SelectClause, "TOP {0} ");

        /// <summary>
        /// LIMIT {n} appended at the end of the statement.  Valid for PostgreSQL,
        /// MySQL and SQLite.
        /// </summary>
        public static readonly RowCapSyntax Limit = new RowCapSyntax(RowCapPlacement.StatementEnd, " LIMIT {0}");

        /// <summary>
        /// FIRST {n} interpolated into the SELECT clause.  Valid for Firebird.
        /// </summary>
        public static readonly RowCapSyntax FirebirdFirst = new RowCapSyntax(RowCapPlacement.SelectClause, "FIRST {0} ");

        /// <summary>
        /// FETCH FIRST {n} ROWS ONLY appended at the end of the statement.  Valid for
        /// Oracle 12c and later.
        /// </summary>
        public static readonly RowCapSyntax OracleFetchFirst = new RowCapSyntax(RowCapPlacement.StatementEnd, " FETCH FIRST {0} ROWS ONLY");

        public RowCapSyntax(RowCapPlacement placement, string format)
        {
            this.Placement = placement;
            this.Format = format;
        }

        /// <summary>
        /// Gets where this dialect's row cap is rendered.
        /// </summary>
        public RowCapPlacement Placement
        {
            get;
        }

        /// <summary>
        /// Gets the format string used to render the row cap; {0} is the row count.
        /// </summary>
        public string Format
        {
            get;
        }

        /// <summary>
        /// Renders the row cap clause for the specified row count.
        /// </summary>
        /// <param name="count">The maximum number of rows to return.</param>
        public string Render(int count)
        {
            return string.Format(Format, count);
        }
    }
}
