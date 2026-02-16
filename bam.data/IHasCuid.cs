namespace Bam.Data
{
    /// <summary>
    /// Indicates that an object has a Cuid (collision-resistant unique identifier) property.
    /// </summary>
    public interface IHasCuid
    {
        /// <summary>
        /// Gets the collision-resistant unique identifier.
        /// </summary>
        string Cuid { get; }
    }
}
