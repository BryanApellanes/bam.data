﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Data
{
    [Serializable]
    public class DatabaseExecutionEventArgs: EventArgs
    {
        public Database Database { get; set; }
        public DbDataReader DataReader { get; set; }
        public DbCommand Command { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}
