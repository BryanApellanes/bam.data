/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Bam.Data
{
    /// <summary>
    /// Builds SQL WHERE clause filters using a fluent API and operator overloads for comparisons, AND/OR logic, and IN clauses.
    /// </summary>
    public class QueryFilter : IParameterInfoParser, IQueryFilter
    {
        protected readonly List<IFilterToken> _filters;
        public QueryFilter()
        {
            this._filters = new List<IFilterToken>();
        }

        public QueryFilter(IFilterToken filter)
            : this()
        {
            this._filters.Add(filter);
        }

        public QueryFilter(string columnName)
            : this()
        {
            this.ColumnName = columnName;
        }

        /// <summary>
        /// Gets a value indicating whether this filter has no column name and no filter tokens.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(ColumnName) && this._filters.Count == 0;

        /// <summary>
        /// Creates a QueryFilter from a dynamic object, generating equality comparisons for each property.
        /// </summary>
        /// <param name="query">The dynamic object whose properties define the filter.</param>
        /// <returns>A new QueryFilter with AND-joined equality comparisons.</returns>
        public static QueryFilter FromDynamic(dynamic query)
        {
            Type type = query.GetType();
            PropertyInfo[] properties = type.GetProperties();
            QueryFilter filter = new QueryFilter();
            bool first = true;
            foreach(PropertyInfo prop in properties)
            {
                QueryFilter next = Query.Where(prop.Name) == Query.Value(prop.GetValue(query));
                if (first) // trying to do filter == null will invoke implicit operator rather than doing an actual null comparison
                {
                    first = false;
                    filter = next;
                }
                else
                {
                    filter = filter.And(next);
                }
            }
            return filter;
        }

        /// <summary>
        /// Creates a new QueryFilter for a WHERE clause on the specified column.
        /// </summary>
        /// <param name="columnName">The column name to filter on.</param>
        /// <returns>A new QueryFilter instance.</returns>
        public static QueryFilter Where(string columnName)
        {
            return Query.Where(columnName);
        }
        
        protected internal string ColumnName { get; set; } = null!;

        /// <summary>
        /// Gets the collection of filter tokens that compose this filter.
        /// </summary>
        public IEnumerable<IFilterToken> Filters => this._filters;
        IEnumerable<IParameterInfo> _parameters = null!;
        public virtual IParameterInfo[] Parameters
        {
            get
            {
                List<IParameterInfo> temp = new List<IParameterInfo>();
                foreach (IFilterToken token in Filters)
                {
                    if (token is IParameterInfo parameter)
                    {
                        temp.Add(parameter);
                    }
                }

                return temp.ToArray();
            }
            set => _parameters = value;
        }

        /// <summary>
        /// Parse the query filter
        /// </summary>
        /// <returns></returns>
        public string Parse()
        {
            return Parse(1);
        }

        /// <summary>
        /// Parses the filter into a SQL string, assigning parameter numbers starting at the specified value.
        /// </summary>
        /// <param name="number">The starting parameter number.</param>
        /// <returns>The parsed SQL filter string.</returns>
        public string Parse(int? number)
        {
            StringBuilder builder = new StringBuilder();

            foreach (IFilterToken token in this.Filters)
            {
                if (token is IParameterInfo c)
                {
                    number = c.SetNumber(number);
                }
                builder.Append(token.ToString());
            }

            return builder.ToString();
        }

        /// <summary>
        /// Adds a filter token to this filter.
        /// </summary>
        /// <param name="c">The filter token to add.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public QueryFilter Add(IFilterToken c)
        {
            this._filters.Add(c);
            return this;
        }

        internal QueryFilter AddRange(IEnumerable<IFilterToken> filters)
        {
            this._filters.AddRange(filters);
            return this;
        }

        internal QueryFilter AddRange(QueryFilter builder)
        {
            this._filters.AddRange(builder.Filters);
            return this;
        }

        /// <summary>
        /// Adds a LIKE comparison that matches values starting with the specified value.
        /// </summary>
        /// <param name="value">The value prefix to match.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public QueryFilter StartsWith(object value)
        {
            this.Add(new StartsWithComparison(this.ColumnName, value));
            return this;
        }

        /// <summary>
        /// Adds a NOT LIKE comparison that excludes values starting with the specified value.
        /// </summary>
        /// <param name="value">The value prefix to exclude.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public QueryFilter DoesntStartWith(object value)
        {
            this.Add(new DoesntStartWithComparison(this.ColumnName, value));
            return this;
        }

        /// <summary>
        /// Adds a NOT LIKE comparison that excludes values containing the specified value.
        /// </summary>
        /// <param name="value">The value to exclude.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public QueryFilter DoesntContain(object value)
        {
            this.Add(new DoesntContainComparison(this.ColumnName, value));
            return this;
        }
        
        /// <summary>
        /// Adds a LIKE comparison that matches values ending with the specified value.
        /// </summary>
        /// <param name="value">The value suffix to match.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public QueryFilter EndsWith(object value)
        {
            this.Add(new EndsWithComparison(this.ColumnName, value));
            return this;
        }

        /// <summary>
        /// Adds a LIKE comparison that matches values containing the specified value.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public QueryFilter Contains(object value)
        {
            this.Add(new ContainsComparison(this.ColumnName, value));
            return this;
        }
		/// <summary>
		/// Adds an IN comparison that matches any of the specified values.
		/// </summary>
		/// <param name="values">The values to match against.</param>
		/// <returns>This QueryFilter instance for method chaining.</returns>
		public QueryFilter In(object[] values)
		{
			return In(values, "@");
		}

        internal QueryFilter In(object[] values, string parameterPrefix = "@")
        {
            this.Add(new InComparison(this.ColumnName, values, parameterPrefix));
            return this;
        }

		/// <summary>
		/// Adds an IN comparison that matches any of the specified long values.
		/// </summary>
		/// <param name="values">The long values to match against.</param>
		/// <returns>This QueryFilter instance for method chaining.</returns>
		public QueryFilter In(long[] values)
		{
			return In(values, "@");
		}

        internal QueryFilter In(long[] values, string parameterPrefix)
        {
            this.Add(new InComparison(this.ColumnName, values, parameterPrefix));
            return this;
        }

		/// <summary>
		/// Adds an IN comparison that matches any of the specified string values.
		/// </summary>
		/// <param name="values">The string values to match against.</param>
		/// <returns>This QueryFilter instance for method chaining.</returns>
		public QueryFilter In(string[] values)
		{
			return In(values, "@");
		}

        internal QueryFilter In(string[] values, string parameterPrefix)
        {
            this.Add(new InComparison(this.ColumnName, values, parameterPrefix));
            return this;
        }

        /// <summary>
        /// Adds an IS NULL comparison for this column.
        /// </summary>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public QueryFilter IsNull()
        {
            return this.Add(new NullComparison(ColumnName, "IS"));
        }

        /// <summary>
        /// Adds an IS NOT NULL comparison for this column.
        /// </summary>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public QueryFilter IsNotNull()
        {
            return this.Add(new NullComparison(ColumnName, "IS NOT"));
        }
        
        /// <summary>
        /// Combines this filter with another using AND logic.
        /// </summary>
        /// <param name="c">The filter to AND with.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public QueryFilter And(QueryFilter c)
        {
            return this.Add(new LiteralFilterToken(" AND "))
                .AddRange(c);
        }

        /// <summary>
        /// Combines this filter with another using OR logic.
        /// </summary>
        /// <param name="c">The filter to OR with.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public QueryFilter Or(QueryFilter c)
        {
            return this.Add(new LiteralFilterToken(" OR "))
                .AddRange(c);
        }
        
        public QueryFilter Or<T>(Expression<Func<T, bool>> expression)
        {
            DaoExpressionFilter expressionFilter = new DaoExpressionFilter();
            return Or(expressionFilter.Where<T>(expression));
        }
        
        public QueryFilter And<T>(Expression<Func<T, bool>> expression)
        {
            DaoExpressionFilter expressionFilter = new DaoExpressionFilter();
            return And(expressionFilter.Where<T>(expression));
        }

        /// <summary>
        /// Adds an equality comparison for this column with the specified value.
        /// </summary>
        /// <param name="value">The value to compare against.</param>
        /// <returns>A QueryFilter with the equality comparison.</returns>
        public virtual QueryFilter IsEqualTo(object value)
        {
            object compareTo = value;
            if (value is ulong ulongVal)
            {
                compareTo = Dao.MapUlongToLong(ulongVal);
            }

            return this == Query.Value(compareTo);
        }

        /// <summary>
        /// Adds an inequality comparison for this column with the specified value.
        /// </summary>
        /// <param name="value">The value to compare against.</param>
        /// <returns>A QueryFilter with the inequality comparison.</returns>
        public virtual QueryFilter IsNotEqualTo(object value)
        {
            object compareTo = value;
            if (value is ulong ulongVal)
            {
                compareTo = Dao.MapUlongToLong(ulongVal);
            }

            return this != Query.Value(compareTo);
        }
        
        public static QueryFilter operator &(QueryFilter one, QueryFilter two)
        {
            return ParenConcat(one, " AND ", two);
        }

        public static QueryFilter operator |(QueryFilter one, QueryFilter two)
        {
            return ParenConcat(one, " OR ", two);
        }

        public static QueryFilter operator ==(QueryFilter c, QueryValue value)
        {
            if(value.IsNull())
            {
                c.Add(new NullComparison(c.ColumnName, "IS"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "=", value.GetValue()));
            }
            return c;
        }

        public static QueryFilter operator !=(QueryFilter c, QueryValue value)
        {
            if(value.IsNull())
            {
                c.Add(new NullComparison(c.ColumnName, "IS NOT"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "<>", value.GetValue()));
            }
            return c;
        }
        
        public static QueryFilter operator ==(QueryFilter c, int value)
        {
            c.Add(new Comparison(c.ColumnName, "=", value));
            return c;
        }

        public static QueryFilter operator !=(QueryFilter c, int value)
        {
            c.Add(new Comparison(c.ColumnName, "<>", value));
            return c;
        }

        public static QueryFilter operator <(QueryFilter c, QueryValue value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value.GetValue()));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, QueryValue value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value.GetValue()));
            return c;
        }
        
        public static QueryFilter operator <=(QueryFilter c, QueryValue value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value.GetValue()));
            return c;   
        }

        public static QueryFilter operator >=(QueryFilter c, QueryValue value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value.GetValue()));
            return c;
        }
        
        public static QueryFilter operator <(QueryFilter c, int value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, int value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, int value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, int value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }
            
        public static QueryFilter operator ==(QueryFilter c, uint value)
        {
            c.Add(new Comparison(c.ColumnName, "=", value));
            return c;
        }

        public static QueryFilter operator !=(QueryFilter c, uint value)
        {
            c.Add(new Comparison(c.ColumnName, "<>", value));
            return c;
        }

        public static QueryFilter operator <(QueryFilter c, uint value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, uint value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, uint value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, uint value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }
        
        public static QueryFilter operator ==(QueryFilter c, bool value)
        {
            c.Add(new Comparison(c.ColumnName, "=", value));
            return c;
        }

        public static QueryFilter operator !=(QueryFilter c, bool value)
        {
            c.Add(new Comparison(c.ColumnName, "<>", value));
            return c;
        }
        
        public static QueryFilter operator ==(QueryFilter c, ulong value)
        {
            c.Add(new Comparison(c.ColumnName, "=", value));
            return c;
        }

        public static QueryFilter operator !=(QueryFilter c, ulong value)
        {
            c.Add(new Comparison(c.ColumnName, "<>", value));
            return c;
        }

        public static QueryFilter operator <(QueryFilter c, ulong value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, ulong value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, ulong value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, ulong value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }
            
        public static QueryFilter operator ==(QueryFilter c, long value)
        {
            c.Add(new Comparison(c.ColumnName, "=", value));
            return c;
        }

        public static QueryFilter operator !=(QueryFilter c, long value)
        {
            c.Add(new Comparison(c.ColumnName, "<>", value));
            return c;
        }

        public static QueryFilter operator <(QueryFilter c, long value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, long value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, long value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, long value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }
            
        public static QueryFilter operator ==(QueryFilter c, decimal value)
        {
            c.Add(new Comparison(c.ColumnName, "=", value));
            return c;
        }

        public static QueryFilter operator !=(QueryFilter c, decimal value)
        {
            c.Add(new Comparison(c.ColumnName, "<>", value));
            return c;
        }

        public static QueryFilter operator <(QueryFilter c, decimal value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, decimal value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, decimal value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, decimal value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }
            
        public static QueryFilter operator ==(QueryFilter c, int? value)
        {
            if(value == null)
            {
                c.Add(new NullComparison(c.ColumnName, "IS"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "=", value));
            }
            return c;
        }

        public static QueryFilter operator !=(QueryFilter c, int? value)
        {
            if(value == null)
            {
                c.Add(new NullComparison(c.ColumnName, "IS NOT"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "<>", value));
            }
            return c;
        }

        public static QueryFilter operator <(QueryFilter c, int? value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value!));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, int? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value!));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, int? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value!));
            return c;
        }

        public static QueryFilter operator ==(QueryFilter c, long? value)
        {
            if (value == null)
            {
                c.Add(new NullComparison(c.ColumnName, "IS"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "=", value));
            }
            return c;
        }

        public static QueryFilter operator !=(QueryFilter c, long? value)
        {
            if (value == null)
            {
                c.Add(new NullComparison(c.ColumnName, "IS NOT"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "<>", value));
            }
            return c;
        }

        public static QueryFilter operator <(QueryFilter c, long? value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value!));
            return c;
        }

        public static QueryFilter operator >(QueryFilter c, long? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value!));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, long? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value!));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, long? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value!));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, int? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value!));
            return c;
        }
            
        public static QueryFilter operator ==(QueryFilter c, uint? value)
        {
            if(value == null)
            {
                c.Add(new NullComparison(c.ColumnName, "IS"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "=", value));
            }
            return c;
        }

        public static QueryFilter operator !=(QueryFilter c, uint? value)
        {
            if(value == null)
            {
                c.Add(new NullComparison(c.ColumnName, "IS NOT"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "<>", value));
            }
            return c;
        }

        public static QueryFilter operator <(QueryFilter c, uint? value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value!));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, uint? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value!));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, uint? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value!));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, uint? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value!));
            return c;
        }
            
        public static QueryFilter operator ==(QueryFilter c, ulong? value)
        {
            if(value == null)
            {
                c.Add(new NullComparison(c.ColumnName, "IS"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "=", value));
            }
            return c;
        }

        public static QueryFilter operator !=(QueryFilter c, ulong? value)
        {
            if(value == null)
            {
                c.Add(new NullComparison(c.ColumnName, "IS NOT"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "<>", value));
            }
            return c;
        }

        public static QueryFilter operator <(QueryFilter c, ulong? value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value!));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, ulong? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value!));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, ulong? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value!));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, ulong? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value!));
            return c;
        }
            
        public static QueryFilter operator ==(QueryFilter c, decimal? value)
        {
            if(value == null)
            {
                c.Add(new NullComparison(c.ColumnName, "IS"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "=", value));
            }
            return c;
        }

        public static QueryFilter operator !=(QueryFilter c, decimal? value)
        {
            if(value == null)
            {
                c.Add(new NullComparison(c.ColumnName, "IS NOT"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "<>", value));
            }
            return c;
        }

        public static QueryFilter operator <(QueryFilter c, decimal? value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value!));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, decimal? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value!));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, decimal? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value!));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, decimal? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value!));
            return c;
        }
            
        public static QueryFilter operator ==(QueryFilter c, string value)
        {
            if(value == null)
            {
                c.Add(new NullComparison(c.ColumnName, "IS"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "=", value));
            }
            return c;
        }

        public static QueryFilter operator !=(QueryFilter c, string value)
        {
            if(value == null)
            {
                c.Add(new NullComparison(c.ColumnName, "IS NOT"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "<>", value));
            }
            return c;
        }

        public static QueryFilter operator <(QueryFilter c, string value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, string value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, string value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, string value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }
            
        public static QueryFilter operator ==(QueryFilter c, DateTime value)
        {
#pragma warning disable CS8073 // value type DateTime is never null
            if(value == null)
#pragma warning restore CS8073
            {
                c.Add(new NullComparison(c.ColumnName, "IS"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "=", value));
            }
            return c;
        }

        public static QueryFilter operator !=(QueryFilter c, DateTime value)
        {
            c.Add(new Comparison(c.ColumnName, "<>", value));
            return c;
        }

        public static QueryFilter operator <(QueryFilter c, DateTime value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, DateTime value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, DateTime value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, DateTime value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }
            
        public static QueryFilter operator ==(QueryFilter c, DateTime? value)
        {
            if(value == null)
            {
                c.Add(new NullComparison(c.ColumnName, "IS"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "=", value));
            }
            return c;
        }

        public static QueryFilter operator !=(QueryFilter c, DateTime? value)
        {
            if(value == null)
            {
                c.Add(new NullComparison(c.ColumnName, "IS NOT"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "<>", value));
            }
            return c;
        }

        public static QueryFilter operator <(QueryFilter c, DateTime? value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value!));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, DateTime? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value!));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, DateTime? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value!));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, DateTime? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value!));
            return c;
        }


        public static bool operator true(QueryFilter e)
        {
            return false;
        }

        public static bool operator false(QueryFilter e)
        {
            return false;
        }
        
        private static QueryFilter ParenConcat(QueryFilter one, string middle, QueryFilter two)
        {
            QueryFilter newBuilder = new QueryFilter();
            newBuilder.Add(new OpenParen())
                .AddRange(one)
                .Add(new CloseParen())
                .Add(new LiteralFilterToken(middle))
                .Add(new OpenParen())
                .AddRange(two)
                .Add(new CloseParen());
            return newBuilder;
        }

        public override bool Equals(object? obj)
        {
            if (obj != null)
            {
                try
                {
                    QueryFilter o = (QueryFilter)obj;
                    return o.Parse().Equals(this.Parse());
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Parse().GetHashCode();
        }
    }

}
