/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bam.Net.Data;
using System.Data;
using System.Data.Common;

namespace Bam.Net.Data
{
    /// <summary>
    /// Convenience entry point for contextually readable syntax; the same as Filter
    /// </summary>
    public static class Query
	{
        public static QueryValue Value(object value)
        {
            return new QueryValue(value);
        }
        /// <summary>
        /// Convenience entry point to
        /// creating a QueryFilter
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
		public static QueryFilter Where(string columnName)
		{
			return new QueryFilter(columnName);
		}

        /// <summary>
        /// Convenience entry point to
        /// creating a QueryFilter
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryFilter Where(dynamic query)
        {
            return QueryFilter.FromDynamic(query);
        }
	}


}
