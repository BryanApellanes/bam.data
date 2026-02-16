/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Provides a static convenience method for creating OrderBy instances.
    /// </summary>
    public static class Order
    {
        /// <summary>
        /// Creates an OrderBy clause for the specified column and sort order.
        /// </summary>
        /// <typeparam name="C">The query filter/column type.</typeparam>
        /// <param name="column">Function selecting the column to order by.</param>
        /// <param name="order">The sort order direction.</param>
        /// <returns>A new OrderBy instance.</returns>
        public static OrderBy<C> By<C>(Func<C, C> column, SortOrder order = SortOrder.Descending) where C: IQueryFilter, IFilterToken, new()
        {
            return new OrderBy<C>(column, order);
        }
    }

    /// <summary>
    /// Represents an ORDER BY clause for a typed query.
    /// </summary>
    /// <typeparam name="C">The query filter/column type.</typeparam>
    public class OrderBy<C>: IOrderBy<C> where C : IQueryFilter, IFilterToken, new()
    {
        /// <summary>
        /// Initializes a new OrderBy with the specified column selector and sort order.
        /// </summary>
        /// <param name="column">Function selecting the column to order by.</param>
        /// <param name="order">The sort order direction.</param>
        public OrderBy(Func<C, C> column, SortOrder order)
        {
            this.SortOrder = order;
            C c = new C();
            this.Column = column(c);
        }

        /// <summary>
        /// Gets the column to order by.
        /// </summary>
        public C Column { get; private set; }
        /// <summary>
        /// Gets the sort order direction.
        /// </summary>
        public SortOrder SortOrder { get; private set; }
    }
}
