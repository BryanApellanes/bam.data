/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using Bam.Data;

namespace Bam.Data
{
    public partial class SqlStringBuilder : IHasFilters, ISqlStringBuilder
    {
        const string InsertFormat = "INSERT INTO {0} ";
        StringBuilder _stringBuilder;
        protected List<IParameterInfo> parameters;
        public static implicit operator string(SqlStringBuilder sqlStringBuilder)
        {
            return sqlStringBuilder._stringBuilder.ToString();
        }

        public SqlStringBuilder()
        {
            Reset();
            TableNameFormatter = t => string.Format("[{0}]", t);
            ColumnNameFormatter = c => string.Format("[{0}]", c);
            Executed += (s, d) =>
            {
                s.Reset();
            };
        }

        public SqlStringBuilder(string command)
            : this()
        {
            this._stringBuilder = new StringBuilder(command);
        }

        public virtual void Reset()
        {
            _stringBuilder = new StringBuilder();
            this.GoText = ";\r\n";
            this.parameters = new List<IParameterInfo>();
            NextNumber = 1;
        }

        public Func<string, string> TableNameFormatter
        {
            get;
            set;
        }

        public Func<string, string> ColumnNameFormatter
        {
            get;
            set;
        }

        public event SqlExecuteDelegate Executed;
        
        public DataTable ExecuteGetDataTable(IDatabase db)
        {
            DataTable table = GetDataTable(db);
            OnExecuted(db);
            return table;
        }

        public DataTable GetDataTable(IDatabase db)
        {
            if (!string.IsNullOrEmpty(this))
            {
                DbParameter[] dbParameters = db.ServiceProvider.Get<IParameterBuilder>().GetParameters(this);
                DataTable val = db.GetDataTable(this, CommandType.Text, dbParameters);
                return val;
            }
            else
            {
                return null;
            }
        }

        public bool TryExecute(IDatabase db)
        {
            return TryExecute(db, out Exception ignore);
        }

        /// <summary>
        /// Tries to execute the script by wrapping a call to Execute
        /// in a try catch; will return true if no exception occurred.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public bool TryExecute(IDatabase db, out Exception ex)
        {
            ex = null;
            try
            {
                Execute(db);
            }
            catch (Exception e)
            {
                ex = e;
            }

            return ex == null;
        }

        public virtual void Execute(IDatabase db)
        {
            if (!string.IsNullOrWhiteSpace(this))
            {
                db.ExecuteSql(this, CommandType.Text, db.ServiceProvider.Get<IParameterBuilder>().GetParameters(this));
                OnExecuted(db);
            }
        }

        public IEnumerable<T> ExecuteReader<T>(IDatabase db) where T : class, new()
        {
            if (!string.IsNullOrWhiteSpace(this))
            {
                return db.ExecuteReader<T>(this, (dr) => OnExecuted(db));
            }
            return new List<T>();
        }

        public virtual DataSet ExecuteGetDataSet(IDatabase db)
        {
            DataSet dataSet = GetDataSet(db);
            OnExecuted(db);
            return dataSet;
        }

        public virtual DataSet GetDataSet(IDatabase db, bool releaseConnection = true, DbConnection conn = null, DbTransaction tx = null)
        {
            if (conn == null)
            {
                conn = db.GetDbConnection();
            }
            if (db.ServiceProvider.TryGet(out IParameterBuilder parameterBuilder))
            {
                return db.GetDataSetFromSql(this, CommandType.Text, releaseConnection, conn, tx, parameterBuilder.GetParameters(this));
            }
            else
            {
                Args.Throw<InvalidOperationException>("Unable to get IParameterBuilder for the database with connection string ({0}), should you specify a Database instance of your own instead of depending on the default database initializer?  Be sure to call the appropriate Registrar method first", db.ConnectionString);
                return null;
            }
        }

        public IEnumerable<IFilterToken> Filters => this.parameters.ToArray();

        public string GoText { get; set; }

        public int? NextNumber { get; set; }

