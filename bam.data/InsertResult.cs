/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data;

namespace Bam.Data
{
    /// <summary>
    /// Represents the result of an INSERT query, holding the inserted instance and assigning its generated ID.
    /// </summary>
    public class InsertResult: QueryResult
    {
        /// <summary>
        /// Initializes a new InsertResult with the default "ID" column name.
        /// </summary>
        /// <param name="instance">The inserted Dao instance.</param>
        public InsertResult(object instance)
            : this(instance, "ID")
        {
        }

        /// <summary>
        /// Initializes a new InsertResult with the specified ID column alias.
        /// </summary>
        /// <param name="instance">The inserted Dao instance.</param>
        /// <param name="idAs">The column alias for the generated ID.</param>
        public InsertResult(object instance, string idAs)
        {
            this.Value = instance;
            this.ColumnName = idAs;
        }
        
        /// <summary>
        /// Gets or sets the inserted Dao instance.
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// Gets the column name alias for the generated ID.
        /// </summary>
        public string ColumnName { get; private set; }

        public override void SetDataTable(DataTable table)
        {
            DataTable = table;            
            ((Dao)Value).DbId = Convert.ToUInt64(DataTable.Rows[0][0]);
        }
    }
}
