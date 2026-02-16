/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Holds the results of executing a QuerySet, providing indexed access to individual result sets.
    /// </summary>
    public class QuerySetResults : IQuerySetResults
    {
        List<IHasDataTable> _values;
        public QuerySetResults(IEnumerable<IHasDataTable> values, IDatabase database)
        {
            this._values = new List<IHasDataTable>(values);
            this.Database = database;
        }

        /// <summary>
        /// Gets or sets the database that produced these results.
        /// </summary>
        public IDatabase Database { get; set; }
        /// <summary>
        /// Instantiates a new instance of T and calls SetDataTable passing
        /// in the DataTable from the specified index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T As<T>(int index) where T: IHasDataTable, new()
        {
            _values[index].Database = this.Database;
            return _values[index].As<T>();
        }

        /// <summary>
        /// Gets the result at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index.</param>
        /// <returns>The IHasDataTable result at the index.</returns>
        public IHasDataTable this[int index]
        {
            get
            {
                return _values[index];
            }
        }

        /// <summary>
        /// Returns the value of the specified index as the specified 
        /// generic Dao type, only valid for inserts.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns></returns>
        public T ToDao<T>(int index) where T : IDao, new()
        {
            InsertResult ir = _values[index] as InsertResult;
            if (ir == null)
            {
                throw new InvalidOperationException("The specified index was not an InsertResult");
            }

            T result = (T)ir.Value;           

            return result;
        }

        /// <summary>
        /// Gets the count value from the result at the specified index; only valid for count results.
        /// </summary>
        /// <param name="index">The zero-based index of the count result.</param>
        /// <returns>The count value.</returns>
        public long ToCountResult(int index)
        {
            CountResult cr = _values[index] as CountResult;
            if (cr == null)
            {
                throw new InvalidOperationException("The specified index was not CountResult");
            }

            return cr.Value;
        }
                
        /// <summary>
        /// Gets the number of result sets.
        /// </summary>
        public int Count
        {
            get
            {
                return _values.Count;
            }
        }
    }
}
