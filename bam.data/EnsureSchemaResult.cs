namespace Bam.Data
{
    /// <summary>
    /// Represents the result of an EnsureSchema operation, including the database, schema name, and status.
    /// </summary>
    public class EnsureSchemaResult
    {
        /// <summary>
        /// Gets or sets the database that the schema was ensured on.
        /// </summary>
        public IDatabase Database { get; set; } = null!;

        /// <summary>
        /// Gets or sets the name of the schema that was ensured.
        /// </summary>
        public string SchemaName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the status of the EnsureSchema operation.
        /// </summary>
        public EnsureSchemaStatus Status { get; set; }

        public override bool Equals(object? obj)
        {
            EnsureSchemaResult? compareTo = obj as EnsureSchemaResult;
            if(compareTo != null)
            {
                return Database.Equals(compareTo.Database) && SchemaName.Equals(compareTo.SchemaName);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.GetHashCode(Database, SchemaName);
        }
    }
}
