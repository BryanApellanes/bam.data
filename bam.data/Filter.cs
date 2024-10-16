﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Data
{
    /// <summary>
    /// Convenience entry point for contextually readable syntax; the same as Query
    /// </summary>
    public static class Filter
    {
        public static QueryFilter Column(string columnName)
        {
            return new QueryFilter(columnName);
        }
        public static QueryFilter Where(string columnName)
        {
            return new QueryFilter(columnName);
        }

        public static QueryValue Value(object value)
        {
            return new QueryValue(value);
        }
    }
}
