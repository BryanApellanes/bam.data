namespace Bam.Data
{
    /// <summary>
    /// A typed query filter that supports operator overloading for building SQL WHERE clauses with type-safe column references.
    /// </summary>
    /// <typeparam name="C">The column/filter token type that defines available columns.</typeparam>
    public class QueryFilter<C> : QueryFilter, IQueryFilter<C> where C : IFilterToken, new()
    {
        /// <summary>
        /// Initializes a new empty QueryFilter.
        /// </summary>
        public QueryFilter() : base()
        {

        }

        /// <summary>
        /// Initializes a new QueryFilter from an existing filter token.
        /// </summary>
        /// <param name="filter">The filter token to initialize from.</param>
        public QueryFilter(IFilterToken filter)
            : base(filter)
        {
        }

        /// <summary>
        /// Initializes a new QueryFilter for the specified column name.
        /// </summary>
        /// <param name="columnName">The column name for this filter.</param>
        public QueryFilter(string columnName)
            : base(columnName)
        {
        }

        /// <summary>
        /// Adds a filter token to this query filter.
        /// </summary>
        /// <param name="c">The filter token to add.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
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

        /// <summary>
        /// Adds a LIKE comparison that matches values starting with the specified value.
        /// </summary>
        /// <param name="value">The prefix to match.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public new QueryFilter<C> StartsWith(object value)
        {
            this.Add(new StartsWithComparison(this.ColumnName, value));
            return this;
        }

        /// <summary>
        /// Adds a NOT LIKE comparison that excludes values starting with the specified value.
        /// </summary>
        /// <param name="value">The prefix to exclude.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public new QueryFilter<C> DoesntStartWith(object value)
        {
            this.Add(new DoesntStartWithComparison(this.ColumnName, value));
            return this;
        }

        /// <summary>
        /// Adds a NOT LIKE comparison that excludes values ending with the specified value.
        /// </summary>
        /// <param name="value">The suffix to exclude.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public QueryFilter<C> DoesntEndWith(object value)
        {
            this.Add(new DoesntEndWithComparison(this.ColumnName, value));
            return this;
        }

        /// <summary>
        /// Adds a NOT LIKE comparison that excludes values containing the specified substring.
        /// </summary>
        /// <param name="value">The substring to exclude.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public new QueryFilter<C> DoesntContain(object value)
        {
            this.Add(new DoesntContainComparison(this.ColumnName, value));
            return this;
        }

        /// <summary>
        /// Adds a LIKE comparison that matches values ending with the specified value.
        /// </summary>
        /// <param name="value">The suffix to match.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public new QueryFilter<C> EndsWith(object value)
        {
            this.Add(new EndsWithComparison(this.ColumnName, value));
            return this;
        }

        /// <summary>
        /// Adds a LIKE comparison that matches values containing the specified substring.
        /// </summary>
        /// <param name="value">The substring to match.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public new QueryFilter<C> Contains(object value)
        {
            this.Add(new ContainsComparison(this.ColumnName, value));
            return this;
        }

        /// <summary>
        /// Adds an equality comparison for this column and the specified value.
        /// </summary>
        /// <param name="value">The value to compare for equality.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public override QueryFilter IsEqualTo(object value)
        {
            this.Add(new Comparison(ColumnName, "=", Query.Value(value).GetValue()));
            return this;
        }

        /// <summary>
        /// Adds an inequality comparison for this column and the specified value.
        /// </summary>
        /// <param name="value">The value to compare for inequality.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
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

        /// <summary>
        /// Adds an IN comparison with the specified unsigned long values.
        /// </summary>
        /// <param name="values">The unsigned long values to match against.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public QueryFilter<C> In(ulong[] values)
        {
            if (values != null && values.Length > 0)
            {
                Add(new InComparison(ColumnName, values));
            }
            return this;
        }

        /// <summary>
        /// Adds an IN comparison with the specified long values.
        /// </summary>
        /// <param name="values">The long values to match against.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
        public new QueryFilter<C> In(long[] values)
        {
            if (values != null && values.Length > 0)
            {
                Add(new InComparison(ColumnName, values));
            }
            return this;
        }

        /// <summary>
        /// Adds an IN comparison with the specified string values.
        /// </summary>
        /// <param name="values">The string values to match against.</param>
        /// <returns>This QueryFilter instance for method chaining.</returns>
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
