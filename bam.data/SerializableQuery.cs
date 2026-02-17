namespace Bam.Data
{
    /// <summary>
    /// Wraps a SQL query and its parameters for serialization and later execution.
    /// </summary>
    public class SerializableQuery
    {
        /// <summary>
        /// Initializes a new SerializableQuery from the specified SQL builder and database.
        /// </summary>
        /// <param name="sql">The SQL string builder.</param>
        /// <param name="db">The database used for parameter resolution.</param>
        public SerializableQuery(SqlStringBuilder sql, Database db)
        {
            SqlStringBuilder = sql;
            Database = db;
            Sql = sql;            
        }

        protected SqlStringBuilder SqlStringBuilder { get; set; }
        protected Database Database { get; set; }

        /// <summary>
        /// Gets or sets the SQL statement text.
        /// </summary>
        public string Sql { get; set; }
        Dictionary<string, object> _parameters = null!;
        /// <summary>
        /// Gets or sets the serializable parameter dictionary.
        /// </summary>
        public Dictionary<string, object> Parameters
        {
            get
            {
                if(_parameters == null)
                {
                    Args.ThrowIfNull(Database, "Database");
                    _parameters = new Dictionary<string, object>();
                    Database.GetParameters(SqlStringBuilder).Each(new { Parameters }, (ctx, p) =>
                    {
                        ctx.Parameters.Add(p.ParameterName, p!.Value!.ToString()!);
                    });
                }
                return _parameters;
            }
            set => _parameters = value;
        }

        /// <summary>
        /// Executes this query against the specified database and returns typed results.
        /// </summary>
        /// <typeparam name="T">The type to map each row to.</typeparam>
        /// <param name="db">The database to execute against.</param>
        /// <returns>An enumerable of T instances.</returns>
        public IEnumerable<T> Execute<T>(Database db = null!) where T : class, new()
        {
            Database = db;
            return db.ExecuteReader<T>(Sql, Parameters.ToDbParameters(Database));
        }
    }
}
