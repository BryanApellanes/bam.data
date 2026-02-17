/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Collections;
using System.Data;
using System.Reflection;

namespace Bam.Data
{
    /// <summary>
    /// A typed collection of Dao instances that supports paging, parent-child relationships, and batch persistence operations.
    /// </summary>
    /// <typeparam name="C">The query filter/column type used for querying.</typeparam>
    /// <typeparam name="T">The Dao type contained in this collection.</typeparam>
    public class DaoCollection<C, T> : PagedEnumerator<T>, IEnumerable<T>, ILoadable, IHasDataTable, IAddable
        where C : QueryFilter, IFilterToken, new()
        where T : IDao, new()
    {
        Book<T> _book;
        List<T> _values;
        DataTable _table = null!;

        IDao _parent = null!;

        ConstructorInfo _ctor = null!;

        /// <summary>
        /// Implicitly converts a DataTable to a DaoCollection.
        /// </summary>
        /// <param name="table">The DataTable to convert.</param>
        public static implicit operator DaoCollection<C, T>(DataTable table)
        {
            return new DaoCollection<C, T>(table);
        }

        /// <summary>
        /// Initializes a new empty DaoCollection.
        /// </summary>
        public DaoCollection()
        {
            this._book = new Book<T>();
            this._values = new List<T>();
        }

        /// <summary>
        /// Initializes a new DaoCollection from a DataTable.
        /// </summary>
        /// <param name="table">The DataTable containing the row data.</param>
        /// <param name="parent">The optional parent Dao instance.</param>
        /// <param name="referencingColumn">The foreign key column name referencing the parent.</param>
        public DaoCollection(DataTable table, IDao parent = null!, string referencingColumn = null!)
            : this()
        {
            this._parent = parent;
            this._table = table;
            this.ReferencingColumn = referencingColumn;

            SetDataTable(table);
        }

		/// <summary>
		/// Initializes a new DaoCollection from a DataTable using the specified database.
		/// </summary>
		/// <param name="database">The database to associate with this collection.</param>
		/// <param name="table">The DataTable containing the row data.</param>
		/// <param name="parent">The optional parent Dao instance.</param>
		/// <param name="referencingColumn">The foreign key column name referencing the parent.</param>
		public DaoCollection(IDatabase database, DataTable table, IDao parent = null!, string referencingColumn = null!)
			: this()
		{
			this._parent = parent;
			this._table = table;
			this.ReferencingColumn = referencingColumn;
			this.Database = database;

			SetDataTable(table);
		}


        /// <summary>
        /// Initializes a new DaoCollection from a query.
        /// </summary>
        /// <param name="query">The query to use for loading data.</param>
        /// <param name="parent">The optional parent Dao instance.</param>
        /// <param name="referencingColumn">The foreign key column name referencing the parent.</param>
        public DaoCollection(IQuery<C, T> query, IDao parent = null!, string referencingColumn = null!): this()
        {
            this._parent = parent;
			this.Query = query;
			this.Database = query.Database;
            this.ReferencingColumn = referencingColumn;
        }
        
        /// <summary>
        /// Initializes a new DaoCollection from a query, optionally loading immediately.
        /// </summary>
        /// <param name="db">The database to load from.</param>
        /// <param name="query">The query to use for loading data.</param>
        /// <param name="load">If true, loads the collection immediately.</param>
        public DaoCollection(IDatabase db, IQuery<C, T> query, bool load = false): this(query, null!, null!)
        {
            if (load)
            {
                Load(db);
            }
        }

        /// <summary>
        /// Initializes a new DaoCollection from a query, optionally loading immediately using the default database.
        /// </summary>
        /// <param name="query">The query to use for loading data.</param>
        /// <param name="load">If true, loads the collection immediately.</param>
        public DaoCollection(IQuery<C, T> query, bool load = false): this(query, null!, null!)
        {
            if (load)
            {
                Load();
            }
        }

        /// <summary>
        /// Converts this collection to a different DaoCollection type, preserving the parent reference.
        /// </summary>
        /// <typeparam name="Co">The target DaoCollection type.</typeparam>
        /// <returns>A new collection of the specified type.</returns>
        public Co Convert<Co>() where Co : DaoCollection<C, T>, IHasDataTable, new()
        {
            Co val = As<Co>();
            val.Parent = this.Parent;
            return val;
        }

        protected string ReferencingColumn
        {
            get;
            set;
        } = null!;

        /// <summary>
        /// Gets or sets the query used to load this collection.
        /// </summary>
        public IQuery<C, T> Query
        {
            get;
            set;
        } = null!;

        /// <summary>
        /// Gets the first DataRow in this collection, or a default row if no data is present.
        /// </summary>
        public DataRow DataRow
        {
            get => DataTable?.Rows?[0] ?? typeof(T).ToDataRow(Dao.TableName(typeof(T)));
            set { }
        }

        IDatabase _database = null!;
        /// <summary>
        /// Gets or sets the database associated with this collection. Defaults to the parent's database or the default database for type T.
        /// </summary>
        public IDatabase Database
        {
            get
            {
                if (_database == null)
                {
                    if (Parent != null)
                    {
                        _database = Parent.Database;
                    }
                    else
                    {
                        _database = Db.For<T>();
                    }
                }

                return _database;
            }
            set
            {
                _database = value;
				SetEachDatabase();
            }
        }

        /// <summary>
        /// Instantiates a new instance of T and calls SetDataTable passing
        /// in the DataTable from the current instance
        /// </summary>
        /// <typeparam name="To"></typeparam>
        /// <returns></returns>
        public To As<To>() where To : IHasDataTable, new()
        {
            To val = new To();
            val.SetDataTable(this.DataTable);
            return val;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this collection has been loaded from the database.
        /// </summary>
        public bool Loaded
        {
            get;
            set;
        }
        
        /// <summary>
        /// Loads the collection from the default database using the configured query.
        /// </summary>
        public void Load()
        {
            Load(Database);
        }

        /// <summary>
        /// Loads the collection from the specified database using the configured query.
        /// </summary>
        /// <param name="db">The database to load from.</param>
        public void Load(IDatabase db)
        {
            if (Query == null)
            {
                throw new ArgumentNullException("Query is not set");
            }
            Database = db;
            SetDataTable(Query.GetDataTable(db));
        }

        /// <summary>
        /// Reload the current collection using the original query
        /// used to populate it
        /// </summary>
        public void Reload()
        {
            Load();
        }

        /// <summary>
        /// Sets the DataTable backing this collection and initializes its items.
        /// </summary>
        /// <param name="table">The DataTable to set.</param>
        public void SetDataTable(DataTable table)
        {
            Initialize(table);
            this.Reset();
            Loaded = true;
        }

        /// <summary>
        /// Gets the parent Dao instance that this collection belongs to.
        /// </summary>
        public IDao Parent
        {
            get => this._parent;
            protected set => this._parent = value;
        }
        
        private void Initialize(DataTable table)
        {
            _ctor = typeof(T).GetConstructor(new Type[] { typeof(Database),  typeof(DataRow) })!;
            _values = new List<T>();
            foreach (DataRow row in table.Rows)
            {
                T dao = (T)_ctor.Invoke(new object[] { Database, row });
                _values.Add(dao);
            }
            this._book = new Book<T>(_values);
        }
        
        /// <summary>
        /// Gets or sets the underlying DataTable for this collection.
        /// </summary>
        public DataTable DataTable
        {
            get => this._table;
            set => this._table = value;
        }

        /// <summary>
        /// Gets the Dao instance at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index.</param>
        /// <returns>The Dao instance at the specified index.</returns>
        public T this[int index] => this._values[index];

        /// <summary>
        /// Instantiate a new instance of T and associate it to
        /// the parent of this DaoCollection.
        /// </summary>
        /// <returns></returns>
        public T AddChild()
        {
            T dao = new T()
            {
                Database = Database
            };
            Add(dao);

            return dao;
        }

        /// <summary>
        /// Add the specified instance to the current
        /// collection.  Will be automatically committed
        /// if a parent is associated with this collection
        /// </summary>
        /// <param name="instance"></param>
        public virtual void Add(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (_parent != null)
            {
                AssociateToParent(instance);
            }

            this._values.Add(instance);
            this._book = new Book<T>(this._values);
        }

        /// <summary>
        /// Clears all the values in this DaoCollection by deleting
        /// values from the specified database.
        /// </summary>
        /// <param name="db"></param>
        public virtual void Clear(IDatabase? db = null!)
        {
            Delete(db);
            _values = new List<T>();
            _book = new Book<T>();
        }

        /// <summary>
        /// Adds a range of Dao instances to this collection, associating each to the parent if one is set.
        /// </summary>
        /// <param name="values">The Dao instances to add.</param>
        public virtual void AddRange(IEnumerable<T> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (_parent != null)
            {
                foreach (T val in values)
                {
                    AssociateToParent(val);
                }
            }

            this._values.AddRange(values);
            this._book = new Book<T>(this._values);
        }

        private void AssociateToParent(T instance)
        {
            Type childType = instance.GetType();

            Validate();
            
            // from the parent get the ReferencedBy Attribute that matches the referencingClass name
            PropertyInfo[] properties = childType.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.HasCustomAttributeOfType(out ForeignKeyAttribute fk))
                {
                    if (fk.ReferencedTable.Equals(Dao.TableName(_parent)) && fk.Name.Equals(ReferencingColumn))
                    {
                        Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                        property.SetValue(instance, System.Convert.ChangeType(_parent.DbId!.Value, propertyType), null);
                    }
                }
            }
        }

