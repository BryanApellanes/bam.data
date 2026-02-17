/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Formats SQL WHERE clauses from query filter parameters.
    /// </summary>
    public class WhereFormat: SetFormat
    {
        public WhereFormat() { }

        public WhereFormat(IQueryFilter filter)
        {
            foreach (IParameterInfo param in filter.Parameters)
            {
                this.AddParameter(param);
            }
            this.Filter = filter;
        }

        private IQueryFilter Filter = null!;

        public override string Parse()
        {
            AssignNumbers();
			SetColumnNameFormatter();
			SetParameterPrefixes();
            string value = string.Empty;
            if (Filter != null)
            {
                value = string.Format("WHERE {0} ", Filter.Parse(this.StartNumber));
            }
            else
            {
                if(this.Parameters[0] != null)
                {
                    value = string.Format("WHERE {0} ", this.Parameters[0].ToString());
                }
            }

            return value;
        }
    }
}
