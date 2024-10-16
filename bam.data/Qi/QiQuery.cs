/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bam.Data;
using System.Data;
using System.Data.Common;

namespace Bam.Data.Qi
{
    /// <summary>
    /// A class that represents a query that
    /// may be sent by JavaScript Query Interface
    /// qi.js
    /// </summary>
    public class QiQuery: IQueryFilter
    {
        public string columns { get; set; }
        public string cxName { get; set; }
        public string parsed { get; set; }
        public string table { get; set; }
        public QiClause[] clauses { get; set; }
        public object[] values { get; set; }

        public int limit { get; set; }

        #region IQueryFilter Members

        public string Parse()
        {
            // parse the filters property as where clauses
            // define filter.ToString to return [property] <transformed operator_> [parameterName]
            // verify token filters (AND, OR, OPENPAREN, CLOSEPAREN) all work as they should 
            // call ToString on all the filters and append the result to a string builder as 
            // the return value

            StringBuilder result = new StringBuilder();
            foreach (QiClause f in clauses)
            {
                result.Append(f.ToComparison().ToString());
            }
            return result.ToString();
        }

        public string Parse(int? number)
        {
            return Parse();
        }

        #endregion

        #region IHasFilters Members

        public IEnumerable<IFilterToken> Filters => this.Parameters;

        #endregion

        #region IHasParameterInfos Members

        public IParameterInfo[] Parameters
        {
            get => clauses;
            set
            {
                //throw new NotImplementedException();
            }
        }

        #endregion
    }

}
