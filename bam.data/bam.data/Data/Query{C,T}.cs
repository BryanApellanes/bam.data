using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net.Data
{
    /// <summary>
    /// Convenience class for queries
    /// </summary>
    /// <typeparam name="C">The type defining column names</typeparam>
    /// <typeparam name="T">The matching table type T for columns C</typeparam>
    public class Query<C, T> : IQuery<C, T>
        where C : IQueryFilter, IFilterToken, new()
        where T : IDao, new()
    {
        public Query() { }
        public Query(WhereDelegate<C> where, IOrderBy<C> orderBy = null, IDatabase db = null)
        {
            this.FilterDelegate = where;
            this.OrderBy = orderBy;
            this.Database = db;
        }

        public Query(Func<C, IQueryFilter<C>> where, IOrderBy<C> orderBy = null, IDatabase db = null)
        {
            this.FilterDelegate = where;
            this.OrderBy = orderBy;
            this.Database = db;
        }

        public Query(Delegate where, Database db = null)
        {
            this.FilterDelegate = where;
            this.Database = db;
        }

        Func<ColumnAttribute, string> _columnNameProvider;
        public Func<ColumnAttribute, string> ColumnNameProvider
        {
            get
            {
                if (_columnNameProvider == null)
                {
                    if (Database != null && Database.ColumnNameProvider != null)
                    {
                        _columnNameProvider = Database.ColumnNameProvider;
                    }
                    else
                    {
                        _columnNameProvider = (c) => $"[{c.Name}]";
                    }
                }

                return _columnNameProvider;
            }
            set => _columnNameProvider = value;
        }
        protected internal Delegate FilterDelegate
        {
            get;
            set;
        }

        protected internal IOrderBy<C> OrderBy
        {
            get;
            set;
        }

        public IDatabase Database
        {
            get;
            set;
        }

        public DataTable Where(WhereDelegate<C> where, IDatabase db = null)
        {
            return Where(where, null, db);
        }

        public DataTable Where(Func<C, IQueryFilter<C>> where, IOrderBy<C> orderBy = null, IDatabase db = null)
        {
            Establish(where, orderBy, db);
            ISqlStringBuilder sql = ToSqlStringBuilder(db);
            db = EstablishOrderAndDb(orderBy, db, sql);

            return GetDataTable(db, sql);
        }

        private DataTable Where(WhereDelegate<C> where, IOrderBy<C> orderBy = null, IDatabase db = null)
        {
            Establish(where, orderBy, db);
            ISqlStringBuilder sql = ToSqlStringBuilder(db);
            db = EstablishOrderAndDb(orderBy, db, sql);

            return GetDataTable(db, sql);
        }

        public DataTable Where(Qi.QiQuery query, IDatabase db = null)
        {
            SqlStringBuilder sql = new SqlStringBuilder();
            if (query.limit > 0)
            {
                sql.SelectTop(query.limit, query.table, query.columns).Where(query);
            }
            else
            {
                sql.Select(query.table, query.columns).Where(query);
            }

            if (db == null)
            {
                db = Db.For(query.cxName);
            }

            return GetDataTable(db, sql);
        }
        public DataTable GetDataTable()
        {
            return GetDataTable(Database);
        }

        public DataTable GetDataTable(IDatabase database)
        {
            ISqlStringBuilder sql = ToSqlStringBuilder(database);
            IDatabase db = EstablishOrderAndDb(OrderBy, database, sql);
            return GetDataTable(db, sql);
        }

        public ISqlStringBuilder ToSqlStringBuilder(IDatabase db)
        {
            if (FilterDelegate == null)
            {
                throw new ArgumentNullException("FilterDelegate was not set");
            }

            db = db ?? Db.For<T>();
            C columns = new C();
            IQueryFilter queryFilter = (IQueryFilter)FilterDelegate.DynamicInvoke(columns);
            // TODO: add FilterInspector operations here
            //  add FilterInspector to Database definition
            //  this can be useful to advise where indexes
            //  might be necessary or helpful
            return GetSqlStringBuilder(db).Where(queryFilter);
        }

        private IDatabase EstablishOrderAndDb(IOrderBy<C> orderBy, IDatabase db, ISqlStringBuilder sql)
        {
            if (orderBy != null)
            {
                OrderBy = orderBy;
                sql.OrderBy(orderBy);
            }

            if (db == null)
            {
                db = Db.For<T>();
                Database = db;
            }
            return db;
        }

        private void Establish(Delegate where, IOrderBy<C> orderBy = null, IDatabase db = null)
        {
            db = db ?? Db.For<T>();
            this.FilterDelegate = where;
            this.OrderBy = orderBy;
            this.Database = db;
        }

        private static DataTable GetDataTable(IDatabase db, ISqlStringBuilder sql)
        {
            db = db ?? Db.For<T>();
            IParameterBuilder parameterBuilder = db.ServiceProvider.Get<IParameterBuilder>();
            DbParameter[] parameters = parameterBuilder.GetParameters(sql);
            return db.GetDataTable(sql.ToString(), System.Data.CommandType.Text, parameters);
        }

        private ISqlStringBuilder GetSqlStringBuilder(IDatabase db)
        {
            db = db ?? Db.For<T>();
            ISqlStringBuilder sql = db.GetSqlStringBuilder();
            sql.Select(Dao.TableName(typeof(T)), db.ColumnNameListProvider(typeof(T), db));
            return sql;
        }
    }
}
