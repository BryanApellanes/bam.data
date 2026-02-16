namespace Bam.Data
{
    /// <summary>
    /// Represents the identifier value for a Dao, providing methods to retrieve database IDs and universal identifiers.
    /// </summary>
    public class DaoId : QueryValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaoId"/> class with the specified value and filter.
        /// </summary>
        /// <param name="value">The identifier value.</param>
        /// <param name="filter">The query filter associated with this identifier.</param>
        public DaoId(object value, QueryFilter filter) : base(value, filter)
        {
            IdentifierName = "Id";
        }
        
        /// <summary>
        /// The name of the property or column that represents the Dao's identifier, this is typically "Id".
        /// </summary>
        public string IdentifierName { get; set; }

        /// <summary>
        /// Gets the stored value of this identifier, returning the raw value.
        /// </summary>
        /// <returns>The raw identifier value.</returns>
        public override object GetStoredValue()
        {
            return GetRawValue();
        }

        /// <summary>
        /// Gets the current value of this identifier, returning the raw value.
        /// </summary>
        /// <returns>The raw identifier value.</returns>
        public override object GetValue()
        {
            return GetRawValue();
        }

        /// <summary>
        /// Gets the database ID from the specified Dao instance.
        /// </summary>
        /// <param name="dao">The Dao instance to get the ID from.</param>
        /// <returns>The database ID of the Dao, or null if the Dao is null.</returns>
        public ulong? GetDbId(Dao dao)
        {
            Args.ThrowIfNull(dao, "dao");
            Args.ThrowIfNull(dao.DbId, "dao.DbId");
            if(dao == null)
            {
                return default;
            }
            return dao.GetDbId();
        }

        /// <summary>
        /// Creates a universal identifier resolver for the specified Dao instance.
        /// </summary>
        /// <param name="data">The Dao instance to create the resolver for.</param>
        /// <returns>An <see cref="IUniversalIdResolver"/> for the Dao.</returns>
        public IUniversalIdResolver GetUniversalIdentifier(Dao data)
        {
            return new UniversalIdResolver(data);
        }
        
        /// <summary>
        /// Gets the database ID from the specified object, which must be a Dao instance.
        /// </summary>
        /// <param name="obj">The object to get the ID from.</param>
        /// <returns>The database ID.</returns>
        public ulong? GetId(object obj)
        {
            if (obj is Dao dao)
            {
                return GetDbId(dao);
            }
            throw new InvalidOperationException("The specified object must be a Dao instance.");
        }
    }
}