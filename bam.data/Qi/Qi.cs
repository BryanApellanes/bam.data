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
    public class Qi
    {      

        public static DataTable Where(QiQuery query)
        {
            IDatabase db = Db.For(query.cxName);
            SqlStringBuilder sql = new SqlStringBuilder();
            sql
                .Select(query.table, query.columns)
                .Where(query);

            IParameterBuilder parameterBuilder = db.ServiceProvider.Get<IParameterBuilder>();
            DbParameter[] parameters = parameterBuilder.GetParameters(sql);
            return db.GetDataTable(sql, System.Data.CommandType.Text, parameters);
        }
    }
}
