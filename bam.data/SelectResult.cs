/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data;

namespace Bam.Data
{
    /// <summary>
    /// Represents the result of a SELECT query, holding the resulting DataTable.
    /// </summary>
    public class SelectResult : QueryResult
    {
        public SelectResult()
        {

        }

        public object Value { get; set; } = null!;

        #region IHasDataTable Members
        
        public override void SetDataTable(DataTable table)
        {
            DataTable = table;
        }

        #endregion
    }
}
