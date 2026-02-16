/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Delegate that builds a SQL commit statement for the specified table with the given column assignments.
    /// </summary>
    /// <param name="tableName">The name of the database table to commit to.</param>
    /// <param name="values">The column-value assignments to include in the statement.</param>
    /// <returns>A <see cref="SqlStringBuilder"/> containing the generated SQL statement.</returns>
    public delegate SqlStringBuilder CommitDelegate(string tableName, params AssignValue[] values);
}
