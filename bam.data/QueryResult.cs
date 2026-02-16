/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data;

namespace Bam.Data
{
    /// <summary>
    /// Abstract base class for query results that hold a DataTable and provide typed access.
    /// </summary>
    public abstract class QueryResult: IHasDataTable
    {
        #region IHasDataTable Members
        /// <summary>
        /// Gets or sets the database that produced this result.
        /// </summary>
        public IDatabase Database { get; set; }

        /// <summary>
        /// Gets the DataTable containing the result data.
        /// </summary>
        public DataTable DataTable
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the first DataRow from the result DataTable.
        /// </summary>
        public DataRow DataRow
        {
            get
            {
                return DataTable.Rows[0];
            }
            set { }
        }

        /// <summary>
        /// Sets the DataTable for this result.
        /// </summary>
        /// <param name="table">The DataTable to set.</param>
        public abstract void SetDataTable(DataTable table);

        /// <summary>
        /// Instantiates a new instance of T and calls SetDataTable passing
        /// in the DataTable from the current instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T As<T>() where T : IHasDataTable, new()
        {
            T val = new T()
            {
                Database = this.Database
            };
            val.SetDataTable(this.DataTable);
            return val;
        }

        #endregion
    }
}