        protected virtual void Validate()
        {
            ValidateParent();
        }

        private void ValidateParent()
        {
            if (_parent == null)
            {
                throw new ArgumentNullException($"{this.GetType().Name}.Parent");
            }

            if (_parent.IsNew || _parent.DbId == null)
            {
                throw new InvalidOperationException("The parent hasn't been committed, unable to associate child by id");
            }   
        }
        
        /// <summary>
        /// Saves all items in this collection to the default database. Same as Commit.
        /// </summary>
        public void Save()
        {
            Commit();
        }

		/// <summary>
		/// Saves all items in this collection to the specified database.
		/// </summary>
		/// <param name="db">The database to save to.</param>
		public void Save(Database db)
		{
			Commit(db);
		}

        /// <summary>
        /// Commits all items in this collection to the default database.
        /// </summary>
        public void Commit()
        {
            Commit(Database);
        }

        /// <summary>
        /// Event fired after a commit operation completes.
        /// </summary>
        public event ICommittableDelegate AfterCommit = null!;

        /// <summary>
        /// Commits all items in this collection to the specified database.
        /// </summary>
        /// <param name="db">The database to commit to.</param>
        public void Commit(IDatabase? db)
        {
			db = db ?? Database;
            SqlStringBuilder sql = db.ServiceProvider.Get<SqlStringBuilder>();
            WriteCommit(sql, db);

            sql.Execute(db);

            AfterCommit?.Invoke(db, this);
        }

