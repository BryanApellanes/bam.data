/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    public class InsertFormat: SetFormat
    {
        public override string Parse()
        {
            AssignNumbers();
			string columns = this.Parameters.ToArray().ToDelimited(p => ColumnNameFormatter(p.ColumnName));
            string values = this.Parameters.ToArray().ToDelimited(p => string.Format("{0}{1}{2}", ParameterPrefix, p.ColumnName, p.Number));
            return string.Format("({0}) VALUES ({1})",
                columns,
                values
            );

        }
    }
}