        /// <summary>
        /// Appends GoText to the end of the current string
        /// </summary>
        /// <returns></returns>
        public virtual ISqlStringBuilder Go()
        {
            string soFar = _stringBuilder.ToString();
            if (!string.IsNullOrEmpty(soFar) && !soFar.EndsWith(GoText))
            {
                _stringBuilder.Append(GoText);
            }
            return this;
        }

        public virtual ISqlStringBuilder Update(IDao instance)
        {
            return Update(instance.TableName(), instance.GetNewAssignValues());
        }

        public virtual ISqlStringBuilder Update<T>(T instance) where T : IDao
        {
            return Update(Dao.TableName(instance), instance.GetNewAssignValues());
        }

        public virtual ISqlStringBuilder Update(string tableName, dynamic valueAssignments)
        {
            IEnumerable<AssignValue> values = AssignValue.FromDynamic(valueAssignments, ColumnNameFormatter);
            return Update(tableName, values.ToArray());
        }

        public virtual ISqlStringBuilder Update(string tableName, Dictionary<string, object> valueAssignments)
        {
            IEnumerable<AssignValue> values = AssignValue.FromDictionary<string, object>(valueAssignments, ColumnNameFormatter);
            return Update(tableName, values.ToArray());
        }

        public virtual ISqlStringBuilder Update(string tableName, params AssignValue[] values)
        {
            _stringBuilder.AppendFormat("UPDATE {0} ", TableNameFormatter(tableName));
            SetFormat set = new SetFormat();
            foreach (AssignValue value in values)
            {
                set.AddAssignment(value);
            }

            set.StartNumber = NextNumber;
            _stringBuilder.Append(set.Parse());
            NextNumber = set.NextNumber;
            this.parameters.AddRange(set.Parameters);
            return this;
        }

        public virtual ISqlStringBuilder Insert<T>(T instance) where T : IDao, new()
        {
            return Insert(Dao.TableName(instance), instance.GetNewAssignValues());
        }

        public virtual ISqlStringBuilder Insert(IDao instance)
        {
            return Insert(Dao.TableName(instance.GetType()), instance.GetNewAssignValues());
        }

        public virtual ISqlStringBuilder Insert(string tableName, dynamic valueAssignments)
        {
            IEnumerable<AssignValue> values = AssignValue.FromDynamic(valueAssignments, ColumnNameFormatter);
            return Insert(tableName, values.ToArray());
        }
        public virtual ISqlStringBuilder Insert(string tableName, params AssignValue[] values)
        {
            return FormatInsert<InsertFormat>(tableName, values);
        }

        public virtual ISqlStringBuilder FormatInsert<T>(string tableName, params AssignValue[] values) where T : SetFormat, new()
        {
            _stringBuilder.AppendFormat(InsertFormat, TableNameFormatter(tableName));
            T insert = new T();
            foreach (AssignValue value in values)
            {
                insert.ColumnNameFormatter = ColumnNameFormatter;
                insert.AddAssignment(value);
            }

            insert.StartNumber = NextNumber;
            _stringBuilder.Append(insert.Parse());
            NextNumber = insert.NextNumber;
            parameters.AddRange(insert.Parameters);
            return this;
        }
        public bool SelectStar { get; set; }

        public virtual ISqlStringBuilder Select<T>() where T: IDao, new()
        {
            return Select(Dao.TableName(typeof(T)), SelectStar ? "*": ColumnAttribute.GetColumns(typeof(T)).ToDelimited(c => ColumnNameFormatter(c.Name)));
        }

        public virtual ISqlStringBuilder Select(Type daoType)
        {
            return Select(Dao.TableName(daoType), SelectStar ? "*" : ColumnAttribute.GetColumns(daoType).ToDelimited(c => ColumnNameFormatter(c.Name)));
        }

        public virtual ISqlStringBuilder Select(Type daoType, params string[] columns)
        {
            List<string> goodColumns = ColumnAttribute.GetColumns(daoType).Select(c => c.Name).ToList();
            foreach (string column in columns)
            {
                if (!SelectStar && !goodColumns.Contains(column))
                {
                    throw new InvalidOperationException(string.Format("Invalid column specified {0}", ColumnNameFormatter(column)));
                }
            }

            return Select(Dao.TableName(daoType),
                columns.ToDelimited(c => ColumnNameFormatter(c)));
        }
        

