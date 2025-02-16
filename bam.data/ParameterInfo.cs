namespace Bam.Data
{
    public class ParameterInfo : IParameterInfo
    {
        public string ColumnName
        {
            get; set;
        }

        public Func<string, string> ColumnNameFormatter
        {
            get; set;
        }

        public int? Number
        {
            get; set;
        }

        public string Operator
        {
            get; set;
        }

        public string ParameterPrefix
        {
            get; set;
        }

        public object Value
        {
            get; set;
        }

        public int? SetNumber(int? value)
        {
            Number = value;
            return ++value;
        }
    }
}
