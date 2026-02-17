namespace Bam.Data
{
    /// <summary>
    /// A class that represents the Universal Deterministic Identifier for an object instance.
    /// </summary>
    public class UniversalDeterministicIdResolver: IUniversalIdResolver
    {
        /// <summary>
        /// Initializes a new UniversalDeterministicIdResolver with the specified data object.
        /// </summary>
        /// <param name="data">The data object to resolve an identifier for.</param>
        public UniversalDeterministicIdResolver(object data)
        {
            this.Data = data;
        }
        
        /// <summary>
        /// Gets or sets the data object associated with this resolver.
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// Gets the deterministic identifier for the specified data object. Not yet implemented.
        /// </summary>
        /// <param name="data">The data object to get an identifier for.</param>
        /// <returns>The deterministic identifier.</returns>
        public ulong GetId(object data)
        {
            // CLAUDE TODO: implement this as a combination of key value pairs of properties with
            // CompositeKeyAttribute and the namespace qualified name of the type followed by a comma
            // and the sha256 of the assembly that contains it.
            throw new System.NotImplementedException();
        }
    }
}