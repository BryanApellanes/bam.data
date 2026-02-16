using Bam.Data;

namespace Bam
{
    /// <summary>
    /// Provides static path resolution for application-specific directories and named directory bundles.
    /// </summary>
    public class AppPaths
    {
        static AppPaths()
        {
            NamedDirectoryBundles = new Dictionary<string, List<Func<DirectoryInfo[]>>>();
        }
        /// <summary>
        /// Resolves {Paths.Apps}/{AppName}
        /// </summary>
        public static string AppRoot => Path.Combine(BamHome.AppsPath, AppName());

        /// <summary>
        /// Resolves {BamHome.Content}/apps/{AppName}
        /// </summary>
        public static string Content => Path.Combine(BamHome.ContentPath, "apps", AppName());

        /// <summary>
        /// Resolves {AppPaths.AppRoot}/services
        /// </summary>
        public static string Services => Path.Combine(AppRoot, "services");

        /// <summary>
        /// Resolves the application data directory path for the current data source provider.
        /// </summary>
        public static string Data => DataPaths.Get(DataSourceProvider.Current, ProcessApplicationNameProvider.Current).AppData;

        /// <summary>
        /// Gets the dictionary of named directory bundles, where each key maps to a list of directory retrieval functions.
        /// </summary>
        public static Dictionary<string, List<Func<DirectoryInfo[]>>> NamedDirectoryBundles { get; }

        /// <summary>
        /// Gets the service directories, optionally adding a new directory retriever.
        /// </summary>
        /// <param name="directoryBundleRetriever">An optional function that returns directories to add to the services bundle.</param>
        /// <returns>An array of service directory infos.</returns>
        public static DirectoryInfo[] GetServicesDirectories(Func<DirectoryInfo[]>? directoryBundleRetriever = null)
        {
            return GetDirectories("services", directoryBundleRetriever);
        }
        
        /// <summary>
        /// Gets directories for the named bundle, optionally registering a new directory retriever.
        /// </summary>
        /// <param name="directoryBundleName">The name of the directory bundle.</param>
        /// <param name="directoryBundleRetriever">An optional function to add to the bundle.</param>
        /// <returns>An array of directory infos from all retrievers in the bundle.</returns>
        public static DirectoryInfo[] GetDirectories(string directoryBundleName, Func<DirectoryInfo[]>? directoryBundleRetriever = null)
        {
            if (directoryBundleRetriever != null)
            {
                AddDirectories(directoryBundleName, directoryBundleRetriever);
            }

            if (NamedDirectoryBundles.ContainsKey(directoryBundleName))
            {
                return NamedDirectoryBundles[directoryBundleName].SelectMany(f => f()).ToArray();
            }

            return new DirectoryInfo[] { };
        }

        /// <summary>
        /// Adds a directory retriever function to the services bundle.
        /// </summary>
        /// <param name="getServiceDirectories">A function that returns service directories.</param>
        public static void AddServiceDirectories(Func<DirectoryInfo[]> getServiceDirectories)
        {
            AddDirectories("services", getServiceDirectories);
        }
        
        /// <summary>
        /// Adds a directory retriever function to the specified named directory bundle.
        /// </summary>
        /// <param name="directoryBundleName">The name of the directory bundle to add to.</param>
        /// <param name="directoryBundleRetriever">A function that returns directories for the bundle.</param>
        public static void AddDirectories(string directoryBundleName, Func<DirectoryInfo[]> directoryBundleRetriever)
        {
            if (!NamedDirectoryBundles.ContainsKey(directoryBundleName))
            {
                NamedDirectoryBundles[directoryBundleName] = new List<Func<DirectoryInfo[]>>();
            }

            NamedDirectoryBundles[directoryBundleName].Add(directoryBundleRetriever);
        }

        private static string AppName()
        {
            return ProcessApplicationNameProvider.Current.GetApplicationName();
        }
    }
}