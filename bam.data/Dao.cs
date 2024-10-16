/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using Bam.Incubation;
using System.Reflection;
using System.Threading.Tasks;
using System.Diagnostics;
using Bam.Data.Schema;
using Bam.Logging;
using System.IO;
using Bam.Data;

namespace Bam.Data
{
    /// <summary>
    /// Data Access Object
    /// </summary>
    public abstract partial class Dao : IDao, ICommittable, IHasDataRow, IComparable
    {
        static Dictionary<string, string> _proxiedConnectionNames;

        static Dao()
        {
            _proxiedConnectionNames = new Dictionary<string, string>();
            PostConstructActions = new Dictionary<Type, Action<Dao>>();
        }

        public Dao()
        {
            Initialize();
        }

        public Dao(IDatabase database)
        {
            Database = database;
            Initialize();
        }

        public Dao(DataRow row)
            : this()
        {
            DataRow = row;
            IsNew = false;
        }

        public Dao(IDatabase database, DataRow row)
        {
            Database = database;
            DataRow = row;
            Initialize();
        }


        Dictionary<string, ILoadable> _childCollections;
        /// <summary>
        /// Actions, keyed by type, to take after construction.
        /// </summary>
        public static Dictionary<Type, Action<Dao>> PostConstructActions { get; set; }

        /// <summary>
        /// Instantiate all dao types found in the assembly containing the
        /// specified type and place them into the Incubator.Default.
        /// </summary>
        /// <param name="daoSibling"></param>
        public static void RegisterDaoTypes(Type daoSibling)
        {
            RegisterDaoTypes(daoSibling.Assembly, DependencyProvider.Default);
        }

        /// <summary>
        /// Instantiate all Dao types in the assembly containing the specified
        /// type and place them into the specified
        /// serviceProvider
        /// </summary>
        /// <param name="daoSibling"></param>
        /// <param name="serviceProvider"></param>
        public static void RegisterDaoTypes(Type daoSibling, DependencyProvider serviceProvider)
        {
            RegisterDaoTypes(daoSibling.Assembly, serviceProvider);
        }

        /// <summary>
        /// Instantiate all Dao types in the specified assembly and place them into the specified
        /// serviceProvider
        /// </summary>
        /// <param name="daoAssembly"></param>
        /// <param name="serviceProvider"></param>
        public static void RegisterDaoTypes(Assembly daoAssembly, DependencyProvider serviceProvider)
        {
            Type[] types = daoAssembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Type current = types[i];
                if (current.IsSubclassOf(typeof(Dao)))
                {
                    serviceProvider.Construct(current);
                }
            }
        }

        public Type[] GetSchemaTypes()
        {
            Type thisType = this.GetType();
            return GetSchemaTypes(thisType);
        }

        public static Type[] GetSchemaTypes<T>()
        {
            return GetSchemaTypes(typeof(T));
        }

        /// <summary>
        /// Returns all Types with a TableAttribute contained in the 
        /// same assembly as the specified daoType.
        /// </summary>
        /// <param name="daoType"></param>
        /// <returns></returns>
        public static Type[] GetSchemaTypes(Type daoType)
        {
            List<Type> results = new List<Type>
            {
                daoType
            };

            results.AddRange(daoType.Assembly.GetTypes().Where(t => t.HasCustomAttributeOfType<TableAttribute>()));
            return results.ToArray();
        }

        /// <summary>
        /// An overridable method to provide constructor functionality since the constructors are generated.  Overrides
        /// should be defined in the generated partial file or similar.
        /// </summary>
        /// <example>
        /// public partial class MyDao: Dao
        /// {
        ///     public override void OnInitialize()
        ///     {
        ///         ... Your initialization code here ...
        ///     }
        /// }
        /// </example>
        public virtual void OnInitialize()
        {
            // override if constructor functionality is required
            Initializer(this);
        }

        Action<IDao> _initializer;
        public Action<IDao> Initializer
        {
            get => _initializer ?? GlobalInitializer;
            set => _initializer = value;
        }

        public static DBNull Null => DBNull.Value;
        
        static Action<IDao> _globalInitializer;
        public static Action<IDao> GlobalInitializer
        {
            get
            {
                return _globalInitializer ?? ((dao) => { });
            }
            set => _globalInitializer = value;
        }

