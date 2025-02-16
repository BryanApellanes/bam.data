using System.Linq.Expressions;

namespace Bam.Data
{
    public class ExpressionTypeNotSupportedException: Exception
    {
        public ExpressionTypeNotSupportedException(ExpressionType expressionType) : 
            base($"Unsupported NodeType ({expressionType.ToString()}): for conditionals use QueryFilter.Or() and QueryFilter.And()")
        { }
    }
}
