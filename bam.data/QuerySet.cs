/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Bam.Data
{
    /// <summary>
    /// Allows
    /// one to specify a sequence of sql statements
    /// that result in the return of potentially
    /// multiple result sets.
    /// </summary>
    public class QuerySet: SqlStringBuilder, IQuerySet
    {
        List<IHasDataTable> _results;
        public QuerySet()
        {
            this._results = new List<IHasDataTable>();
            this.SubscribeToExecute();
        }
                
        public void Select<C, T>(Func<C, IQueryFilter> where)
            where C : IFilterToken, new()
            where T : IDao, new()
        {
            C columns = new C();
            IQueryFilter filter = where(columns);
            Select<C, T>(filter);
        }

        public void Select<C, T>(IQueryFilter filter)
            where C : IFilterToken, new()
            where T : IDao, new()
        {
            _results.Add(new SelectResult()); 
            base.Select<T>().Where(filter).Go();
        }

		protected List<IHasDataTable> ResultDataTables
		{
			get
			{
				return _results;
			}
		}

        public override ISqlStringBuilder Insert<T>(T instance)
        {
            return Insert((IDao)instance);
        }

        public override ISqlStringBuilder Insert(IDao instance)
        {
            _results.Add(new InsertResult(instance));
            base.Insert(instance).Id().Go();
            return this;
        }

        public override ISqlStringBuilder Select<T>()
        {
            _results.Add(new SelectResult());
            base.Select<T>();
            return this;
        }

        public override ISqlStringBuilder Select<T>(params string[] columns)
        {
            _results.Add(new SelectResult());
            base.Select<T>(columns);
            return this;
        }

        /// <summary>
        /// Same as SelectCount. Equivalent to (SELECT COUNT(*))
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override SqlStringBuilder Count<T>()
        {
            _results.Add(new CountResult());
            base.Count<T>();
            return this;
        }

        /// <summary>
        /// Same as Count. Equivalent to (SELECT COUNT(*))
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override SqlStringBuilder SelectCount<T>()
        {
            _results.Add(new CountResult());
            base.SelectCount<T>();
            return this;
        }

        public override SqlStringBuilder Top<T>(int count)
        {
            return SelectTop<T>(count);
        }

        public override SqlStringBuilder SelectTop<T>(int count)
        {
            _results.Add(new SelectResult());
            base.SelectTop<T>(count);
            return this;
        }

        public IQuerySetResults Results => new QuerySetResults(_results, Database);

        public IDatabase Database { get; set; }

        public override void Execute(IDatabase db)
        {
            this.DataSet = base.GetDataSet(db);
            this.OnExecuted(db);
        }

        protected internal DataSet DataSet
        {
            get;
            set;
        }
        
        /// <summary>
        /// This is what's responsible for setting the ID 
        /// </summary>
        protected internal virtual void SubscribeToExecute()
        {
            this.Executed += (s, d) =>
            {
                if (_results.Count > 0)
                {
                    for (int i = 0; i < DataSet.Tables.Count; i++)
                    {
                        DataTable table = DataSet.Tables[i];
                        IHasDataTable result = _results[i];
                        result.SetDataTable(table);
                    }
                }
                this.Reset();
            };
        }
    }
}
