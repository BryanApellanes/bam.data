using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bam.Data;
using Bam.Data.Repositories;

namespace Bam
{
    // TODO: derive an AppPaths instance that represents all relevant path information for a running application.  AppPaths appPaths = AppPaths.ForApplication(IApplicationNameProvider appNameProvider)
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

        public static string Data => DataPaths.Get(DataProvider.Current, ProcessApplicationNameProvider.Current).AppData;

        public static Dictionary<string, List<Func<DirectoryInfo[]>>> NamedDirectoryBundles { get; }

        public static DirectoryInfo[] GetServicesDirectories(Func<DirectoryInfo[]>? directoryBundleRetriever = null)
        {
            return GetDirectories("services", directoryBundleRetriever);
        }
        
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

        public static void AddServiceDirectories(Func<DirectoryInfo[]> getServiceDirectories)
        {
            AddDirectories("services", getServiceDirectories);
        }
        
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