        public override int GetHashCode()
        {
            ulong id = DbId.GetValueOrDefault();
            return id > 0 ? id.GetHashCode() : base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Dao dao)
            {
                return dao.GetType() == this.GetType() && dao.DbId == this.DbId;
            }
            else
            {
                return base.Equals(obj);
            }
        }

        /// <summary>
        /// The name of the property to use in CompareTo operations.
        /// </summary>
        public string DefaultSortProperty { get; set; }

        public virtual int CompareTo(object obj)
        {
            Type thisType = GetType();
            Type objType = obj.GetType();
            if(thisType != objType)
            {
                return thisType.Name.CompareTo(objType.Name);
            }
            PropertyInfo compareProp = thisType.GetProperty(DefaultSortProperty ?? "Name");
            if(compareProp == null)
            {
                compareProp = thisType.GetProperty("IdValue");
            }
            IComparable val1 = (IComparable)compareProp.GetValue(this);
            IComparable val2 = (IComparable)compareProp.GetValue(obj);
            return val1.CompareTo(val2);
        }

        PropertyInfo _uuidProp;
        bool? _hasUuid;
        protected bool HasUuidProperty(out PropertyInfo uuidProp)
        {
            if (_hasUuid == null)
            {
                _uuidProp = GetType().GetProperty("Uuid");
                _hasUuid = _uuidProp != null;
            }

            uuidProp = _uuidProp;
            return _hasUuid.Value;
        }

        PropertyInfo _cuidProp;
        bool? _hasCuid;
        protected bool HasCuidProperty(out PropertyInfo cuidProp)
        {
            if (_hasCuid == null)
            {
                _cuidProp = GetType().GetProperty("Cuid");
                _hasCuid = _cuidProp != null;
            }

            cuidProp = _cuidProp;
            return _hasCuid.Value;
        }

        protected IDatabase _database;
        readonly object _databaseSync = new object();
        public IDatabase Database
        {
            get
            {
                return _databaseSync.DoubleCheckLock(ref _database, () => Db.For(this.GetType()));
            }
            set => _database = value;
        }

        List<string> _columns;
        readonly object _columnsLock = new object();
        public string[] Columns
        {
            get
            {
                _columns = _columnsLock.DoubleCheckLock(ref _columns, () =>
                {
                    List<string> keys = new List<string>();
                    for (int i = 0; i < DataRow?.Table?.Columns.Count; i++)
                    {
                        DataColumn current = DataRow.Table.Columns[i];
                        keys.Add(current.ColumnName);
                    }
                    return keys;
                });
                return _columns.ToArray();
            }
        }

        public TypeCode GetTypeCode(DataTypes dataTypes)
        {
            switch (dataTypes)
            {
                case DataTypes.Default:
                    return TypeCode.Int64;
                case DataTypes.Boolean:
                    return TypeCode.Boolean;
                case DataTypes.Int:
                    return TypeCode.Int32;
                case DataTypes.UInt:
                    return TypeCode.UInt32;
                case DataTypes.Long:
                    return TypeCode.Int64;
                case DataTypes.ULong:
                    return TypeCode.UInt64;
                case DataTypes.Decimal:
                    return TypeCode.Decimal;
                case DataTypes.String:
                    return TypeCode.String;
                case DataTypes.ByteArray:
                    return TypeCode.Object;
                case DataTypes.DateTime:
                    return TypeCode.DateTime;
                default:
                    return TypeCode.Object;
            }
        }

        public DataTypes GetDataType(string columnName)
        {
            return Database.GetDataTypeTranslator().TranslateDataType(columnName);
        }

        Dictionary<string, string> _columnDataTypes;
        public virtual string GetDbDataType(string columnName)
        {
            if(_columnDataTypes == null)
            {
                _columnDataTypes = new Dictionary<string, string>();
            }
            if (!_columnDataTypes.ContainsKey(columnName))
            {
                _columnDataTypes.Add(columnName, GetDbDataType(GetType(), columnName));
            }
            return _columnDataTypes[columnName];
        }

        public static string GetDbDataType(Type type, string columnName)
        {
            PropertyInfo prop = type.GetProperty(columnName);
            if (prop == null)
            {
                prop = type.GetProperties().FirstOrDefault(pi =>
                {
                    if(pi.HasCustomAttributeOfType(out ColumnAttribute column))
                    {
                        return column.Name.Equals(columnName);
                    }
                    return false;
                });
            }

            return prop?.GetCustomAttributeOfType<ColumnAttribute>()?.DbDataType ?? "VARCHAR";
        }

        /// <summary>
        /// Gets and/or sets the value of the specified column.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public T Column<T>(string columnName, object value = null)
        {
            return Column<T>(columnName, value);
        }

        /// <summary>
        /// Get the value of the specified column, setting it if
        /// value is specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public T ColumnValue<T>(string columnName, object value = null)
        {
            object val = ColumnValue(columnName, value);
            return val == null ? default(T) : (T)val;
        }

        public object ColumnValue(string columnName, object value = null)
        {
            DataTable table = DataRow.Table;
            if (!table.Columns.Contains(columnName) && value != null)
            {
                Type columnType = Database.GetDataTypeTranslator().TypeFromDbDataType(GetDbDataType(columnName));
                table.Columns.Add(columnName, columnType);
            }
            if(value != null)
            {
                DataRow[columnName] = value;                
            }
            object currentValue = GetCurrentValue(columnName);
            
            if(currentValue == null || currentValue == DBNull.Value)
            {
                return null;
            }
            return currentValue;
        }

        /// <summary>
        /// If true, any references to the current
        /// record are deleted prior to deleting
        /// the current record when Delete() is called, as long as
        /// those references were hydrated on
        /// the current instance.
        /// </summary>
        [Exclude]
        public bool AutoDeleteChildren { get; set; }

        /// <summary>
        /// If true, will hydrate child collections
        /// prior to deletion so that they can be deleted.
        /// Incurs performance cost if many child 
        /// collections must be loaded; can cause intense
        /// memory pressure if large amounts of data
        /// need to be loaded.
        /// </summary>
        [Exclude]
        public bool AutoHydrateChildrenOnDelete { get; set; }

        public event DaoDelegate BeforeWriteCommit;
        public static event DaoDelegate BeforeWriteCommitAny;
        protected internal void OnBeforeWriteCommit(IDatabase db)
        {
            BeforeWriteCommit?.Invoke(db, this);
            BeforeWriteCommitAny?.Invoke(db, this);
        }

        public event DaoDelegate AfterWriteCommit;
        public static event DaoDelegate AfterWriteCommitAny;
        protected internal void OnAfterWriteCommit(IDatabase db)
        {
            AfterWriteCommit?.Invoke(db, this);
            AfterWriteCommitAny?.Invoke(db, this);
        }

        public event DaoDelegate BeforeCommit;
        public static event DaoDelegate BeforeCommitAny;
        protected internal void OnBeforeCommit(IDatabase db)
        {
            BeforeCommit?.Invoke(db, this);
            BeforeCommitAny?.Invoke(db, this);
        }

        /// <summary>
        /// The event that fires after this instance is committed.
        /// May be fired as the result of its membership in 
        /// a DaoCollection, in that case the current
        /// Dao instance may not be fully-hydrated at the
        /// time of the firing of this event.
        /// </summary>
        public event ICommittableDelegate AfterCommit;

        /// <summary>
        /// The event that fires after any Dao instance is committed.
        /// </summary>
        public static event DaoDelegate AfterCommitAny;
        protected internal void OnAfterCommit(IDatabase db)
        {
            AfterCommit?.Invoke(db, this);
            AfterCommitAny?.Invoke(db, this);
        }

        public event DaoDelegate BeforeWriteDelete;
        public static event DaoDelegate BeforeWriteDeleteAny;
        protected void OnBeforeWriteDelete(IDatabase db)
        {
            BeforeWriteDelete?.Invoke(db, this);
            BeforeWriteDeleteAny?.Invoke(db, this);
        }

        public event DaoDelegate AfterWriteDelete;
        public static event DaoDelegate AfterWriteDeleteAny;
        protected void OnAfterWriteDelete(IDatabase db)
        {
            AfterWriteDelete?.Invoke(db, this);
            AfterWriteDeleteAny?.Invoke(db, this);
        }

        public event DaoDelegate BeforeDelete;
        public static event DaoDelegate BeforeDeleteAny;
        protected void OnBeforeDelete(IDatabase db)
        {
            BeforeDelete?.Invoke(db, this);
            BeforeDeleteAny?.Invoke(db, this);
        }

        public event DaoDelegate AfterDelete;
        public static event DaoDelegate AfterDeleteAny;
        protected void OnAfterDelete(IDatabase db)
        {
            AfterDelete?.Invoke(db, this);
            AfterDeleteAny?.Invoke(db, this);
        }

        protected internal Dictionary<string, ILoadable> ChildCollections => _childCollections;

        public virtual void Hydrate(IDatabase database = null)
        {
            database.Hydrate(this);
        }

        public virtual void HydrateChildren(IDatabase database = null)
        {
            foreach (string key in ChildCollections.Keys)
            {
                ChildCollections?[key].Load(database);
            }
        }

        /// <summary>
        /// Reset the child collections for this instance forcing
        /// them to be reloaded the next time they are referenced.
        /// </summary>
        public void ResetChildren()
        {
            _childCollections.Clear();
        }

        public virtual DaoValidationResult Validate()
        {
            return Validator(this);
        }

        Func<Dao, DaoValidationResult> _validator;
        public Func<Dao, DaoValidationResult> Validator
        {
            get
            {
                if (_validator == null)
                {
                    return GlobalValidator;
                }

                return _validator;
            }
            set => _validator = value;
        }

        static Func<Dao, DaoValidationResult> _globalValidator;
        public static Func<Dao, DaoValidationResult> GlobalValidator
        {
            get
            {
                if (_globalValidator == null)
                {
                    return (dao) => new DaoValidationResult();
                }

                return _globalValidator;
            }
            set => _globalValidator = value;
        }

        /// <summary>
        /// Checks that every required column has a value.  Untested as of 05/09/2013 :b
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public bool ValidateRequiredProperties(out string[] messages)
        {
            Type type = this.GetType();
            PropertyInfo[] props = type.GetPropertiesWithAttributeOfType<ColumnAttribute>();
            List<string> msgs = new List<string>();
            foreach (PropertyInfo prop in props)
            {
                ColumnAttribute ca = prop.GetCustomAttributeOfType<ColumnAttribute>();
                if (!(ca is KeyColumnAttribute) && !ca.AllowNull)
                {
                    string propTypeName = prop.PropertyType.Name;
                    MethodInfo getter = type.GetMethod("Get{0}Value".Format(propTypeName));
                    object val = getter.Invoke(this, new object[] { ca.Name });
                    if (val == null || (val is string s && string.IsNullOrEmpty(s)))
                    {
                        msgs.Add("{0} can't be null".Format(prop.Name));
                    }
                }
            }
            messages = msgs.ToArray();
            return messages.Length == 0;
        }

        /// <summary>
        /// Save the current instance.  If the Id is less than or
        /// equal to 0 the current instance will be Inserted, otherwise
        /// it will be Updated. Same as Commit
        /// </summary>
        public void Save()
        {
            Commit();
        }

        /// <summary>
        /// Saves the current instance asynchronously
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public Task SaveAsync(IDatabase db = null)
        {
            return Task.Run(() =>
            {
                Save(db);
            });
        }

        /// <summary>
        /// Save the current instance.  If the Id is less than or
        /// equal to 0 the current instance will be Inserted, otherwise
        /// it will be Updated.  Will also commit any updates to its
        /// children, though child commit events will not be triggerred.
        /// Same as Save
        /// </summary>
        public void Save(IDatabase db)
        {
            Commit(db);
        }

        /// <summary>
        /// Save the current instance.  If the Id is less than or
        /// equal to 0 the current instance will be Inserted, otherwise
        /// it will be Updated.  Will also commit any updates to its
        /// children, though child commit events will not be triggerred.
        /// Same as Save
        /// </summary>
        public virtual void Commit()
        {
            IDatabase db = Database;
            Commit(db);
        }

        /// <summary>
        /// Save the current instance.  If the Id is less than or
        /// equal to 0 the current instance will be Inserted, otherwise
        /// it will be Updated.  Will also commit any updates to its
        /// children, though child commit events will not be triggerred.
        /// Same as Save
        /// </summary>
        public virtual void Commit(IDaoTransaction tx)
        {
            Commit(tx.Database);
        }

        public void Commit(IDatabase db)
        {
            Commit(db, true);
        }

        /// <summary>
        /// Save the current instance.  If the Id is less than or
        /// equal to 0 the current instance will be Inserted, otherwise
        /// it will be Updated.  Will also commit any updates to its
        /// children, though child commit events will be triggerred the 
        /// children should be re-hydrated to ensure they are fully
        /// hydrated.
        /// Same as Save
        /// </summary>
        public void Commit(IDatabase db, bool commitChildren)
        {
            db = db ?? Database;

            ThrowIfInvalid();

            IQuerySet querySet = GetQuerySet(db);

            WriteCommit(querySet, db);
            if (commitChildren)
            {
                WriteChildCommits(querySet, db);
            }

            if (!string.IsNullOrWhiteSpace(querySet.ToString()))
            {
                ExecuteCommit(db, querySet);
            }
        }

        public void Update(IDatabase db = null)
        {
            db = db ?? Database;
            ThrowIfInvalid();
            IQuerySet querySet = GetQuerySet(db);
            WriteUpdate(querySet);

            if (!string.IsNullOrWhiteSpace(querySet.ToString()))
            {
                ExecuteCommit(db, querySet);
            }
        }

        public void Insert(IDatabase db = null)
        {
            db = db ?? Database;
            ThrowIfInvalid();
            IQuerySet querySet = GetQuerySet(db);
            WriteInsert(querySet);

            if (!string.IsNullOrWhiteSpace(querySet.ToString()))
            {
                ExecuteCommit(db, querySet);
            }
        }

        protected internal void WriteChildCommits(ISqlStringBuilder sql, IDatabase db = null)
        {
            db = db ?? Database;
            foreach (string key in this.ChildCollections.Keys)
            {
                ICommittable c = this.ChildCollections[key];
                c.WriteCommit(sql, db);
            }

            sql.Executed += (s, d) =>
            {
                this.ChildCollections.Clear();
            };
        }

        public void WriteChildDeletes(ISqlStringBuilder sql)
        {
            foreach (string key in ChildCollections.Keys)
            {
                ILoadable collection = ChildCollections[key];
                if (AutoHydrateChildrenOnDelete)
                {
                    if (collection.HasProperty(nameof(AutoHydrateChildrenOnDelete))) // it might be an XrefDaoCollection which doesn't have that property
                    {
                        collection.Property(nameof(AutoHydrateChildrenOnDelete), true, false);
                        collection.Load(Database);
                    }
                }
                collection.WriteDelete(sql);
            }
        }

        public virtual void Delete(IDatabase database = null)
        {
            ISqlStringBuilder sql = GetSqlStringBuilder(out IDatabase db);
            if (database != null)
            {
                sql = GetSqlStringBuilder(database);
                db = database;
            }

            if (AutoDeleteChildren)
            {
                WriteChildDeletes(sql);
            }
            WriteDelete(sql);

            OnBeforeDelete(db);
            sql.Execute(db);
            OnAfterDelete(db);
        }

        /// <summary>
        /// Loads all child collections in parallel.
        /// </summary>
        public void PreLoadChildCollections()
        {
            Parallel.ForEach(ChildCollections.Values, (loadable) =>
            {
                loadable.Load(Database);
            });
        }

        protected virtual void Delete<C>(Func<C, IQueryFilter<C>> where) where C : IFilterToken, new()
        {
            C columns = new C();
            IQueryFilter<C> filter = where(columns);
            Delete(filter);
        }

        protected virtual void Delete(IQueryFilter filter)
        {
            ISqlStringBuilder sql = GetSqlStringBuilder(out IDatabase db);

            foreach (string key in this.ChildCollections.Keys)
            {
                this.ChildCollections[key].WriteDelete(sql);
            }

            WriteDelete(sql, filter);
            sql.Execute(db);
            sql.Reset();
        }

        protected internal ISqlStringBuilder GetSqlStringBuilder()
        {
            return GetSqlStringBuilder(out IDatabase ignore);
        }

        protected internal ISqlStringBuilder GetSqlStringBuilder(out IDatabase db)
        {
            db = Database;
            return GetSqlStringBuilder(db);
        }

        protected internal static ISqlStringBuilder GetSqlStringBuilder(IDatabase db)
        {
            SqlStringBuilder sql = db.ServiceProvider.Get<SqlStringBuilder>();
            return sql;
        }

        protected internal IQuerySet GetQuerySet()
        {
            return GetQuerySet(out IDatabase ignore);
        }

        protected internal IQuerySet GetQuerySet(out IDatabase db)
        {
            db = Database;
            return GetQuerySet(db);
        }

        protected internal static IQuerySet GetQuerySet(IDatabase db)
        {
            return db.GetQuerySet();
        }

        public virtual void WriteDelete(ISqlStringBuilder sql)
        {
            IDatabase db = Database;
            OnBeforeWriteDelete(db);
            sql.Delete(TableName()).Where(new AssignValue(KeyColumnName, DbId, sql.ColumnNameFormatter));
            OnAfterWriteDelete(db);
        }

        protected internal virtual void WriteDelete(ISqlStringBuilder sqlStringBuilder, IQueryFilter filter)
        {
            sqlStringBuilder.Delete(TableName()).Where(filter).Go();
        }

        /// <summary>
        /// Write the update or insert statement for the current instance
        /// to the specified SqlStringBuilder.
        /// </summary>
        /// <param name="sqlStringBuilder"></param>
        public virtual void WriteCommit(ISqlStringBuilder sqlStringBuilder)
        {
            IDatabase db = Database;
            WriteCommit(sqlStringBuilder, db);
        }

        public virtual void WriteCommit(ISqlStringBuilder sqlStringBuilder, IDatabase db)
        {
            OnBeforeWriteCommit(db);
            if (HasNewValues)
            {
                sqlStringBuilder.Executed += (_sql, _db) => OnAfterCommit(_db);
                if (this.IsNew || this.ForceInsert)
                {
                    WriteInsert(sqlStringBuilder);
                }
                else
                {
                    WriteUpdate(sqlStringBuilder);
                }
            }
            OnAfterWriteCommit(db);
        }

        /// <summary>
        /// Write an update statement into the specified SqlStringBuilder
        /// which when executed will update the instance identified by
        /// GetUniqueFilter().
        /// </summary>
        /// <param name="sqlStringBuilder"></param>
        public void WriteUpdate(ISqlStringBuilder sqlStringBuilder)
        {
            AssignValue[] valueAssignments = GetNewAssignValues();
            sqlStringBuilder
                .Update(this.TableName(), valueAssignments)
                .Where(this.GetUniqueFilter())
                .Go();
        }

        /// <summary>
        /// Write an insert statement into the specified SqlStringBuilder
        /// which when executed will insert the current instance.
        /// </summary>
        /// <param name="sqlStringBuilder"></param>
        public void WriteInsert(ISqlStringBuilder sqlStringBuilder)
        {
            sqlStringBuilder
                .Insert(this)
                .Go();
        }

        /// <summary>
        /// Undo any changes that have been made to the current instance
        /// since it was loaded.  This method will write to the database.
        /// </summary>
        /// <param name="db"></param>
        public virtual void Undo(IDatabase db = null)
        {
            Type thisType = this.GetType();
            if (db == null)
            {
                db = Database;
            }

            ISqlStringBuilder sql = GetSqlStringBuilder(db);
            ColumnAttribute[] columns = Db.GetColumns(thisType);
            foreach (ColumnAttribute col in columns)
            {
                this.SetValue(col.Name, this.GetOriginalValue(col.Name));
            }
            AssignValue[] values = GetNewAssignValues();
            sql.Update(this.TableName(), values)
                .Where(this.GetUniqueFilter())
                .Go();

            sql.Execute(db);
        }

        /// <summary>
        /// Re-insert the current instance after it has been deleted.
        /// </summary>
        /// <param name="db"></param>
        public virtual void Undelete(IDatabase db = null)
        {
            Type thisType = this.GetType();
            if (db == null)
            {
                db = Database;
            }

            this.IsNew = true;
            ColumnAttribute[] columns = Db.GetColumns(thisType);
            foreach (ColumnAttribute col in columns)
            {
                this.SetValue(col.Name, this.GetCurrentValue(col.Name));
            }

            this.Commit();
        }
                
        /// <summary>
        /// Gets an array of AssignValue instances that represent 
        /// the names and values of columns with new values
        /// </summary>
        /// <returns></returns>
        public virtual AssignValue[] GetNewAssignValues()
        {
            List<AssignValue> valueAssignments = new List<AssignValue>();
            foreach (string columnName in this.NewValues.Keys)
            {
                valueAssignments.Add(new AssignValue(columnName, this.NewValues[columnName]));
            }
            return valueAssignments.ToArray();
        }

        /// <summary>
        /// Gets an array of AssignValue instances that represent 
        /// the names and values of columns with old values (prior to any new sets)
        /// </summary>
        /// <returns></returns>
        protected internal AssignValue[] GetOldAssignValues()
        {
            List<AssignValue> valueAssignments = new List<AssignValue>();
            foreach (DataColumn column in this.DataRow.Table.Columns)
            {
                valueAssignments.Add(new AssignValue(column.ColumnName, this.DataRow[column]));
            }
            return valueAssignments.ToArray();
        }

        /// <summary>
        /// When implemented by a derived class returns filters that should uniquely
        /// identify a single record.
        /// </summary>
        /// <returns></returns>
        public abstract IQueryFilter GetUniqueFilter();

        public Func<IDao, IQueryFilter> UniqueFilterProvider
        {
            get;
            set;
        }

        public string ConnectionName()
        {
            return ConnectionName(this);
        }

        /// <summary>
        /// Returns the connection name for the specified Dao instance or the proxied
        /// name if the connection name for the specified Dao instance has been proxied
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static string ConnectionName(object instance)
        {
            Type type = instance.GetType();
            return ConnectionName(type);
        }

        public static string ConnectionName<T>()
        {
            return ConnectionName(typeof(T));
        }

        /// <summary>
        /// Returns the connection name for the specified type or the proxied
        /// name if the connection name for the specified type is proxied.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ConnectionName(Type type)
        {
            string real = RealConnectionName(type);
            string proxied = ProxiedConnectionName(type);

            if (!real.Equals(proxied))
            {
                return proxied;
            }
            else
            {
                return real;
            }
        }

        protected internal static string RealConnectionName(Type type)
        {
            TableAttribute tableAttr = type.GetCustomAttributeOfType<TableAttribute>();
            string value = string.Empty;
            if (tableAttr != null)
            {
                value = tableAttr.ConnectionName;
            }
            else
            {
                PropertyInfo prop = type.GetProperty("ConnectionName");
                if (prop != null && prop.GetGetMethod().IsStatic && prop.PropertyType == typeof(string))
                {
                    value = (string)prop.GetValue(null, null);
                }
            }

            return value;
        }

        /// <summary>
        /// Returns the proxied connection name for the specified
        /// Type if the connection hasn't been proxied/redirected
        /// then the real connection name will be returned.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected static string ProxiedConnectionName(Type type)
        {
            string realName = RealConnectionName(type);
            if (_proxiedConnectionNames.ContainsKey(realName))
            {
                return _proxiedConnectionNames[realName];
            }
            return realName;
        }

        /// <summary>
        /// Stop redirecting the connection name for the specified
        /// type
        /// </summary>
        /// <param name="type"></param>
        public static void UnproxyConnection(Type type)
        {
            string realName = RealConnectionName(type);
            UnproxyConnection(realName);
        }

        /// <summary>
        /// Stop redirecting the connection name for the specified type
        /// </summary>
        /// <param name="realName"></param>
        public static void UnproxyConnection(string realName)
        {
            if (_proxiedConnectionNames.ContainsKey(realName))
            {
                _proxiedConnectionNames.Remove(realName);
            }
        }

        /// <summary>
        /// Causes calls to ConnectionName for the specified type to 
        /// return the specified newConnectionName.  This method must be
        /// called prior to any XXXRegistrar.Register(Type, "CxName") calls
        /// for example: SqlClientRegistrar.Register(typeof(Employee), "MyConnectionName")
        /// </summary>
        /// <param name="type"></param>
        /// <param name="newConnectionName"></param>
        public static void ProxyConnection(Type type, string newConnectionName)
        {
            string real = RealConnectionName(type);
            ProxyConnection(real, newConnectionName);
        }

        /// <summary>
        /// Causes calls to ConnectionName for the specified originalConnection 
        /// name to return the specified newConnectionName
        /// </summary>
        /// <param name="originalConnectionName"></param>
        /// <param name="newConnectionName"></param>
        public static void ProxyConnection(string originalConnectionName, string newConnectionName)
        {
            _proxiedConnectionNames.AddMissing(originalConnectionName, newConnectionName);
            _proxiedConnectionNames[originalConnectionName] = newConnectionName;
        }

        public string TableName()
        {
            return TableName(this);
        }

        public static string TableName(object instance)
        {
            if(instance != null)
            {
                Type type = instance.GetType();
                return TableName(type);
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns the table name that represents the
        /// current Dao type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string TableName(Type type)
        {
            TableAttribute tableAttr = type.GetCustomAttributeOfType<TableAttribute>();
            string value = type.Name;
            if (tableAttr != null)
            {
                value = tableAttr.TableName;
            }

            return value;
        }

        public static string GetKeyColumnName<T>() where T : Dao, new()
        {
            return GetKeyColumnName(typeof(T));
        }

        /// <summary>
        /// Gets the name of the key column by reading the 
        /// Name property of the first KeyColumnAttribute found
        /// addorning a property on the specified type.  "Id" is
        /// returned if no property with a KeyColumnAttribute is
        /// found.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static string GetKeyColumnName(Type type)
        {
            string name = "Id";
            type.GetFirstProperyWithAttributeOfType<KeyColumnAttribute>(out KeyColumnAttribute attribute);
            if (attribute != null)
            {
                name = attribute.Name;
            }
            return name;
        }

        ulong? _dbId;
        public virtual ulong? TryGetId(Action<Exception> exceptionHandler = null)
        {
            try
            {
                return GetDbId();
            }
            catch (Exception ex)
            {
                exceptionHandler = exceptionHandler ?? (Action<Exception>)(e =>{ });
                exceptionHandler(ex);
                return null;
            }
        }
        
        public virtual void SetDbId(ulong? id)
        {
            _dbId = id;
        }

        public virtual void SetDbId(object value)
        {
            if (value != null && value != DBNull.Value)
            {
                _dbId = new ulong?(Convert.ToUInt64(value));
            }
        }

        public virtual ulong? GetDbId()
        {
            object value = PrimaryKey;
            if (value != null && value != DBNull.Value)
            {
                try
                {
                    if (value.IsNumber() || value is string)
                    {
                        _dbId = new ulong?(Convert.ToUInt64(value));
                    }                        
                }
                catch (Exception ex)
                {
                    Type type = GetType();
                    Assembly assembly = type.Assembly;
                    Log.AddEntry("Exception getting IdValue for Dao instance of type ({0}.{1}) in Assembly ({2}) with hash (sha256) ({3})", ex, type.Namespace, type.Name, assembly.FullName, assembly.GetFileInfo().Sha256());
                }
            }

            return _dbId;
        }
        
        [Exclude]
        public ulong? DbId
        {
            get => GetDbId();
            set => _dbId = value;
        }

        /// <summary>
        /// Gets the name of the key column.
        /// </summary>
        [Exclude]
        public string KeyColumnName { get; protected set; }

        protected void SetKeyColumnName()
        {
            KeyColumnName = GetKeyColumnName(this.GetType());
        }

        object _primaryKey;
        /// <summary>
        /// Gets the primary key.  If the current instance is backed
        /// by a DataRow because it was hydrated from a database query,
        /// the primary key value is the value in DataRow[KeyColumnName].
        /// Otherwise, null.
        /// </summary>
        /// <value>
        /// The primary key.
        /// </value>
        [Exclude]
        public object PrimaryKey
        {
            get
            {
                if (DataRow != null && DataRow.Table.Columns.Contains(KeyColumnName))
                {
                    _primaryKey = DataRow[KeyColumnName];
                }

                return _primaryKey;
            }
            set
            {
                _primaryKey = value;
                if (DataRow != null)
                {
                    if (!DataRow.Table.Columns.Contains(KeyColumnName))
                    {
                        DataRow.Table.Columns.Add(new DataColumn(KeyColumnName, typeof(object)));
                    }
                    DataRow[KeyColumnName] = _primaryKey;
                }
            }
        }

        /// <summary>
        /// Overrides default logic as to whether 
        /// to insert or update the current instance
        /// based on the state of its Id.  This causes
        /// Save to always insert, instead of checking
        /// whether it should insert or update.
        /// </summary>
        [Exclude]
        public bool ForceInsert { get; set; }

        /// <summary>
        /// Overrides default logic as to whether 
        /// to insert or update the current instance
        /// based on the state of its Id.  This causes
        /// Save to always update instead of checking
        /// whether it should insert or update.
        /// </summary>
        [Exclude]
        public bool ForceUpdate
        {
            get => !ForceInsert;
            set => ForceInsert = !value;
        }

        bool _isNew;
        /// <summary>
        /// Gets or sets a value indiciating whether this instance has 
        /// been committed as determined by whether the DbId has a value
        /// greater than 0.
        /// </summary>
        [Exclude]
        public bool IsNew
        {
            get
            {
                if (DbId.HasValue && DbId > 0)
                {
                    _isNew = false;
                }

                return _isNew;
            }
            private set => _isNew = value;
        }

        DependencyProvider _incubator;
        [Exclude]
        public DependencyProvider ServiceProvider
        {
            get
            {
                if (_incubator == null)
                {
                    if (Database != null)
                    {
                        _incubator = Database.ServiceProvider;
                    }
                }
                return _incubator;
            }
            protected internal set => _incubator = value;
        }

        [Exclude]
        public DataRow DataRow { get; set; }

        /// <summary>
        /// Returns true if properties of the
        /// current Dao instance have been set
        /// since its instanciation.
        /// </summary>
        public bool HasNewValues => NewValues.Count > 0;

        protected internal Dictionary<string, object> NewValues
        {
            get;
            set;
        }

        protected internal DataRow ToDataRow()
        {
            return ToDataRow(this, _database?.GetDataTypeTranslator());
        }

        protected internal static DataRow ToDataRow(Dao instance, IDataTypeTranslator dataTypeTranslator = null)
        {
            if (instance.DataRow != null)
            {
                return instance.DataRow;
            }

            return ((object)instance).ToDataRow(TableName(instance), dataTypeTranslator);
        }

        protected internal object GetOriginalValue(string columnName)
        {
            if (DataRow != null && DataRow.Table.Columns.Contains(columnName))
            {
                return DataRow[columnName];
            }
            else if (columnName.Equals(KeyColumnName))
            {
                return PrimaryKey;
            }

            return null;
        }

        protected internal object GetCurrentValue(string columnName)
        {
            object result = null;
            if (columnName.Equals(KeyColumnName))
            {
                result = DbId;
                if (result == null)
                {
                    result = PrimaryKey;
                }
            }
            else
            {
                if (NewValues.ContainsKey(columnName))
                {
                    result = NewValues[columnName];
                }
                else
                {
                    result = GetOriginalValue(columnName);
                }
            }
            return result;
        }

        protected string GetStringValue(string columnName)
        {
            object val = GetCurrentValue(columnName);
            if (val != null && val != DBNull.Value)
            {
                return Convert.ToString(val); 
            }

            return string.Empty;
        }

        protected int? GetIntValue(string columnName)
        {
            object val = GetCurrentValue(columnName);
            if (val != null && val != DBNull.Value)
            {
                return new int?(Convert.ToInt32(val));
            }

            return new int?();
        }

        protected long? GetLongValue(string columnName)
        {
            object val = GetCurrentValue(columnName);
            if (val != null && val != DBNull.Value)
            {
                return new long?(Convert.ToInt64(val));
            }

            return new long?();
        }

        public static long MapUlongToLong(ulong ulongValue)
        {
            return unchecked((long)ulongValue + long.MinValue);
        }

        public static ulong MapLongToUlong(long longValue)
        {
            return unchecked((ulong)(longValue - long.MinValue));
        }

        protected ulong? GetULongValue(string columnName)
        {
            Args.ThrowIfNullOrEmpty(columnName, nameof(columnName));
            return GetULongValue(columnName, !columnName.Equals(KeyColumnName));
        }
        
        protected ulong? GetULongValue(string columnName, bool mapLongToUlong)
        {
            object val = GetCurrentValue(columnName);
            if (val != null && val != DBNull.Value)
            {
                if(val is long longVal && mapLongToUlong)
                {
                    return new ulong?(MapLongToUlong(longVal));
                }
                return new ulong?(Convert.ToUInt64(val));
            }

            return new ulong?();
        }

        protected decimal? GetDecimalValue(string columnName)
        {
            object val = GetCurrentValue(columnName);
            if (val != null && val != DBNull.Value)
            {
                return new decimal?(Convert.ToDecimal(val));
            }

            return new decimal?();
        }

        protected bool? GetBooleanValue(string columnName)
        {
            object val = GetCurrentValue(columnName);
            if (val != null && val != DBNull.Value)
            {
                if (val is long l)
                {
                    return new bool?(l > 0);
                }
                else if (val is int i)
                {
                    return new bool?(i > 0);
                }
                else if (val is string str)
                {
                    return str.IsAffirmative();
                }
                else
                {
                    return new bool?((bool)val);
                }
            }

            return new bool?(false);
        }

        protected byte[] GetByteArrayValue(string columnName)
        {
            object val = GetCurrentValue(columnName);
            if (val == null || val == DBNull.Value || val is string)
            {
                return new byte[] { };
            }
            else
            {
                return (byte[])val;
            }
        }

        protected DateTime GetDateTimeValue(string columnName)
        {
            object val = GetCurrentValue(columnName);
            if (val != null && val != DBNull.Value)
            {
                if (val is double d)
                {
                    return d.ToDateTime();
                }
                else if (val is string s)
                {
                    return DateTime.Parse(s);
                }
                else
                {
                    return (DateTime)val;
                }
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        public void SetValue(string columnName, object value)
        {
            SetValue(columnName, value, true);
        }


        protected internal void SetValue(string columnName, object value, bool mapUlongToLong)
        {
            // If the columnName specified is the key column then set the database id value.

            if (columnName.Equals(KeyColumnName))
            {
                SetDbId(value);
            }
            else
            {
                if (mapUlongToLong && value is ulong ulongVal)
                {
                    value = MapUlongToLong(ulongVal);
                }

                if (this.NewValues.ContainsKey(columnName))
                {
                    this.NewValues[columnName] = value;
                }
                else
                {
                    this.NewValues.Add(columnName, value);
                }
            }
        }
        
        private void Initialize()
        {
            this._childCollections = new Dictionary<string, ILoadable>();
            this.NewValues = new Dictionary<string, object>();
            this.ServiceProvider = DependencyProvider.Default;
            this.IsNew = true;
            this.DataRow = this.ToDataRow();
            this.AutoDeleteChildren = true;
            this.BeforeWriteCommit += (db, dao) => dao.SetUuid();

            Type currentType = this.GetType();
            if (PostConstructActions.ContainsKey(currentType))
            {
                PostConstructActions[currentType](this);
            }

            this.OnInitialize();
        }

        public void SetUuid()
        {
            if (HasUuidProperty(out PropertyInfo uuid))
            {
                string currentUuid = (string)uuid.GetValue(this);
                if (string.IsNullOrEmpty(currentUuid))
                {
                    string uuidVal = Guid.NewGuid().ToString();
                    uuid.SetValue(this, uuidVal);
                }
            } 
        }

        private void ExecuteCommit(IDatabase db, IQuerySet querySet)
        {
            OnBeforeCommit(db);
            querySet.Execute(db);
            this.IsNew = false;
            this.ResetChildren();
            this.Database = db;
            OnAfterCommit(db);
        }

        private void ThrowIfInvalid()
        {
            DaoValidationResult valid = this.Validate();
            if (!valid.Success)
            {
                throw new ValidationException(valid.Message, valid.Exception);
            }
        }
    }
}
