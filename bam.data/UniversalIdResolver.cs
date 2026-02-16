namespace Bam.Data
{
    /// <summary>
    /// Resolves a universal identifier for a given data object.
    /// </summary>
    public class UniversalIdResolver: IUniversalIdResolver
    {
        /// <summary>
        /// Initializes a new UniversalIdResolver with the specified data object.
        /// </summary>
        /// <param name="data">The data object to resolve an identifier for.</param>
        public UniversalIdResolver(object data)
        {
            Data = data;
        }
        public object Data { get; set; }
        
        public ulong GetId(object data)
        {
            throw new System.NotImplementedException();
        }
    }
}