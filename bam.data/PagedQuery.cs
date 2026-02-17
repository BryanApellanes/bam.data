/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Data;

namespace Bam.Data
{
	/// <summary>
	/// Provides paged query execution over a Dao type, loading IDs first then fetching pages of results.
	/// </summary>
	/// <typeparam name="C">The query filter/column type.</typeparam>
	/// <typeparam name="T">The Dao type to query.</typeparam>
	public class PagedQuery<C, T>
		where C : QueryFilter, IFilterToken, new()
		where T : IDao, new()
	{
		/// <summary>
		/// Initializes a new PagedQuery with the specified order column, query, and optional database.
		/// </summary>
		/// <param name="orderByColumn">The column to order results by.</param>
		/// <param name="query">The base query to page over.</param>
		/// <param name="db">Optional database; defaults to the database for type T.</param>
		public PagedQuery(C orderByColumn, Query<C, T> query, IDatabase db = null!)
		{
			this.Database = db;
			this.OrderByColumn = orderByColumn;
			this.SortOrder = SortOrder.Ascending;
			this.Query = query;
			this.PageSize = 5;
			this.CurrentPage = -1;
		}

		/// <summary>
		/// Gets or sets the column used for ordering results.
		/// </summary>
		public C OrderByColumn { get; set; }
		/// <summary>
		/// Gets or sets the sort order direction.
		/// </summary>
		public SortOrder SortOrder { get; set; }
		/// <summary>
		/// Gets the total number of pages.
		/// </summary>
		public int PageCount
		{
			get
			{
				return IdBook.PageCount;
			}
		}

		/// <summary>
		/// The number of records per page, default is 5
		/// </summary>
		public int PageSize { get; set; }
		/// <summary>
		/// Gets or sets the current zero-based page index.
		/// </summary>
		public int CurrentPage { get; set; }
		/// <summary>
		/// Gets or sets the underlying query.
		/// </summary>
		public Query<C, T> Query { get; set; }
		IDatabase _database = null!;
		/// <summary>
		/// Gets or sets the database to execute queries against.
		/// </summary>
		public IDatabase Database
		{
			get
			{
				if (_database == null)
				{
					_database = Db.For<T>();
				}

				return _database;
			}
			set
			{
				_database = value;
			}
		}

		/// <summary>
		/// Advances to the next page and retrieves the results.
		/// </summary>
		/// <param name="results">The results for the next page.</param>
		/// <returns>True if there are results on the page; false if no more pages.</returns>
		public bool NextPage(out IEnumerable<T> results)
		{
			LoadMeta();

			if (IdBook.PageCount >= CurrentPage + 1)
			{
				++CurrentPage;
				List<long> ids = IdBook.PageNumber(CurrentPage);
				QuerySet sql = Database.GetService<QuerySet>();
				sql.Top<T>(PageSize);
				QueryFilter? queryFilter = (QueryFilter?)Query.FilterDelegate.DynamicInvoke(OrderByColumn);
				sql.Where(queryFilter! && new QueryFilter("Id").In(ids.Select(i => (object)i).ToArray())!).OrderBy(OrderByColumn.ToString()!, SortOrder);
				CurrentResults = new DaoCollection<C, T>(Database, sql.ExecuteGetDataTable(Database));
			}
			SetLastEntry();
			results = CurrentResults = null!;
			return CurrentResults.Count > 0;
		}
		private void SetLastEntry()
		{
			if (CurrentResults.Count > 0)
			{
				LastEntry = CurrentResults[CurrentResults.Count - 1];
			}
			else
			{
				LastEntry = default(T)!;
			}
		}

		/// <summary>
		/// Gets the results of the current page.
		/// </summary>
		public DaoCollection<C, T> CurrentResults
		{
			get;
			private set;
		} = null!;

		/// <summary>
		/// Gets or sets all matching IDs for the query.
		/// </summary>
		public long[] Ids { get; set; } = null!;

		/// <summary>
		/// Gets or sets the last entry from the current page.
		/// </summary>
		public T LastEntry { get; set; } = default!;

		protected Book<long> IdBook { get; set; } = null!;

		bool _metaLoaded;
		protected internal void LoadMeta()
		{
			if (!_metaLoaded)
			{
				string id = "Id";
				SqlStringBuilder sql = GetBaseIdQuery();
				DataTable table = sql.ExecuteGetDataTable(Database);
				List<long> ids = new List<long>();
				foreach(DataRow row in table.Rows)
				{
					ids.Add(Database.GetLongValue(id, row)!.Value);
				}
				Ids = ids.ToArray();
				IdBook = new Book<long>(Ids, PageSize);

                _metaLoaded = true;
            }
		}

		private SqlStringBuilder GetBaseIdQuery()
		{
			// get the ids for the specified query
			SqlStringBuilder sql = Database.GetService<SqlStringBuilder>();
            sql.Select(Dao.TableName(typeof(T)), sql.ColumnNameFormatter(Dao.GetKeyColumnName(typeof(T))));
			SetQuery(sql);
			return sql;
		}

		private void SetQuery(ISqlStringBuilder sql)
		{
			Args.ThrowIfNull(OrderByColumn, "OrderByColumn");
			IQueryFilter? queryFilter = (IQueryFilter?)Query.FilterDelegate.DynamicInvoke(OrderByColumn);
			sql = sql.Where(queryFilter!).OrderBy(OrderByColumn.ToString()!, SortOrder);
		}
	}
	
}
