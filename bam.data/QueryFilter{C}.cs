using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Data
{

    public class QueryFilter<C> : QueryFilter, IQueryFilter<C> where C : IFilterToken, new()
    {
        public QueryFilter() : base()
        {

        }

        public QueryFilter(IFilterToken filter)
            : base(filter)
        {
        }

        public QueryFilter(string columnName)
            : base(columnName)
        {
        }

        public new QueryFilter<C> Add(IFilterToken c)
        {
            this._filters.Add(c);
            return this;
        }

        internal QueryValue ToQueryValue(ulong value)
        {
            QueryFilter keyColumnFilter = this.Property<QueryFilter>("KeyColumn");
            if ((keyColumnFilter?.ColumnName?.Equals(ColumnName)).Value)
            {
                return new DaoId(value, this) { IdentifierName = keyColumnFilter.ColumnName };
            }

            return new QueryValue(value, this);
        }

        internal new QueryFilter<C> AddRange(IEnumerable<IFilterToken> filters)
        {
            this._filters.AddRange(filters);
            return this;
        }

        internal QueryFilter<C> AddRange(QueryFilter<C> builder)
        {
            this._filters.AddRange(builder.Filters);
            return this;
        }

        public new QueryFilter<C> StartsWith(object value)
        {
            this.Add(new StartsWithComparison(this.ColumnName, value));
            return this;
        }

        public new QueryFilter<C> DoesntStartWith(object value)
        {
            this.Add(new DoesntStartWithComparison(this.ColumnName, value));
            return this;
        }

        public QueryFilter<C> DoesntEndWith(object value)
        {
            this.Add(new DoesntEndWithComparison(this.ColumnName, value));
            return this;
        }

        public new QueryFilter<C> DoesntContain(object value)
        {
            this.Add(new DoesntContainComparison(this.ColumnName, value));
            return this;
        }

        public new QueryFilter<C> EndsWith(object value)
        {
            this.Add(new EndsWithComparison(this.ColumnName, value));
            return this;
        }

        public new QueryFilter<C> Contains(object value)
        {
            this.Add(new ContainsComparison(this.ColumnName, value));
            return this;
        }

        public override QueryFilter IsEqualTo(object value)
        {
            this.Add(new Comparison(ColumnName, "=", Query.Value(value).GetValue()));
            return this;
        }

        public override QueryFilter IsNotEqualTo(object value)
        {
            this.Add(new Comparison(ColumnName, "<>", Query.Value(value).GetValue()));
            return this;
        }

        /// <summary>
        /// Adds an InComparison only if the specified object array is not empty
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public QueryFilter<C> InIfNotEmpty(object[] values)
        {
            if (values != null && values.Length > 0)
            {
                return In(values);
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// Adds an InComparison if the specified object array is not null and is not empty.
        /// </summary>
        public new QueryFilter<C> In(params object[] values)
        {
            if (values != null && values.Length > 0)
            {
                Add(new InComparison(ColumnName, values));
            }
            return this;
        }

        public QueryFilter<C> In(ulong[] values)
        {
            if (values != null && values.Length > 0)
            {
                Add(new InComparison(ColumnName, values));
            }
            return this;
        }

        public new QueryFilter<C> In(long[] values)
        {
            if (values != null && values.Length > 0)
            {
                Add(new InComparison(ColumnName, values));
            }
            return this;
        }

        public new QueryFilter<C> In(string[] values)
        {
            if (values != null && values.Length > 0)
            {
                Add(new InComparison(ColumnName, values));
            }
            return this;
        }

        public QueryFilter<C> And(QueryFilter<C> c)
        {
            return Add(new LiteralFilterToken(" AND "))
                .AddRange(c);
        }

        public QueryFilter<C> Or(QueryFilter<C> c)
        {
            return Add(new LiteralFilterToken(" OR "))
                .AddRange(c);
        }

        public static QueryFilter<C> operator &(QueryFilter<C> one, QueryFilter<C> two)
        {
            return ParenConcat(one, " AND ", two);
        }

        public static QueryFilter<C> operator |(QueryFilter<C> one, QueryFilter<C> two)
        {
            return ParenConcat(one, " OR ", two);
        }

        public static QueryFilter<C> operator ==(QueryFilter<C> c, DBNull value)
        {
            c.Add(new NullComparison(c.ColumnName, "IS"));
            return c;
        }

        public static QueryFilter<C> operator !=(QueryFilter<C> c, DBNull value)
        {
            c.Add(new NullComparison(c.ColumnName, "IS NOT"));
            return c;
        }

        public static QueryFilter<C> operator ==(QueryFilter<C> c, DaoId daoId)
        {
            if (c.ColumnName.Equals(daoId.IdentifierName))
            {
                c.Add(new Comparison(c.ColumnName, "=", daoId.GetRawValue()));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "=", daoId.GetValue(true)));
            }
            return c;
        }

        public static QueryFilter<C> operator !=(QueryFilter<C> c, DaoId daoId)
        {
            if (c.ColumnName.Equals(daoId.IdentifierName))
            {
                c.Add(new Comparison(c.ColumnName, "<>", daoId.GetRawValue()));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "<>", daoId.GetValue(true)));
            }

            return c;
        }

        public static QueryFilter<C> operator !=(QueryFilter<C> c, ulong value)
        {
            Comparison comp = new Comparison(c.ColumnName, "<>", c.ToQueryValue(value).GetValue());
            c.Add(comp);
            return c;
        }

        public static QueryFilter<C> operator ==(QueryFilter<C> c, ulong value)
        {
            Comparison comp = new Comparison(c.ColumnName, "=", c.ToQueryValue(value).GetValue());
            c.Add(comp);
            return c;
        }

        public static QueryFilter<C> operator ==(QueryFilter<C> c, int value)
        {
            c.Add(new Comparison(c.ColumnName, "=", value));
            return c;
        }

        public static QueryFilter<C> operator !=(QueryFilter<C> c, int value)
        {
            c.Add(new Comparison(c.ColumnName, "<>", value));
            return c;
        }

        public static QueryFilter<C> operator <(QueryFilter<C> c, int value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;
        }

        public static QueryFilter<C> operator >(QueryFilter<C> c, int value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter<C> operator <=(QueryFilter<C> c, int value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter<C> operator >=(QueryFilter<C> c, int value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }

        public static QueryFilter<C> operator ==(QueryFilter<C> c, uint value)
        {
            c.Add(new Comparison(c.ColumnName, "=", value));
            return c;
        }

        public static QueryFilter<C> operator !=(QueryFilter<C> c, uint value)
        {
            c.Add(new Comparison(c.ColumnName, "<>", value));
            return c;
        }

        public static QueryFilter<C> operator <(QueryFilter<C> c, uint value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;
        }

        public static QueryFilter<C> operator >(QueryFilter<C> c, uint value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter<C> operator <=(QueryFilter<C> c, uint value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter<C> operator >=(QueryFilter<C> c, uint value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }


        public static QueryFilter<C> operator <(QueryFilter<C> c, ulong value)
        {
            c.Add(new Comparison(c.ColumnName, "<", Dao.MapUlongToLong(value)));
            return c;
        }

        public static QueryFilter<C> operator >(QueryFilter<C> c, ulong value)
        {
            c.Add(new Comparison(c.ColumnName, ">", Dao.MapUlongToLong(value)));
            return c;
        }

        public static QueryFilter<C> operator <=(QueryFilter<C> c, ulong value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", Dao.MapUlongToLong(value)));
            return c;
        }

        public static QueryFilter<C> operator >=(QueryFilter<C> c, ulong value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", Dao.MapUlongToLong(value)));
            return c;
        }

        public static QueryFilter<C> operator <(QueryFilter<C> c, object value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;
        }

        public static QueryFilter<C> operator >(QueryFilter<C> c, object value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter<C> operator <=(QueryFilter<C> c, object value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter<C> operator >=(QueryFilter<C> c, object value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }

        public static QueryFilter<C> operator ==(QueryFilter<C> c, long value)
        {
            c.Add(new Comparison(c.ColumnName, "=", value));
            return c;
        }

        public static QueryFilter<C> operator !=(QueryFilter<C> c, long value)
        {
            c.Add(new Comparison(c.ColumnName, "<>", value));
            return c;
        }

        public static QueryFilter<C> operator <(QueryFilter<C> c, long value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;
        }

        public static QueryFilter<C> operator >(QueryFilter<C> c, long value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter<C> operator <=(QueryFilter<C> c, long value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter<C> operator >=(QueryFilter<C> c, long value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }

        public static QueryFilter<C> operator ==(QueryFilter<C> c, decimal value)
        {
            c.Add(new Comparison(c.ColumnName, "=", value));
            return c;
        }

        public static QueryFilter<C> operator !=(QueryFilter<C> c, decimal value)
        {
            c.Add(new Comparison(c.ColumnName, "<>", value));
            return c;
        }

        public static QueryFilter<C> operator <(QueryFilter<C> c, decimal value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;
        }

        public static QueryFilter<C> operator >(QueryFilter<C> c, decimal value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter<C> operator <=(QueryFilter<C> c, decimal value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter<C> operator >=(QueryFilter<C> c, decimal value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }

        public static QueryFilter<C> operator ==(QueryFilter<C> c, int? value)
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

        public static QueryFilter<C> operator !=(QueryFilter<C> c, int? value)
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

        public static QueryFilter<C> operator <(QueryFilter<C> c, int? value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;
        }

        public static QueryFilter<C> operator >(QueryFilter<C> c, int? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter<C> operator <=(QueryFilter<C> c, int? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter<C> operator >=(QueryFilter<C> c, int? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }

        public static QueryFilter<C> operator ==(QueryFilter<C> c, uint? value)
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

        public static QueryFilter<C> operator !=(QueryFilter<C> c, uint? value)
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

        public static QueryFilter<C> operator <(QueryFilter<C> c, uint? value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;
        }

        public static QueryFilter<C> operator >(QueryFilter<C> c, uint? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter<C> operator <=(QueryFilter<C> c, uint? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter<C> operator >=(QueryFilter<C> c, uint? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }

        public static QueryFilter<C> operator ==(QueryFilter<C> c, ulong? value)
        {
            if (value == null)
            {
                c.Add(new NullComparison(c.ColumnName, "IS"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "=", c.ToQueryValue(value.Value).GetValue()));
            }
            return c;
        }

        public static QueryFilter<C> operator !=(QueryFilter<C> c, ulong? value)
        {
            if (value == null)
            {
                c.Add(new NullComparison(c.ColumnName, "IS NOT"));
            }
            else
            {
                c.Add(new Comparison(c.ColumnName, "<>", c.ToQueryValue(value.Value).GetValue()));
            }
            return c;
        }

        public static QueryFilter<C> operator <(QueryFilter<C> c, ulong? value)
        {
            c.Add(new Comparison(c.ColumnName, "<", c.ToQueryValue(value.Value).GetValue()));
            return c;
        }

        public static QueryFilter<C> operator >(QueryFilter<C> c, ulong? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", c.ToQueryValue(value.Value).GetValue()));
            return c;
        }

        public static QueryFilter<C> operator <=(QueryFilter<C> c, ulong? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", c.ToQueryValue(value.Value).GetValue()));
            return c;
        }

        public static QueryFilter<C> operator >=(QueryFilter<C> c, ulong? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", c.ToQueryValue(value.Value).GetValue()));
            return c;
        }

        public static QueryFilter<C> operator ==(QueryFilter<C> c, decimal? value)
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

        public static QueryFilter<C> operator !=(QueryFilter<C> c, decimal? value)
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

        public static QueryFilter<C> operator <(QueryFilter<C> c, decimal? value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;
        }

        public static QueryFilter<C> operator >(QueryFilter<C> c, decimal? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter<C> operator <=(QueryFilter<C> c, decimal? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter<C> operator >=(QueryFilter<C> c, decimal? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }

        public static QueryFilter<C> operator ==(QueryFilter<C> c, string value)
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

        public static QueryFilter<C> operator !=(QueryFilter<C> c, string value)
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

        public static QueryFilter<C> operator <(QueryFilter<C> c, string value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;
        }

        public static QueryFilter<C> operator >(QueryFilter<C> c, string value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter<C> operator <=(QueryFilter<C> c, string value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter<C> operator >=(QueryFilter<C> c, string value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }

        public static QueryFilter<C> operator ==(QueryFilter<C> c, DateTime value)
        {
            c.Add(new Comparison(c.ColumnName, "=", value));
            return c;
        }

        public static QueryFilter<C> operator !=(QueryFilter<C> c, DateTime value)
        {
            c.Add(new Comparison(c.ColumnName, "<>", value));
            return c;
        }

        public static QueryFilter<C> operator <(QueryFilter<C> c, DateTime value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;
        }

        public static QueryFilter<C> operator >(QueryFilter<C> c, DateTime value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter<C> operator <=(QueryFilter<C> c, DateTime value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter<C> operator >=(QueryFilter<C> c, DateTime value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }

        public static QueryFilter<C> operator ==(QueryFilter<C> c, DateTime? value)
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

        public static QueryFilter<C> operator !=(QueryFilter<C> c, DateTime? value)
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

        public static QueryFilter<C> operator <(QueryFilter<C> c, DateTime? value)
        {
            c.Add(new Comparison(c.ColumnName, "<", value));
            return c;
        }

        public static QueryFilter<C> operator >(QueryFilter<C> c, DateTime? value)
        {
            c.Add(new Comparison(c.ColumnName, ">", value));
            return c;
        }

        public static QueryFilter<C> operator <=(QueryFilter<C> c, DateTime? value)
        {
            c.Add(new Comparison(c.ColumnName, "<=", value));
            return c;
        }

        public static QueryFilter<C> operator >=(QueryFilter<C> c, DateTime? value)
        {
            c.Add(new Comparison(c.ColumnName, ">=", value));
            return c;
        }

        public static bool operator true(QueryFilter<C> e)
        {
            return false;
        }

        public static bool operator false(QueryFilter<C> e)
        {
            return false;
        }

        private static QueryFilter<C> ParenConcat(QueryFilter<C> one, string middle, QueryFilter<C> two)
        {
            QueryFilter<C> newBuilder = new QueryFilter<C>();
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
                if (obj is QueryFilter<C> queryFilter)
                {
                    return queryFilter.Parse().Equals(this.Parse());
                }
                else
                {
                    return base.Equals(obj);
                }
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return this.Parse().GetHashCode();
        }
    }

}
