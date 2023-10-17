/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bam.Net.Data
{
    public class StartsWithComparison: Comparison
    {
        public StartsWithComparison(string columnName, object value, int? number = null)
            : base(columnName, "LIKE", "{0}%".Format(value), number)
        { }
    }
}