        public virtual ISqlStringBuilder Select<T>(params string[] columns)
        {
            List<string> goodColumns = ColumnAttribute.GetColumns(typeof(T)).Select(c => c.Name).ToList();
            foreach (string column in columns)
            {
                if (!SelectStar && !goodColumns.Contains(column))
                {
					throw new InvalidOperationException(string.Format("Invalid column specified {0}", ColumnNameFormatter(column)));
                }
            }

            return Select(Dao.TableName(typeof(T)),
                columns.ToDelimited(c => ColumnNameFormatter(c)));
        }

        /// <summary>
        /// Select Top [topCount].  Same as SelectTop
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topCount"></param>
        /// <returns></returns>
        public virtual ISqlStringBuilder Top<T>(int topCount) where T : IDao, new()
        {
            return SelectTop<T>(topCount);
        }

        /// <summary>
        /// Select Top [topCount].  Same as Top
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topCount"></param>
        /// <returns></returns>
        public virtual ISqlStringBuilder SelectTop<T>(int topCount) where T : IDao, new()
        {
            return SelectTop(topCount, Dao.TableName(typeof(T)), SelectStar ? "*" : ColumnAttribute.GetColumns(typeof(T)).ToDelimited(c => ColumnNameFormatter(c.Name)));
        }

        public virtual ISqlStringBuilder Select(string tableName, params string[] columnNames)
        {
            return SelectTop(-1, tableName, columnNames);
        }

        public virtual ISqlStringBuilder SelectTop(int topCount, string tableName, params string[] columnNames)
        {
            if (columnNames.Length == 0)
            {
                columnNames = new string[] { "*" };
            }

            string top = string.Empty;
            if (topCount > 0)
            {
                top = $" TOP {topCount} ";
            }
            string cols = columnNames.ToDelimited(s => $"{s}");
            _stringBuilder.AppendFormat("SELECT {0}{1} FROM {2} ", top, cols, TableNameFormatter(tableName));
            return this;
        }

        /// <summary>
        /// Select count from the table for the specified type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual ISqlStringBuilder Count<T>() where T : IDao, new()
        {
            return SelectCount<T>();
        }

        /// <summary>
        /// Select count from the table for the specified type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual ISqlStringBuilder SelectCount<T>() where T : IDao, new()
        {
			return SelectCount(Dao.TableName(typeof(T)));
        }

        /// <summary>
        /// Select count from the specified table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
		public virtual ISqlStringBuilder Count(string tableName)
		{
			return SelectCount(tableName);
		}

		public virtual ISqlStringBuilder SelectCount(string tableName)
		{
			_stringBuilder.AppendFormat("SELECT COUNT(*) FROM {0} ", TableNameFormatter(tableName));
			return this;
		}

        public virtual ISqlStringBuilder Delete(string tableName)
        {
			_stringBuilder.AppendFormat("DELETE FROM {0} ", TableNameFormatter(tableName));
            return this;
        }

        public ISqlStringBuilder Where<C>(Func<C, IQueryFilter> where) where C : IQueryFilter, IFilterToken, new()
        {
            C columns = new C();
            IQueryFilter filter = where(columns);
            Where(filter);            
            return this;
        }

        public ISqlStringBuilder OrderBy<C>(IOrderBy<C> orderBy) where C : IQueryFilter, IFilterToken, new()
        {
            return OrderBy(orderBy.Column.ToString(), orderBy.SortOrder);
        }

        public virtual ISqlStringBuilder Where(IQueryFilter filter)
        {
            WhereFormat where = new WhereFormat(filter)
            {
                StartNumber = NextNumber
            };
            _stringBuilder.Append(where.Parse());
            NextNumber = where.NextNumber;
            this.parameters.AddRange(where.Parameters);
            return this;
        }

        public virtual ISqlStringBuilder Where(string columnName, object value)
        {
            return Where(new AssignValue(columnName, value, ColumnNameFormatter));
        }

