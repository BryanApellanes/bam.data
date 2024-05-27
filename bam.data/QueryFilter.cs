/*
	Copyright © Bryan Apellanes 2015  
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Bam.Data
{
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

        public bool IsEmpty => string.IsNullOrWhiteSpace(ColumnName) && this._filters.Count == 0;

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

        public static QueryFilter Where(string columnName)
        {
            return Query.Where(columnName);
        }
        
        protected internal string ColumnName { get; set; }

        public IEnumerable<IFilterToken> Filters => this._filters;
        IEnumerable<IParameterInfo> _parameters;
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

        public QueryFilter StartsWith(object value)
        {
            this.Add(new StartsWithComparison(this.ColumnName, value));
            return this;
        }

        public QueryFilter DoesntStartWith(object value)
        {
            this.Add(new DoesntStartWithComparison(this.ColumnName, value));
            return this;
        }

        public QueryFilter DoesntContain(object value)
        {
            this.Add(new DoesntContainComparison(this.ColumnName, value));
            return this;
        }
        
        public QueryFilter EndsWith(object value)
        {
            this.Add(new EndsWithComparison(this.ColumnName, value));
            return this;
        }

        public QueryFilter Contains(object value)
        {
            this.Add(new ContainsComparison(this.ColumnName, value));
            return this;
        }
		public QueryFilter In(object[] values)
		{
			return In(values, "@");
		}

        internal QueryFilter In(object[] values, string parameterPrefix = "@")
        {
            this.Add(new InComparison(this.ColumnName, values, parameterPrefix));
            return this;
        }

		public QueryFilter In(long[] values)
		{
			return In(values, "@");
		}

        internal QueryFilter In(long[] values, string parameterPrefix)
        {
            this.Add(new InComparison(this.ColumnName, values, parameterPrefix));
            return this;
        }

		public QueryFilter In(string[] values)
		{
			return In(values, "@");
		}

        internal QueryFilter In(string[] values, string parameterPrefix)
        {
            this.Add(new InComparison(this.ColumnName, values, parameterPrefix));
            return this;
        }

        public QueryFilter IsNull()
        {
            return this.Add(new NullComparison(ColumnName, "IS"));
        }

        public QueryFilter IsNotNull()
        {
            return this.Add(new NullComparison(ColumnName, "IS NOT"));
        }
        
        public QueryFilter And(QueryFilter c)
        {
            return this.Add(new LiteralFilterToken(" AND "))
                .AddRange(c);
        }

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

        public virtual QueryFilter IsEqualTo(object value)
        {
            object compareTo = value;
            if (value is ulong ulongVal)
            {
                compareTo = Dao.MapUlongToLong(ulongVal);
            }

            return this == Query.Value(compareTo);
        }

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
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, int? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, int? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
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
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;
        }

        public static QueryFilter operator >(QueryFilter c, long? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, long? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, long? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, int? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
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
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, uint? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, uint? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, uint? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
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
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, ulong? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, ulong? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, ulong? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
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
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, decimal? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, decimal? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, decimal? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
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
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;   
        }

        public static QueryFilter operator >(QueryFilter c, DateTime? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter operator <=(QueryFilter c, DateTime? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter operator >=(QueryFilter c, DateTime? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
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

        public override bool Equals(object obj)
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
