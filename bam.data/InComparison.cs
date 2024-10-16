/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Bam;
using Bam.Data;
//using Bam.FileExt;
//using Bam.FileExt.Js;

namespace Bam.Data
{
    public class InComparison: Comparison
    {
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

            public object Value
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
        
        public object[] Values { get; set; }

        // the number of the parameter names (@P1)
        int[] numbers;
        public override int? SetNumber(int? value)
        {
            if (Values.Length > 0)
            {
                numbers = new int[Values.Length];
                for (int i = 0; i < Values.Length; i++)
                {
                    numbers[i] = value.Value;
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
