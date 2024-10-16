﻿using System;
using System.Collections.Generic;
using System.IO;
using Bam.Configuration;
using Bam.Data.SQLite;
using Bam.Logging;
using Bam.UserAccounts;

namespace Bam.Data.Repositories
{
    public class DataProvider : DatabaseProvider<SQLiteDatabase>, IDataDirectoryProvider
    {
        public DataProvider()
        {
            DataRootDirectory = BamHome.DataPath;
            AppDataDirectory = "AppData";
            UsersDirectory = "Users";
            SysDataDirectory = "SysData";
            DatabaseDirectory = "Databases";
            RepositoryDirectory = "Repositories";
            FilesDirectory = "Files";
            ChunksDirectory = "Chunks";
            WorkspacesDirectory = "Workspaces";
            EmailTemplatesDirectory = "EmailTemplates";
            AssemblyDirectory = "Assemblies";
            ProcessMode = ProcessMode.Current;
            Logger = Log.Default;            
        }

        public DataProvider(ProcessMode processMode, ILogger logger = null):this()
        {
            ProcessMode = processMode;
            Logger = logger ?? Log.Default;
        }

        public static SystemPaths GetPaths()
        {
            return SystemPaths.Get(Instance);
        }

        public static DataPaths GetDataPaths()
        {
            return GetDataPaths(ProcessMode.Current);
        }

        public static DataPaths GetDataPaths(ProcessModes mode)
        {
            return GetDataPaths(ProcessMode.FromEnum(mode));
        }

        public static DataPaths GetDataPaths(ProcessMode mode)
        {
            return DataPaths.Get(new DataProvider(mode));
        }

        public ProcessMode ProcessMode { get; set; }

        /// <summary>
        /// The root directory used to store any data files provided by this component.
        /// The default is BamHome.DataPath.
        /// </summary>
        public string DataRootDirectory { get; set; }
        public string AppDataDirectory { get; set; }
        public string UsersDirectory { get; set; }
        public string SysDataDirectory { get; set; }
        public string DatabaseDirectory { get; set; }
        public string RepositoryDirectory { get; set; }
        public string FilesDirectory { get; set; }
        public string ChunksDirectory { get; set; }
        public string WorkspacesDirectory { get; set; }
        public string EmailTemplatesDirectory { get; set; }
        public string AssemblyDirectory { get; set; }

        static DataProvider _default;
        static readonly object _defaultLock = new object();
        /// <summary>
        /// Gets the default instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static DataProvider Instance
        {
            get
            {
                return _defaultLock.DoubleCheckLock(ref _default, () => new DataProvider());
            }
        }

        static DataProvider _fromConfig;
        static readonly object _fromConfigLock = new object();
        /// <summary>
        /// Gets the current instance configured for the current ProcessMode.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        public static DataProvider Current
        {
            get
            {
                return _fromConfigLock.DoubleCheckLock(ref _fromConfig, () => new DataProvider(ProcessMode.Current));
            }
        }

        public void Init(IUserManager userManager)
        {
            Init(DefaultConfigurationApplicationNameProvider.Instance, userManager);
        }

        public void Init(IApplicationNameProvider appNameProvider, IUserManager userManager)
        {            
            SetRuntimeAppDataDirectory(appNameProvider);
            //User.UserDatabase = userManager.Database;
            //Vault.SystemVaultDatabase = Current.GetSysDatabaseFor(typeof(Vault), "System");
            //Vault.ApplicationVaultDatabase = Current.GetAppDatabaseFor(appNameProvider, typeof(Vault), appNameProvider.GetApplicationName());
        }

        public void SetRuntimeAppDataDirectory()
        {
            SetRuntimeAppDataDirectory(DefaultConfigurationApplicationNameProvider.Instance);
        }

        /// <summary>
        /// Sets RuntimeSettings.ProcessDataFolder to a location appropriate to the application name provided by the
        /// specified IApplicationNameProvider.
        /// </summary>
        /// <param name="appNameProvider"></param>
        public void SetRuntimeAppDataDirectory(IApplicationNameProvider appNameProvider)
        {
            RuntimeSettings.ProcessDataFolder = GetAppDataDirectory(appNameProvider).FullName;
        }

        public DirectoryInfo GetRootDataDirectory()
        {
            return new DirectoryInfo(Path.Combine(DataRootDirectory, ProcessMode.ToString()));
        }

