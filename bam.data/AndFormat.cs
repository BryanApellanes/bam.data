namespace Bam.Data
{
    /// <summary>
    /// A SQL format part that generates an AND clause from a query filter or parameter.
    /// </summary>
    public class AndFormat : SetFormat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AndFormat"/> class.
        /// </summary>
        public AndFormat() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AndFormat"/> class with the specified query filter.
        /// </summary>
        /// <param name="filter">The query filter to use in the AND clause.</param>
        public AndFormat(IQueryFilter filter)
        {
            foreach (IParameterInfo param in filter.Parameters)
            {
                AddParameter(param);
            }
            Filter = filter;
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
