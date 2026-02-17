using Bam.Logging;

namespace Bam.Data
{
    /// <summary>
    /// Provides hydration mechanism for Dao instances; loads child collections on hydrate.
    /// </summary>
    public class Hydrator : IHydrator
    {
        static Hydrator()
        {
            DefaultHydrator = new Hydrator();
        }

        public Hydrator()
        {
            Logger = Log.Default!;
        }

        /// <summary>
        /// Gets or sets the default Hydrator instance used when no other is configured.
        /// </summary>
        public static Hydrator DefaultHydrator { get; set; }

        /// <summary>
        /// Gets or sets the logger used for error reporting during hydration.
        /// </summary>
        public ILogger Logger { get; set; } = null!;

        /// <summary>
        /// Attempts to hydrate the child collections of the specified Dao instance, returning false on failure.
        /// </summary>
        /// <param name="dao">The Dao instance to hydrate.</param>
        /// <param name="database">Optional database to use for loading children.</param>
        /// <returns>True if hydration succeeded, false otherwise.</returns>
        public bool TryHydrateChildren(IDao dao, IDatabase? database = null!)
        {
            try
            {
                HydrateChildren(dao, database);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Exception hydrating dao of type ({0}): {1}", ex, dao?.GetType()?.Name!, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Hydrates the child collections of the specified Dao instance.
        /// </summary>
        /// <param name="dao">The Dao instance to hydrate.</param>
        /// <param name="database">Optional database to use for loading children.</param>
        public void HydrateChildren(IDao dao, IDatabase? database = null!)
        {
            dao.HydrateChildren(database);
        }
    }
}
