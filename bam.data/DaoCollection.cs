/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace Bam.Data
{
    public class DaoCollection<C, T> : PagedEnumerator<T>, IEnumerable<T>, ILoadable, IHasDataTable, IAddable
        where C : QueryFilter, IFilterToken, new()
        where T : IDao, new()
    {
        Book<T> _book;
        List<T> _values;
        DataTable _table;

        IDao _parent;

        ConstructorInfo _ctor;

        public static implicit operator DaoCollection<C, T>(DataTable table)
        {
            return new DaoCollection<C, T>(table);
        }

        public DaoCollection()
        {
            this._book = new Book<T>();
            this._values = new List<T>();
        }

        public DaoCollection(DataTable table, IDao parent = null, string referencingColumn = null)
            : this()
        {
            this._parent = parent;
            this._table = table;
            this.ReferencingColumn = referencingColumn;

            SetDataTable(table);
        }

		public DaoCollection(IDatabase database, DataTable table, IDao parent = null, string referencingColumn = null)
			: this()
		{
			this._parent = parent;
			this._table = table;
			this.ReferencingColumn = referencingColumn;
			this.Database = database;

			SetDataTable(table);
		}


        public DaoCollection(IQuery<C, T> query, IDao parent = null, string referencingColumn = null): this()
        {
            this._parent = parent;
			this.Query = query;
			this.Database = query.Database;
            this.ReferencingColumn = referencingColumn;
        }
        
        public DaoCollection(IDatabase db, IQuery<C, T> query, bool load = false): this(query, null, null)
        {
            if (load)
            {
                Load(db);
            }
        }

        public DaoCollection(IQuery<C, T> query, bool load = false): this(query, null, null)
        {
            if (load)
            {
                Load();
            }
        }

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
        }

        public IQuery<C, T> Query
        {
            get;
            set;
        }

        public DataRow DataRow
        {
            get => DataTable?.Rows?[0] ?? typeof(T).ToDataRow(Dao.TableName(typeof(T)));
            set { }
        }

        IDatabase _database;
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

        public bool Loaded
        {
            get;
            set;
        }
        
        public void Load()
        {
            Load(Database);
        }

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

        public void SetDataTable(DataTable table)
        {
            Initialize(table);
            this.Reset();
            Loaded = true;
        }

        public IDao Parent
        {
            get => this._parent;
            protected set => this._parent = value;
        }
        
        private void Initialize(DataTable table)
        {
            _ctor = typeof(T).GetConstructor(new Type[] { typeof(Database),  typeof(DataRow) });
            _values = new List<T>();
            foreach (DataRow row in table.Rows)
            {
                T dao = (T)_ctor.Invoke(new object[] { Database, row });
                _values.Add(dao);
            }
            this._book = new Book<T>(_values);
        }
        
        public DataTable DataTable
        {
            get => this._table;
            set => this._table = value;
        }

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
        public virtual void Clear(IDatabase db = null)
        {
            Delete(db);
            _values = new List<T>();
            _book = new Book<T>();
        }

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
                        property.SetValue(instance, System.Convert.ChangeType(_parent.DbId.Value, propertyType), null);
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
        
        public void Save()
        {
            Commit();
        }

		public void Save(Database db)
		{
			Commit(db);
		}

        public void Commit()
        {
            Commit(Database);
        }

        public event ICommittableDelegate AfterCommit;
        public void Commit(IDatabase db)
        {
			db = db ?? Database;
            SqlStringBuilder sql = db.ServiceProvider.Get<SqlStringBuilder>();
            WriteCommit(sql, db);

            sql.Execute(db);

            AfterCommit?.Invoke(db, this);
        }

        public void WriteCommit(ISqlStringBuilder sql, IDatabase db = null)
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

        public void Delete(IDatabase db = null)
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
                        .Where(new AssignValue(ReferencingColumn, Parent.DbId))
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

            T result = this.FirstOrDefault();
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

        public object[] ToJsonSafe()
        {
            object[] result = new object[this.Count];
            this.Each((o, i) =>
            {
                result[i] = o.ToJsonSafe();
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

        public int PageCount => this._book.PageCount;

        public int Count => this._book.ItemCount;

        public int PageSize
        {
            get => this._book.PageSize;
            set => this._book.PageSize = value;
        }

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
