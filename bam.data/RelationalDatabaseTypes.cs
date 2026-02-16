using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bam.Data
{
    /// <summary>
    /// Enumerates supported relational database types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RelationalDatabaseTypes
    {
        /// <summary>
        /// SQLite database.
        /// </summary>
        SQLite,
        /// <summary>
        /// Microsoft SQL Server database.
        /// </summary>
        MsSql,
        /// <summary>
        /// MySQL database.
        /// </summary>
        MySql,
        /// <summary>
        /// PostgreSQL database.
        /// </summary>
        Postgres
    }
}