using System.Linq.Expressions;

namespace Bam.Data
{
    /// <summary>
    /// Exception thrown when an unsupported expression type is encountered in a Dao expression filter.
    /// </summary>
    public class ExpressionTypeNotSupportedException: Exception
    {
        public ExpressionTypeNotSupportedException(ExpressionType expressionType) : 
            base($"Unsupported NodeType ({expressionType.ToString()}): for conditionals use QueryFilter.Or() and QueryFilter.And()")
        { }
    }
}
