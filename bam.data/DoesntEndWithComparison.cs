/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bam.Data
{
    public class DoesntEndWithComparison : Comparison
    {
        public DoesntEndWithComparison(string columnName, object value, int? num = null)
            : base(columnName, "NOT LIKE", "%{0}".Format(value), num)
        { }
    }
}
