/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Abstract base class for SQL format parts that contribute to building parameterized SQL statements.
    /// </summary>
    public abstract class FormatPart : IHasParameterInfos, IFormatPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormatPart"/> class with default parameter numbering starting at 1.
        /// </summary>
        public FormatPart()
        {
            this.parameters = new List<IParameterInfo>();
            this.StartNumber = 1;
        }

        /// <summary>
        /// Gets or sets the starting parameter number for this format part.
        /// </summary>
        public int? StartNumber { get; set; }

        /// <summary>
        /// Gets the next available parameter number after all current parameters.
        /// </summary>
        public int? NextNumber
        {
            get
            {
                return StartNumber + Parameters.Count();
            }
        }
        Func<string, string> _columnNameProvider;

        /// <summary>
        /// Gets or sets the function used to format column names in SQL output (defaults to bracket wrapping).
        /// </summary>
        public Func<string, string> ColumnNameFormatter
        {
            get
            {
                if (_columnNameProvider == null)
                {
                    _columnNameProvider = (c) =>
                    {
                        return string.Format("[{0}]", c);
                    };
                }

                return _columnNameProvider;
            }
            set
            {
                _columnNameProvider = value;
            }
        }
        /// <summary>
        /// Adds the specified IParameterInfo
        /// </summary>
        /// <param name="parameter"></param>
        public void AddParameter(IParameterInfo parameter)
        {
            this.parameters.Add(parameter);
        }

        /// <summary>
        /// Parses this format part into its SQL string representation.
        /// </summary>
        /// <returns>The SQL string for this format part.</returns>
        public abstract string Parse();

        List<IParameterInfo> parameters;
        #region IHasParameterInfos Members

        /// <summary>
        /// Gets or sets the array of parameter information objects for this format part.
        /// </summary>
        public IParameterInfo[] Parameters
        {
            get
            {
                return parameters.ToArray();
            }
            set
            {
                this.parameters = new List<IParameterInfo>();
                this.parameters.AddRange(value);
            }
        }

        #endregion
    }
}