        /// <summary>
        /// Writes commit SQL statements for all items with new values into the specified SqlStringBuilder.
        /// </summary>
        /// <param name="sql">The SqlStringBuilder to write commit statements into.</param>
        /// <param name="db">The optional database context.</param>
        public void WriteCommit(ISqlStringBuilder sql, IDatabase? db = null!)
        {
			db = db ?? Database;
            List<T> children = new List<T>();
            foreach (T dao in this._values)
            {
                if (dao.HasNewValues)
                {
                    dao.WriteCommit(sql, db);
                    children.Add(dao);
                }
            }

            sql.Executed += (s, d) =>
            {
                //children.Each(dao => dao.OnAfterCommit(d));
                AfterCommit?.Invoke(d, this);
            };
        }

        /// <summary>
        /// Deletes all items in this collection from the specified database.
        /// </summary>
        /// <param name="db">The database to delete from; defaults to the collection's database.</param>
        public void Delete(IDatabase? db = null!)
        {
			db = db ?? Database;
            SqlStringBuilder sql = db.ServiceProvider.Get<SqlStringBuilder>();
            WriteDelete(sql);
            sql.Execute(db);
        }

        /// <summary>
        /// If true, will cause dao instances in this collection
        /// to load their child collections on delete for auto deletion
        /// </summary>
        [Exclude]
        public bool AutoHydrateChildrenOnDelete { get; set; }
		/// <summary>
		/// Write the necessary Sql statements into the specified SqlStringBuilder 
		/// to delete all the records represented by the current collection.
		/// </summary>
		/// <param name="sql"></param>
        public virtual void WriteDelete(ISqlStringBuilder sql)
        {
            if (this._values.Count > 0)
            {
                bool deleteIndividually = Parent == null;

                if (!deleteIndividually)
                {
                    if (string.IsNullOrEmpty(ReferencingColumn))
                    {
                        throw new ArgumentNullException("{0}.ReferencingColumn not set", this.GetType().Name);
                    }

                    sql.Delete(Dao.TableName(typeof(T)))
                        .Where(new AssignValue(ReferencingColumn, Parent!.DbId))
                        .Go();
                }
                
                foreach (T d in this)
                {
                    if (d.AutoDeleteChildren)
                    {
                        d.AutoHydrateChildrenOnDelete = AutoHydrateChildrenOnDelete;
                        d.WriteChildDeletes(sql);
                        sql.Go();
                    }

                    if (deleteIndividually)
                    {
                        d.WriteDelete(sql);
                        sql.Go();
                    }
                }
            }
        }