        public DirectoryInfo GetRootDataDirectory(params string[] pathSegments)
        {
            List<string> segments = new List<string>
            {
                GetRootDataDirectory().FullName
            };
            segments.AddRange(pathSegments);
            return new DirectoryInfo(Path.Combine(segments.ToArray()));
        }

        public DirectoryInfo GetSysDataDirectory()
        {
            return new DirectoryInfo(Path.Combine(GetRootDataDirectory().FullName, SysDataDirectory));
        }

        public DirectoryInfo GetSysDataDirectory(params string[] pathSegments)
        {
            List<string> segments = new List<string>
            {
                GetSysDataDirectory().FullName
            };
            segments.AddRange(pathSegments);
            return new DirectoryInfo(Path.Combine(segments.ToArray()));
        }

        public DirectoryInfo GetSysUsersDataDirectory()
        {
            return GetSysDataDirectory(UsersDirectory);
        }

        public DirectoryInfo GetSysAssemblyDirectory()
        {
            return GetSysDataDirectory(AssemblyDirectory);
        }

        public DirectoryInfo GetAppAssemblyDirectory(IApplicationNameProvider appNameProvider)
        {
            return GetAppDataDirectory(appNameProvider, AssemblyDirectory);
        }

        public DirectoryInfo GetAppDataDirectory(IApplicationNameProvider appNameProvider)
        {
            return new DirectoryInfo(Path.Combine(GetRootDataDirectory().FullName, AppDataDirectory, appNameProvider.GetApplicationName()));
        }

        public DirectoryInfo GetAppUsersDirectory(IApplicationNameProvider appNameProvider)
        {
            return GetAppDataDirectory(appNameProvider, UsersDirectory);
        }

        public DirectoryInfo GetAppDataDirectory(IApplicationNameProvider appNameProvider, params string[] directoryName)
        {
            List<string> pathSegments = new List<string>()
            {
                GetAppDataDirectory(appNameProvider).FullName
            };
            pathSegments.AddRange(directoryName);
            
            return new DirectoryInfo(Path.Combine(pathSegments.ToArray()));
        }
        
        public DirectoryInfo GetAppDatabaseDirectory(IApplicationNameProvider appNameProvider)
        {
            return GetAppDataDirectory(appNameProvider, DatabaseDirectory);
        }

        public DirectoryInfo GetSysDatabaseDirectory()
        {
            return GetSysDataDirectory(DatabaseDirectory);
        }
        
        public DirectoryInfo GetAppRepositoryDirectory(IApplicationNameProvider appNameProvider)
        {
            return GetAppDataDirectory(appNameProvider, RepositoryDirectory);
        }

        public DirectoryInfo GetAppRepositoryDirectory(IApplicationNameProvider appNameProvider, string subDirectory)
        {
            return new DirectoryInfo(Path.Combine(GetAppDataDirectory(appNameProvider).FullName, subDirectory));
        }

        public DirectoryInfo GetSysRepositoryDirectory()
        {
            return GetSysDataDirectory(RepositoryDirectory);
        }

        public DirectoryInfo GetSysRepositoryDirectory(string subDirectory)
        {
            return new DirectoryInfo(Path.Combine(GetSysRepositoryDirectory().FullName, subDirectory));
        }

        public T GetSysDaoRepository<T>() where T: IDaoRepository, new()
        {
            T result = new T();
            result.Database = GetSysDatabaseFor(result);
            result.EnsureDaoAssemblyAndSchema();
            return result;
        }

        public T GetAppDaoRepository<T>(IApplicationNameProvider applicationNameProvider) where T : IDaoRepository, new()
        {
            T result = new T
            {
                Database = GetAppDatabaseFor(applicationNameProvider, typeof(T))
            };
            result.EnsureDaoAssemblyAndSchema();
            return result;
        }

        public DirectoryInfo GetAppRepositoryWorkspaceDirectory(IApplicationNameProvider appNameProvider)
        {
            return GetAppDataDirectory(appNameProvider, WorkspacesDirectory);
        }

        public DirectoryInfo GetRepositoryWorkspaceDirectory()
        {
            return GetRootDataDirectory(RepositoryDirectory);
        }

        public DirectoryInfo GetAppFilesDirectory(IApplicationNameProvider appNameProvider)
        {
            return GetAppDataDirectory(appNameProvider, FilesDirectory);
        }

        public DirectoryInfo GetFilesDirectory()
        {
            return GetRootDataDirectory(FilesDirectory);
        }
        
