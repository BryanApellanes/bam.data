﻿using Bam.Data;
using Bam.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    /// <summary>
    /// A class referencing all file system paths of importance to the bam system.
    /// </summary>
    public class SystemPaths
    {
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

        public static SystemPaths Get(IDataDirectoryProvider dataDirectoryProvider)
        {
            return new SystemPaths()
            {
                Data = DataPaths.Get(dataDirectoryProvider)
            };
        }

        public static SystemPaths Current
        {
            get
            {
                return Get(DataProvider.Current);
            }
        }

        public DataPaths Data { get; set; }

        public string Root { get; set; }
        public string Public { get; set; }
        public string SystemDrive { get; set; }

        public string Apps { get; set; }
        public string Local { get; set; }
        public string Content { get; set; }
        public string Conf { get; set; }
        public string Sys { get; set; }
        public string Generated { get; set; }
        public string Proxies { get; set; }
        public string Logs { get; set; }
        public string Tools { get; set; }
        public string NugetPackages { get; set; }

        public string Tests { get; set; }
        public string Builds { get; set; }
    }
}
