/*
	Copyright © Bryan Apellanes 2015  
*/

using Bam.Data.FirebirdSql;

namespace Bam.Data
{
    public class FirebirdSqlQuerySet : QuerySet
    {
        public FirebirdSqlQuerySet()
            : base()
        {
            this.GoText = ";\r\n";
            this.TableNameFormatter = (s) => "\"{0}\"".Format(s);
            this.ColumnNameFormatter = (s) => "\"{0}\"".Format(s);
        }

        public override SqlStringBuilder Id(string idAs)
        {
            Builder.AppendFormat(" RETURNING \"Id\" AS \"{0}\"{1}", idAs, this.GoText);
            return this;
        }

        public override SqlStringBuilder Where(IQueryFilter filter)
        {
            WhereFormat where = FirebirdSqlFormatProvider.GetWhereFormat(filter, StringBuilder, NextNumber);
            NextNumber = where.NextNumber;
            this.parameters.AddRange(where.Parameters);
            return this;
        }

        public override SqlStringBuilder Update(string tableName, params AssignValue[] values)
        {
            Builder.AppendFormat("UPDATE {0} ", TableNameFormatter(tableName));
            SetFormat set = FirebirdSqlFormatProvider.GetSetFormat(tableName, StringBuilder, NextNumber, values);
            NextNumber = set.NextNumber;
            this.parameters.AddRange(set.Parameters);
            return this;
        }

        public override SqlStringBuilder SelectTop(int topCount, string tableName, params string[] columnNames)
        {
            if (columnNames.Length == 0)
            {
                columnNames = new string[] { "*" };
            }
            string cols = columnNames.ToDelimited(s => string.Format("{0}", s));
            if (topCount > 0)
            {
                StringBuilder.AppendFormat("SELECT FIRST {0} {1} FROM {2} ", topCount, cols, TableNameFormatter(tableName));
            }
            else
            {
                StringBuilder.AppendFormat("SELECT {0} FROM {1} ", cols, TableNameFormatter(tableName));
            }
            return this;
        }
    }
}