        public DirectoryInfo GetChunksDirectory()
        {
            return GetRootDataDirectory(ChunksDirectory);
        }

        public DirectoryInfo GetAppWorkspaceDirectory(IApplicationNameProvider appNameProvider, Type type)
        {
            string hash = type.ToInfoHash();
            return GetAppWorkspaceDirectory(appNameProvider, type.Name, hash);
        }

        public DirectoryInfo GetAppWorkspaceDirectory(IApplicationNameProvider appNameProvider, string workspaceName, string hash)
        {
            return new DirectoryInfo(Path.Combine(GetAppDataDirectory(appNameProvider).FullName, WorkspacesDirectory, workspaceName, hash));
        }

        public DirectoryInfo GetWorkspaceDirectory(Type type)
        {
            string hash = type.ToInfoHash();
            return new DirectoryInfo(Path.Combine(GetRootDataDirectory().FullName, WorkspacesDirectory, type.Name, hash));
        }
        
        public DirectoryInfo GetAppEmailTemplatesDirectory(IApplicationNameProvider appNameProvider)
        {
            return GetAppDataDirectory(appNameProvider, EmailTemplatesDirectory);
        }

        public DirectoryInfo GetSysEmailTemplatesDirectory()
        {
            return GetSysDataDirectory(EmailTemplatesDirectory);
        }

        public void SetSysDatabaseFor(object instance)
        {
            instance.Property("Database", GetSysDatabaseFor(instance), false);
        }        

        /// <summary>
        /// Get a SQLiteDatabase for the specified object instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public override SQLiteDatabase GetSysDatabaseFor(object instance)
        {
            string databaseName = instance.GetType().FullName;
            string schemaName = instance.Property<string>("SchemaName", false);
            if (!string.IsNullOrEmpty(schemaName))
            {
                databaseName = $"{databaseName}_{schemaName}";
            }
            return new SQLiteDatabase(GetSysDatabaseDirectory().FullName, databaseName);
        }

        /// <summary>
        /// Get the standard path for the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public override string GetSysDatabasePathFor(Type type, string info = null)
        {
            return GetSysDatabaseFor(type, info).DatabaseFile.FullName;
        }
        
        public override SQLiteDatabase GetSysDatabaseFor(Type objectType, string info = null)
        {
            return GetDatabaseFor(objectType, () => GetSysDatabaseDirectory().FullName, info);
        }

        public override SQLiteDatabase GetAppDatabaseFor(IApplicationNameProvider appNameProvider, object instance)
        {
            return GetDatabaseFor(instance.GetType(), () => GetAppDatabaseDirectory(appNameProvider).FullName);
        }

        public override SQLiteDatabase GetAppDatabaseFor(IApplicationNameProvider appNameProvider, Type objectType, string info = null)
        {
            return GetDatabaseFor(objectType, () => GetAppDatabaseDirectory(appNameProvider).FullName, info);
        }

        /// <summary>
        /// Get the path to the application specific SQLite database file for the specified type
        /// </summary>
        /// <param name="appNameProvider"></param>
        /// <param name="type"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public override string GetAppDatabasePathFor(IApplicationNameProvider appNameProvider, Type type, string info = null)
        {
            return GetAppDatabaseFor(appNameProvider, type, info).DatabaseFile.FullName;
        }

        protected SQLiteDatabase GetDatabaseFor(Type objectType, Func<string> databasePathProvider, string info = null)
        {
            string connectionName = Dao.ConnectionName(objectType);
            string fileName = string.IsNullOrEmpty(info) ? (string.IsNullOrEmpty(connectionName) ? objectType.FullName : connectionName) : $"{objectType.FullName}_{info}";
            string directoryPath = databasePathProvider();
            SQLiteDatabase db = new SQLiteDatabase(directoryPath, fileName);
            Logger.Info("Returned SQLiteDatabase with path {0} for type {1}\r\nFullPath: {2}\r\nName: {3}", db.DatabaseFile.FullName, objectType.Name, directoryPath, fileName);
            return db;
        }

        public override SQLiteDatabase GetAppDatabase(IApplicationNameProvider appNameProvider, string databaseName)
        {
            return new SQLiteDatabase(GetAppDatabaseDirectory(appNameProvider).FullName, databaseName);
        }

        public override SQLiteDatabase GetSysDatabase(string databaseName)
        {
            return new SQLiteDatabase(GetSysDatabaseDirectory().FullName, databaseName);
        }
    }
}
