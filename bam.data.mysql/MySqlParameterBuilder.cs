/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data.Common;
using MySql.Data.MySqlClient;

namespace Bam.Data
{
    public class MySqlParameterBuilder: ParameterBuilder
    {
        public override DbParameter BuildParameter(string name, object value)
        {
            return new MySqlParameter(EnsurePrefix(name, "@"), value);
        }
        public override DbParameter BuildParameter(IParameterInfo c)
        {
            return new MySqlParameter(string.Format("@{0}{1}", c.ColumnName, c.Number), c.Value);
        }
    }
}
