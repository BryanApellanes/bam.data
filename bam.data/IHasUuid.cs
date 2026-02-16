namespace Bam.Data
{
    /// <summary>
    /// Indicates that an object has a Uuid (universally unique identifier) property.
    /// </summary>
    public interface IHasUuid
    {
        /// <summary>
        /// Gets the universally unique identifier.
        /// </summary>
        string Uuid { get; }
    }
}
