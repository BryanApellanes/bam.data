/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public class SQLiteQuerySet: QuerySet
    {
        public override SqlStringBuilder Id(string idAs)
        {
            StringBuilder.AppendFormat("{0}SELECT last_insert_rowid() AS {1}", this.GoText, idAs);
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

            string cols = columnNames.ToDelimited(s => string.Format("{0}", s));
            StringBuilder.AppendFormat("SELECT {0} FROM [{1}] ", cols, tableName);
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