        /// <summary>
        /// Returns a sorted copy of this collection's items using the specified comparison.
        /// </summary>
        /// <param name="comparison">The comparison delegate for sorting.</param>
        /// <returns>A sorted list of items.</returns>
        public List<T> Sorted(Comparison<T> comparison)
        {
            T[] results = new T[this._values.Count];
            _values.CopyTo(results);
            List<T> sorter = new List<T>(results);
            sorter.Sort(comparison);

            return sorter;
        }

		/// <summary>
		/// Gets one value if it exists, creates it if it doesn't.  Throws MultipleEntriesFoundException
		/// if more than one value is in this collection.
		/// </summary>
		/// <param name="saveIfNew">If true and a new entry is required, the Dao value will 
		/// be saved prior to being returned </param>
		/// <returns></returns>
        public T JustOne(bool saveIfNew = false)
        {
            return JustOne(Database, saveIfNew);
        }

        /// <summary>
        /// Gets one value if it exists, creates it if it doesn't.  Throws MultipleEntriesFoundException
        /// if more than one value is in this collection.
        /// </summary>
        /// <param name="saveIfNew">If true and a new entry is required, the Dao value is saved prior to being returned.</param>
        /// <returns></returns>
        public T JustOne(IDatabase db, bool saveIfNew = false)
        {
            if (this.Count > 1)
            {
                throw new MultipleEntriesFoundException();
            }

            T? result = this.FirstOrDefault();
            if (result == null)
            {
                result = AddChild();
                if (saveIfNew)
                {
                    result.Save(db);
                }
            }

            return result;
        }

        /// <summary>
        /// Converts each item in the collection to a JSON-safe object representation.
        /// </summary>
        /// <returns>An array of JSON-safe objects.</returns>
        public object[] ToJsonSafe()
        {
            object[] result = new object[this.Count];
            this.Each((o, i) =>
            {
                result[i] = o.ColumnsToJsonSafe();
            });
            return result;
        }
        /// <summary>
        /// Get the 1 based page number or an empty list
        /// if the specified page number is not found.
        /// </summary>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public List<T> GetPage(int pageNum)
        {
            return _book[pageNum - 1];
        }    

        /// <summary>
        /// Gets the total number of pages in this collection.
        /// </summary>
        public int PageCount => this._book.PageCount;

        /// <summary>
        /// Gets the total number of items in this collection.
        /// </summary>
        public int Count => this._book.ItemCount;

        /// <summary>
        /// Gets or sets the page size for paging operations.
        /// </summary>
        public int PageSize
        {
            get => this._book.PageSize;
            set => this._book.PageSize = value;
        }

        /// <summary>
        /// Advances to the next page in the collection.
        /// </summary>
        /// <returns>True if there is a next page; otherwise false.</returns>
        public override bool MoveNextPage()
        {
            CurrentPageIndex++;
            if (CurrentPageIndex >= _book.PageCount)
            {
                return false;
            }

            this.CurrentPage = this._book[CurrentPageIndex];
            return true;
        }

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator for the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        #endregion

		#region IAddable Members

		/// <summary>
		/// Adds a value to this collection by casting to the collection's element type.
		/// </summary>
		/// <param name="value">The value to add, which must be castable to T.</param>
		public void Add(object value)
		{
			this.Add((T)value);
		}

		#endregion


		private void SetEachDatabase()
		{
			foreach(T dao in this)
			{
				dao.Database = Database;
			}
		}
	}
}
