/*
	Copyright © Bryan Apellanes 2015  
*/

using Bam.DependencyInjection;
using Bam.Services;

namespace Bam.Data
{
    public class MsSqlSqlStringBuilder: SchemaWriter
    {
        public static void Register(DependencyProvider incubator)
        {
            MsSqlSqlStringBuilder builder = new MsSqlSqlStringBuilder();
            incubator.Set(typeof(SqlStringBuilder), builder);
            incubator.Set<SqlStringBuilder>(builder);
        }

        public override string GetKeyColumnDefinition(KeyColumnAttribute keyColumn)
        {
            return string.Format(KeyColumnFormat, GetColumnDefinition(keyColumn));
        }

        public override string GetColumnDefinition(ColumnAttribute column)
		{
			if ("vector".Equals(column.DbDataType, StringComparison.OrdinalIgnoreCase))
			{
				throw new NotSupportedException($"{this.GetType().Name} does not support vector columns: declared as {column.Name}.");
			}
			if ("uuid[]".Equals(column.DbDataType, StringComparison.OrdinalIgnoreCase))
			{
				throw new NotSupportedException($"{this.GetType().Name} does not support uuid[] columns: declared as {column.Name}.");
			}
			if ("jsonb".Equals(column.DbDataType, StringComparison.OrdinalIgnoreCase))
			{
				return string.Format("\"{0}\" NVARCHAR(MAX){1}{2}", column.Name, GetColumnDefaultClause(column), column.AllowNull ? "" : " NOT NULL");
			}
			string max = string.Format("({0})", column.MaxLength);
			string type = column.DbDataType.ToLowerInvariant();

			if (type.Equals("bigint") ||
				type.Equals("int") ||
				type.Equals("datetime") ||
				type.Equals("bit"))
			{
				max = string.Empty;
			}
			else if (type.Equals("decimal"))
			{
				max = string.Format("({0}, 2)", column.MaxLength);
			}

			return string.Format("\"{0}\" {1}{2}{3}", column.Name, column.DbDataType, max, column.AllowNull ? "" : " NOT NULL");
		}
    }
}
