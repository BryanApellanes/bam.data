using Bam.Data;

namespace Bam
{
    /// <summary>
    /// A class referencing all file system paths of importance to the bam system.
    /// </summary>
    public class SystemPaths
    {
        /// <summary>
        /// Initializes a new SystemPaths instance with default paths resolved from BamHome and BamProfile.
        /// </summary>
        public SystemPaths()
        {
            Root = BamHome.Path;
            Public = BamHome.PublicPath;
            SystemDrive = BamHome.SystemRoot;
            Apps = BamHome.AppsPath;
            Local = BamHome.Local;
            Content = BamHome.ContentPath;
            Conf = BamHome.ConfigPath;
            Generated = BamProfile.GeneratedPath;
            Proxies = BamProfile.ProxiesPath;
            Logs = BamProfile.LogsPath;
            Tools = BamHome.ToolsPath;
        }

        /// <summary>
        /// Creates a new SystemPaths instance with data paths resolved from the specified data directory provider.
        /// </summary>
        /// <param name="dataDirectoryProvider">The provider used to resolve data directory paths.</param>
        /// <returns>A new SystemPaths instance with configured data paths.</returns>
        public static SystemPaths Get(IDataDirectoryProvider dataDirectoryProvider)
        {
            return new SystemPaths()
            {
                Data = DataPaths.Get(dataDirectoryProvider)
            };
        }

        /// <summary>
        /// Gets the current SystemPaths using the current DataSourceProvider.
        /// </summary>
        public static SystemPaths Current
        {
            get
            {
                return Get(DataSourceProvider.Current);
            }
        }

        /// <summary>
        /// Gets or sets the data-related paths.
        /// </summary>
        public DataPaths Data { get; set; }

        /// <summary>
        /// Gets or sets the root path of the bam system.
        /// </summary>
        public string Root { get; set; }
        /// <summary>
        /// Gets or sets the public path.
        /// </summary>
        public string Public { get; set; }
        /// <summary>
        /// Gets or sets the system drive root path.
        /// </summary>
        public string SystemDrive { get; set; }

        /// <summary>
        /// Gets or sets the applications path.
        /// </summary>
        public string Apps { get; set; }
        /// <summary>
        /// Gets or sets the local path.
        /// </summary>
        public string Local { get; set; }
        /// <summary>
        /// Gets or sets the content path.
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Gets or sets the configuration path.
        /// </summary>
        public string Conf { get; set; }
        /// <summary>
        /// Gets or sets the system path.
        /// </summary>
        public string Sys { get; set; }
        /// <summary>
        /// Gets or sets the generated code path.
        /// </summary>
        public string Generated { get; set; }
        /// <summary>
        /// Gets or sets the proxies path.
        /// </summary>
        public string Proxies { get; set; }
        /// <summary>
        /// Gets or sets the logs path.
        /// </summary>
        public string Logs { get; set; }
        /// <summary>
        /// Gets or sets the tools path.
        /// </summary>
        public string Tools { get; set; }
        /// <summary>
        /// Gets or sets the NuGet packages path.
        /// </summary>
        public string NugetPackages { get; set; }

        /// <summary>
        /// Gets or sets the tests path.
        /// </summary>
        public string Tests { get; set; }
        /// <summary>
        /// Gets or sets the builds path.
        /// </summary>
        public string Builds { get; set; }
    }
}
