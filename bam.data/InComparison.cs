/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Collections;

//using Bam.FileExt;
//using Bam.FileExt.Js;

namespace Bam.Data
{
    /// <summary>
    /// A SQL IN comparison that checks if a column value matches any value in a given set (e.g., column IN (1, 2, 3)).
    /// </summary>
    public class InComparison: Comparison
    {
        /// <summary>
        /// Represents a single parameter within an IN clause.
        /// </summary>
        public class ParameterInfo: IParameterInfo
        {
            public ParameterInfo(int number, object value)
            {
                this.Number = number;
                this.Value = value;
				this.ColumnNameFormatter = (c) => c;
				this.ParameterPrefix = "@";
            }
            #region IParameterInfo Members
			public Func<string, string> ColumnNameFormatter { get; set; }
			public string ParameterPrefix { get; set; }
            public string ColumnName
            {
                get { return "P"; }
                set { }
            }
                        
            public int? Number
            {
                get;
                set;
            }

            public int? SetNumber(int? value)
            {
                throw new NotImplementedException();
            }

            public object? Value
            {
                get;
                set;
            }

            #endregion

            #region IFilterToken Members

            public string Operator
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            #endregion
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InComparison"/> class with an array of object values.
        /// </summary>
        /// <param name="columnName">The column name to filter on.</param>
        /// <param name="values">The set of values to match against.</param>
        /// <param name="parameterPrefix">The parameter prefix for the database provider.</param>
        public InComparison(string columnName, object[] values, string parameterPrefix = "@")
            :base(columnName, " IN ", values)
        {
            ThrowIfNull(values, "values");
            Args.ThrowIf<InvalidOperationException>(values.Length == 0, "At least one value must be specified");
            Values = new object[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                Values[i] = values[i];
            }
            ParameterPrefix = parameterPrefix;
			numbers = new int[] { };			
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InComparison"/> class with an array of long values.
        /// </summary>
        /// <param name="columnName">The column name to filter on.</param>
        /// <param name="values">The set of long values to match against.</param>
        /// <param name="parameterPrefix">The parameter prefix for the database provider.</param>
        public InComparison(string columnName, long[] values, string parameterPrefix = "@")
            :base(columnName, " IN ", values)
        {
            ThrowIfNull(values, "values");
            Args.ThrowIf<InvalidOperationException>(values.Length == 0, "At least one value must be specified for 'InComparison'");
            Values = new object[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                Values[i] = values[i];
            }
            ParameterPrefix = parameterPrefix;
            numbers = new int[] { };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InComparison"/> class with an array of unsigned long values.
        /// </summary>
        /// <param name="columnName">The column name to filter on.</param>
        /// <param name="values">The set of unsigned long values to match against.</param>
        /// <param name="parameterPrefix">The parameter prefix for the database provider.</param>
        public InComparison(string columnName, ulong[] values, string parameterPrefix = "@")
           : base(columnName, " IN ", values)
        {
            ThrowIfNull(values, "values");
            Args.ThrowIf<InvalidOperationException>(values.Length == 0, "At least one value must be specified for 'InComparison'");
            Values = new object[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                Values[i] = values[i];
            }
            ParameterPrefix = parameterPrefix;
            numbers = new int[] { };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InComparison"/> class with an array of string values.
        /// </summary>
        /// <param name="columnName">The column name to filter on.</param>
        /// <param name="values">The set of string values to match against.</param>
        /// <param name="parameterPrefix">The parameter prefix for the database provider.</param>
        public InComparison(string columnName, string[] values, string parameterPrefix = "@")
            : base(columnName, " IN ", values)
        {
            ThrowIfNull(values, "values");
            Args.ThrowIf<InvalidOperationException>(values.Length == 0, "At least one value must be specified for 'InComparison'");
            Values = new object[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                Values[i] = values[i];
            }
            ParameterPrefix = parameterPrefix;

            numbers = new int[] { };
        }
        
        /// <summary>
        /// Gets or sets the array of values for the IN clause.
        /// </summary>
        public object[] Values { get; set; }

        // the number of the parameter names (@P1)
        int[] numbers;

        /// <summary>
        /// Sets the parameter numbers for all values in the IN clause and returns the next available number.
        /// </summary>
        /// <param name="value">The starting parameter number.</param>
        /// <returns>The next available parameter number after all IN clause values.</returns>
        public override int? SetNumber(int? value)
        {
            if (Values.Length > 0)
            {
                numbers = new int[Values.Length];
                for (int i = 0; i < Values.Length; i++)
                {
                    numbers[i] = value!.Value;
                    value = numbers[i] + 1;
                }
                return value;
            }
            else
            {
                this.Number = value;
                return value;
            }
        }

        /// <summary>
        /// Gets the array of parameter info objects, one for each value in the IN clause.
        /// </summary>
        public ParameterInfo[] Parameters
        {
            get
            {
                List<ParameterInfo> results = new List<ParameterInfo>();

                for (int i = 0; i < Values.Length; i++)
                {
                    results.Add(new ParameterInfo(numbers[i], Values[i]));
                }

                return results.ToArray();
            }
        }

        public override string ToString()
        {
            List<string> paramNames = new List<string>();
            foreach(int i in numbers)
            {
                paramNames.Add($"{ParameterPrefix}P{i}");
            }

            return $"{ColumnNameFormatter(ColumnName)} IN ({paramNames.ToArray().ToDelimited(s => s)})";
        }

        private void ThrowIfNull(IEnumerable values, string name)
        {
            if (values == null)
            {
                throw new InvalidOperationException($"{name} can't be null or empty");
            }
        }
    }
}
