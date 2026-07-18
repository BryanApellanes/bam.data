namespace Bam.Data
{
    /// <summary>
    /// Names where a SQL dialect renders its row cap: interpolated into the SELECT clause
    /// (T-SQL TOP, Firebird FIRST) or appended at the end of the statement after any
    /// WHERE and ORDER BY clauses (LIMIT, FETCH FIRST n ROWS ONLY).
    /// </summary>
    public enum RowCapPlacement
    {
        SelectClause,
        StatementEnd
    }
}
