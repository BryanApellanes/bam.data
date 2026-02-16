/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Provides static convenience methods for building typed SELECT queries.
    /// </summary>
    /// <typeparam name="C">The query filter/column type.</typeparam>
    public static class Select<C> where C : IQueryFilter, IFilterToken, new()
    {
        /// <summary>
        /// Creates a new query to select all records of Dao type T.
        /// </summary>
        /// <typeparam name="T">The Dao type to select.</typeparam>
        /// <returns>A new Query instance.</returns>
        public static Query<C, T> From<T>() where T: IDao, new()
        {
            return new Query<C, T>();
        }

        /// <summary>
        /// Creates a new query to select records of Dao type T matching the WHERE delegate.
        /// </summary>
        /// <typeparam name="T">The Dao type to select.</typeparam>
        /// <param name="where">The WHERE delegate defining the filter.</param>
        /// <returns>A new Query instance with the specified filter.</returns>
        public static Query<C, T> From<T>(WhereDelegate<C> where) where T : IDao, new()
        {
            return new Query<C, T>(where);
        }
        
        /// <summary>
        /// Creates a new query to select records of Dao type T matching the filter function.
        /// </summary>
        /// <typeparam name="T">The Dao type to select.</typeparam>
        /// <param name="where">The function defining the filter.</param>
        /// <returns>A new Query instance with the specified filter.</returns>
        public static Query<C, T> From<T>(Func<C, QueryFilter<C>> where) where T : IDao, new()
        {
            return new Query<C, T>(where);
        }
    }
}
