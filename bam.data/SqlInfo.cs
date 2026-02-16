using System.Data.Common;

namespace Bam.Data
{
    /// <summary>
    /// Encapsulates a SQL statement and its associated database parameters.
    /// </summary>
    public class SqlInfo
    {
        /// <summary>
        /// Initializes a new SqlInfo with the specified SQL statement and parameters.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement text.</param>
        /// <param name="dbParameters">The database parameters for the statement.</param>
        public SqlInfo(string sqlStatement, params DbParameter[] dbParameters)
        {
            Sql = sqlStatement;
            DbParameters = dbParameters;
        }
        /// <summary>
        /// Gets or sets the SQL statement text.
        /// </summary>
        public string Sql { get; set; }
        /// <summary>
        /// Gets or sets the database parameters for the statement.
        /// </summary>
        public DbParameter[] DbParameters { get; set; }

        /// <summary>
        /// Returns a string representation of this SQL info including parameters and SQL text.
        /// </summary>
        /// <returns>A formatted string containing parameter info, hash, and SQL text.</returns>
        public string ToInfoString()
        {
            return ToString();
        }

        public override string ToString()
        {
            return $"{DbParameters.ToInfoString()}\r\n{DbParameters.Sha1()}\r\n{Sql}";
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override bool Equals(object obj)
        {
            SqlInfo compareTo = obj as SqlInfo;
            if(compareTo != null)
            {
                return compareTo.ToString().Equals(ToString());
            }
            return base.Equals(obj);
        }
    }
}
