/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Convenience class for building a SELECT statement for a single column from a Dao type's table.
    /// </summary>
    /// <typeparam name="T">The Dao type whose table to select from.</typeparam>
    public class SelectColumn<T>: SqlStringBuilder where T: Dao
    {
        public SelectColumn(string column)
        {
            this.Select(Dao.TableName(typeof(T)), column);            
        }
    }
}
