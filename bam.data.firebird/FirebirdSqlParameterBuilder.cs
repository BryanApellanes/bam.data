/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace Bam.Data
{
    public class FirebirdSqlParameterBuilder : ParameterBuilder
    {
        public override DbParameter BuildParameter(string name, object value)
        {
            return new FbParameter(EnsurePrefix(name, "@"), value);
        }
        public override DbParameter BuildParameter(IParameterInfo c)
        {
            string parameterName = string.Format("@{0}{1}", c.ColumnName, c.Number);
            object value = c.Value!;
            if (value is DateTime || value is DateTime?)
            {
                value = new Instant((DateTime)value).ToDateTime();
            }
            return new FbParameter(parameterName, value);
        }
    }
}
