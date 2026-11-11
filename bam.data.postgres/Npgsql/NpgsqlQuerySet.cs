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
            Builder.AppendFormat(" RETURNING {0} AS {1}{2}", ColumnNameFormatter("Id"), idAs, this.GoText);
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

        /// <summary>
        /// The row cap <see cref="SelectTop"/> records for the statement being built; <see cref="Go"/> appends it
        /// as a LIMIT clause and resets it to -1. Named so it does not hide the inherited
        /// <see cref="SqlStringBuilder.Limit(int)"/> method.
        /// </summary>
        public int LimitCount
        {
            get;
            set;
        }

        public override SqlStringBuilder SelectTop(int topCount, string tableName, params string[] columnNames)
        {
            this.LimitCount = topCount;

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
            if (LimitCount > 0)
            {
                Go();
            }
            base.Execute(db);
        }

        public override SqlStringBuilder Go()
        {
            if (this.LimitCount > 0)
            {
                StringBuilder.AppendFormat(" Limit {0} ", this.LimitCount);
            }

            base.Go();
            this.LimitCount = -1;
            return this;
        }
    }
}
