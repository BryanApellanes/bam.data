/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public class NullComparison: Comparison
    {
        public NullComparison(string columnName, string oper)
            : base(columnName, oper, null)
        { }

        public override string ToString()
        {
            return $"{ColumnName} {this.Operator} NULL";
        }
    }
}
