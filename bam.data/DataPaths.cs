using Bam.Configuration;

namespace Bam.Data
{
    /// <summary>
    /// Represents resolved directory paths for application data, system data, and related resources.
    /// </summary>
    public class DataPaths
    {
        /// <summary>
        /// Creates a DataPaths instance by resolving directories from the specified providers.
        /// </summary>
        /// <param name="dataDirectoryProvider">The provider that resolves directory paths.</param>
        /// <param name="applicationNameProvider">The optional application name provider; defaults to DefaultConfigurationApplicationNameProvider.</param>
        /// <returns>A DataPaths instance with resolved paths.</returns>
        public static DataPaths Get(IDataDirectoryProvider dataDirectoryProvider, IApplicationNameProvider applicationNameProvider = null!)
        {
            applicationNameProvider = applicationNameProvider ?? DefaultConfigurationApplicationNameProvider.Instance;
            return new DataPaths
            {
                DataRoot = dataDirectoryProvider.GetRootDataDirectory().FullName,
                SysData = dataDirectoryProvider.GetSysDataDirectory().FullName,

                AppData = dataDirectoryProvider.GetAppDataDirectory(applicationNameProvider).FullName,
                UserData = dataDirectoryProvider.GetAppUsersDirectory(applicationNameProvider).FullName,
                AppDatabase = dataDirectoryProvider.GetAppDatabaseDirectory(applicationNameProvider).FullName,
                AppRepository = dataDirectoryProvider.GetAppRepositoryDirectory(applicationNameProvider).FullName,
                AppFiles = dataDirectoryProvider.GetAppFilesDirectory(applicationNameProvider).FullName,
                AppEmailTemplates = dataDirectoryProvider.GetAppEmailTemplatesDirectory(applicationNameProvider).FullName
            };
        }

        /// <summary>
        /// Gets or sets the root data directory path.
        /// </summary>
        public string DataRoot { get; set; } = null!;

        /// <summary>
        /// Gets or sets the system data directory path.
        /// </summary>
        public string SysData { get; set; } = null!;

        /// <summary>
        /// Gets or sets the application data directory path.
        /// </summary>
        public string AppData { get; set; } = null!;

        /// <summary>
        /// Gets or sets the user data directory path.
        /// </summary>
        public string UserData { get; set; } = null!;

        /// <summary>
        /// Gets or sets the application database directory path.
        /// </summary>
        public string AppDatabase { get; set; } = null!;

        /// <summary>
        /// Gets or sets the application repository directory path.
        /// </summary>
        public string AppRepository { get; set; } = null!;

        /// <summary>
        /// Gets or sets the application files directory path.
        /// </summary>
        public string AppFiles { get; set; } = null!;

        /// <summary>
        /// Gets or sets the application email templates directory path.
        /// </summary>
        public string AppEmailTemplates { get; set; } = null!;
    }
}
