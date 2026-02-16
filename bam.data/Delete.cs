namespace Bam.Data
{
    /// <summary>
    /// Provides static convenience methods for building DELETE SQL statements.
    /// </summary>
    public static class Delete
    {
        /// <summary>
        /// Creates a DELETE statement for the table associated with Dao type T.
        /// </summary>
        /// <typeparam name="T">The Dao type whose table to delete from.</typeparam>
        /// <param name="db">The optional database to use; defaults to the database for type T.</param>
        /// <returns>An ISqlStringBuilder with the DELETE statement.</returns>
        public static ISqlStringBuilder From<T>(IDatabase db = null) where T : Dao, new()
        {
            return GetSqlStringBuilder<T>(db).Delete(Dao.TableName(typeof(T)));
        }

        /// <summary>
        /// Creates a DELETE statement with a WHERE clause for the table associated with Dao type T.
        /// </summary>
        /// <typeparam name="T">The Dao type whose table to delete from.</typeparam>
        /// <param name="filter">The filter defining which rows to delete.</param>
        /// <param name="db">The optional database to use; defaults to the database for type T.</param>
        /// <returns>An ISqlStringBuilder with the DELETE statement and WHERE clause.</returns>
        public static ISqlStringBuilder From<T>(IQueryFilter filter, IDatabase db = null) where T: Dao, new()
        {
            ISqlStringBuilder sql = GetSqlStringBuilder<T>(db);
            return sql.Delete(Dao.TableName(typeof(T))).Where(filter);
        }

        private static ISqlStringBuilder GetSqlStringBuilder<T>(IDatabase db) where T : Dao, new()
        {
            db = db ?? Db.For<T>();
            SqlStringBuilder sql = db.GetService<SqlStringBuilder>();
            return sql;
        }
    }
}
