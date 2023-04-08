using System;
using System.Collections.Generic;
using System.Text;
using Bam.Net.Logging;

namespace Bam.Net.Data
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
            Logger = Log.Default;
        }

        public static Hydrator DefaultHydrator { get; set; }

        public ILogger Logger { get; set; }

        public bool TryHydrateChildren(IDao dao, IDatabase database = null)
        {
            try
            {
                HydrateChildren(dao, database);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Exception hydrating dao of type ({0}): {1}", ex, dao?.GetType()?.Name, ex.Message);
                return false;
            }
        }

        public void HydrateChildren(IDao dao, IDatabase database = null)
        {
            dao.HydrateChildren(database);
        }
    }
}
