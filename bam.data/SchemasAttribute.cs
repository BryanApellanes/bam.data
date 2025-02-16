namespace Bam.Data
{
    /// <summary>
    /// Attribute used to mark a database property
    /// with Dao types used to initialize the
    /// schema in the database
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SchemasAttribute: Attribute
    {
        public SchemasAttribute(params Type[] daoSchemaTypes)
        {
            DaoSchemaTypes = daoSchemaTypes;
        }
        /// <summary>
        /// Dao types to use to initialize schemas
        /// </summary>
        public Type[] DaoSchemaTypes { get; set; }
    }
}
