/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
	/// <summary>
	/// Enumerates supported SQL dialect types.
	/// </summary>
	public enum SqlDialect
	{
		/// <summary>
		/// Invalid or unspecified SQL dialect.
		/// </summary>
		Invalid,
		/// <summary>
		/// SQLite database dialect.
		/// </summary>
		SQLite,
        /// <summary>
        /// Microsoft sql; same as MsSql
        /// </summary>
        Ms,
        /// <summary>
        /// Microsoft sql; same as Ms
        /// </summary>
		MsSql,
        /// <summary>
        /// My sql; same as MySql
        /// </summary>
        My,
        /// <summary>
        /// My sql; same as My
        /// </summary>
        MySql,
		/// <summary>
		/// Oracle database dialect.
		/// </summary>
		Oracle,
        /// <summary>
        /// Postgres sql; same as Npgsql
        /// </summary>
        Postgres,
        /// <summary>
        /// Postgres sql; same as Postgres
        /// </summary>
        Npgsql,

        /// <summary>
        /// InterSystems database dialect.
        /// </summary>
        InterSystems
	}
}
