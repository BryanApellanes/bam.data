/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.Common;
using Bam.Incubation;
using Bam;
using Bam.Data;
using MySql.Data.Common;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace Bam.Data
{
    public class MySqlParameterBuilder: ParameterBuilder
    {
        public override DbParameter BuildParameter(string name, object value)
        {
            return new MySqlParameter($"@{name}", value);
        }
        public override DbParameter BuildParameter(IParameterInfo c)
        {
            return new MySqlParameter(string.Format("@{0}{1}", c.ColumnName, c.Number), c.Value);
        }
    }
}
