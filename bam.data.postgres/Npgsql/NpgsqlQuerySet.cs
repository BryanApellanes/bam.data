/*
	Copyright © Bryan Apellanes 2015  
*/

using Bam.Data.Npgsql;

namespace Bam.Data
{
    public class NpgsqlQuerySet : QuerySet
    {
        public NpgsqlQuerySet()
            : base()
        {
            this.GoText = ";\r\n";
            this.TableNameFormatter = (s) =>
            {
                if (this.Database is NpgsqlDatabase postgresDb && !string.IsNullOrEmpty(postgresDb.PostgresSchema))
                {
                    return $"{postgresDb.PostgresSchema}.{s}";
                }
                return $"{s}";
            };
            this.ColumnNameFormatter = NpgsqlFormatProvider.ColumnNameFormatter;
        }
        public override SqlStringBuilder Id(string idAs)
        {
            Builder.AppendFormat(" RETURNING Id AS {0}{1}", idAs, this.GoText);
            return this;
        }

        public override SqlStringBuilder Where(IQueryFilter filter)
        {
            WhereFormat where = NpgsqlFormatProvider.GetWhereFormat(filter, StringBuilder, NextNumber);
            NextNumber = where.NextNumber;
            this.parameters.AddRange(where.Parameters);
            return this;
        }

        public override SqlStringBuilder Update(string tableName, params AssignValue[] values)
        {
            Builder.AppendFormat("UPDATE {0} ", TableNameFormatter(tableName));
            SetFormat set = NpgsqlFormatProvider.GetSetFormat(tableName, StringBuilder, NextNumber, values);
            NextNumber = set.NextNumber;
            this.parameters.AddRange(set.Parameters);
            return this;
        }

        public int Limit
        {
            get;
            set;
        }

        public override SqlStringBuilder SelectTop(int topCount, string tableName, params string[] columnNames)
        {
            this.Limit = topCount;

            if (columnNames.Length == 0)
            {
                columnNames = new string[] { "*" };
            }
            string cols = columnNames.ToDelimited(s => $"{s}");
            StringBuilder.AppendFormat("SELECT {0} FROM {1} ", cols, TableNameFormatter(tableName));
            return this;
        }

        public override void Execute(IDatabase db)
        {
            if (Limit > 0)
            {
                Go();
            }
            base.Execute(db);
        }

        public override SqlStringBuilder Go()
        {
            if (this.Limit > 0)
            {
                StringBuilder.AppendFormat(" Limit {0} ", this.Limit);
            }

            base.Go();
            this.Limit = -1;
            return this;
        }
    }
}
