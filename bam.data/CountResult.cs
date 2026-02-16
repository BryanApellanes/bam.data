/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data;

namespace Bam.Data
{
    /// <summary>
    /// A query result that extracts a count value from the first row and column of the result set.
    /// </summary>
    public class CountResult: QueryResult
    {
        /// <summary>
        /// Gets the count value extracted from the query result, or -1 if no result was returned.
        /// </summary>
        public long Value
        {
            get;
            private set;
        }
        /// <summary>
        /// Sets the data table and extracts the count value from the first row and column.
        /// </summary>
        /// <param name="table">The data table containing the count result.</param>
        public override void SetDataTable(DataTable table)
        {
            Value = -1;
            this.DataTable = table;
            if (table.Rows.Count > 0 && table.Columns.Count > 0)
            {
                Value = Convert.ToInt64(table.Rows[0][0]);
            }
        }
    }
}
