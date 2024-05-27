using Bam.Data;

namespace Bam.Data
{
    public class AndFormat : SetFormat
    {
        public AndFormat() { }

        public AndFormat(IQueryFilter filter)
        {
            foreach (IParameterInfo param in filter.Parameters)
            {
                AddParameter(param);
            }
            Filter = filter;
        }

        private IQueryFilter Filter;

        public override string Parse()
        {
            AssignNumbers();
            SetColumnNameFormatter();
            SetParameterPrefixes();
            string value = string.Empty;
            if (Filter != null)
            {
                value = string.Format("AND {0} ", Filter.Parse(StartNumber));
            }
            else
            {
                if (Parameters[0] != null)
                {
                    value = string.Format("AND {0} ", Parameters[0].ToString());
                }
            }

            return value;
        }
    }
}