        public virtual ISqlStringBuilder Where(dynamic parameters)
        {
            IEnumerable<AssignValue> values = AssignValue.FromDynamic(parameters, ColumnNameFormatter);
            foreach(AssignValue value in values)
            {
                this.Where(value);
            }
            return this;
        }

        public virtual ISqlStringBuilder Where(AssignValue filter)
        {
            WhereFormat where = new WhereFormat() {ParameterPrefix = filter.ParameterPrefix};
            where.ColumnNameFormatter = filter.ColumnNameFormatter;
            where.StartNumber = NextNumber;
            where.AddAssignment(filter);
            _stringBuilder.Append(where.Parse());
            NextNumber = where.NextNumber;
            this.parameters.AddRange(where.Parameters);
            return this;
        }

        public virtual ISqlStringBuilder And(IQueryFilter filter)
        {
            AndFormat and = new AndFormat(filter)
            {
                StartNumber = NextNumber
            };
            _stringBuilder.Append(and.Parse());
            NextNumber = and.NextNumber;
            this.parameters.AddRange(and.Parameters);
            return this;
        }

        public virtual ISqlStringBuilder And(string columnName, object value)
        {
            return And(new AssignValue(columnName, value, ColumnNameFormatter));
        }

        public virtual ISqlStringBuilder And(dynamic parameters)
        {
            IEnumerable<AssignValue> values = AssignValue.FromDynamic(parameters, ColumnNameFormatter);
            foreach(AssignValue value in values)
            {
                this.Where(value);
            }
            return this;
        }
        
        public virtual ISqlStringBuilder And(AssignValue filter)
        {
            AndFormat where = new AndFormat() {ParameterPrefix = filter.ParameterPrefix};
            where.ColumnNameFormatter = filter.ColumnNameFormatter;
            where.StartNumber = NextNumber;
            where.AddAssignment(filter);
            _stringBuilder.Append(where.Parse());
            NextNumber = where.NextNumber;
            this.parameters.AddRange(where.Parameters);
            return this;
        }

        public virtual ISqlStringBuilder Id()
        {
            return Id("ID");
        }

        public virtual ISqlStringBuilder Id(string idAs)
        {
            _stringBuilder.AppendFormat("{0}SELECT @@IDENTITY AS {1}", this.GoText, idAs);
            return this;
        }

        public IParameterInfo[] Parameters
        {
            get
            {
                return parameters.ToArray();
            }
            set
            {
                this.parameters.Clear();
                this.parameters.AddRange(value);
            }
        }

        public virtual void CommentFormat(string format, params object[] args)
        {
            Comment(string.Format(format, args));
        }

        public virtual void Comment(string comment)
        {
            StringBuilder.AppendLine($"\r\n-- {comment.Replace("\r", "").Replace("\n", "").Trim()}");
        }

        public override string ToString()
        {
			return (string)this;
        }
		
		protected internal void OnExecuted(IDatabase db)
		{
            Executed?.Invoke(this, db);
        }
		
		public virtual ISqlStringBuilder OrderBy(string columnName, SortOrder order = SortOrder.Ascending)
		{
			_stringBuilder.AppendFormat("ORDER BY {0} {1}", ColumnNameFormatter(columnName), GetSortOrder(order));
			return this;
		}

		protected string GetSortOrder(SortOrder order)
		{
			switch (order)
			{
				case SortOrder.Unspecified:
					return "ASC";
				case SortOrder.Descending:
					return "DESC";
				case SortOrder.Ascending:
					return "ASC";
				default:
					return "ASC";
			}
		}

		/// <summary>
		/// Contains the Sql statement thus far for this
		/// SqlStringBuilder, same as StringBuilder
		/// </summary>
		protected StringBuilder Builder
		{
			get => _stringBuilder;
            set => _stringBuilder = value;
        }

		/// <summary>
		/// Contains the Sql statement thus far for this
		/// SqlStringBuilder, same as Builder
		/// </summary>
		protected StringBuilder StringBuilder
		{
			get => this._stringBuilder;
            set => _stringBuilder = value;
        }

    }
}
