/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bam.Data;
using System.Data.Common;
using Bam.Incubation;
using System.Data.SQLite;

namespace Bam.Data
{
    public class SQLiteParameterBuilder: ParameterBuilder
    {
        public override DbParameter BuildParameter(string name, object value)
        {
            return new SQLiteParameter($"@{name}", value);
        }

        public override DbParameter BuildParameter(IParameterInfo c)
        {
            string parameterName = $"@{c.ColumnName}{c.Number}";
            object value = c.Value;
            if (value is DateTime || value is DateTime?)
            {
                value = new Instant((DateTime) value).ToDateTime();
            }

            SQLiteParameter result = new SQLiteParameter(parameterName, value);

            return result;
        }

    }
}
