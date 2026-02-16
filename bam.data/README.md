# bam.data

Core data access layer providing a database-agnostic ORM framework built on the Data Access Object (DAO) pattern.

## Overview

bam.data is the foundational data access library in the BAM toolkit. It provides an abstract `Database` class and a rich `Dao` (Data Access Object) base class that together form a provider-agnostic ORM. The library handles connection management, SQL generation, schema creation, query building, parameterized execution, and object hydration across multiple relational database backends.

The library uses a dependency injection / service provider pattern (`DependencyProvider`) to resolve database-specific implementations at runtime. Each database provider (MSSQL, MySQL, PostgreSQL, Oracle, Firebird, SQLite) registers its own `SchemaWriter`, `QuerySet`, `ParameterBuilder`, and `SqlStringBuilder` implementations into the service provider, allowing the core `Database` and `Dao` classes to remain backend-agnostic.

bam.data also includes a built-in SQLite provider (`SQLiteDatabase`), a fluent query filter system with operator overloads for building WHERE clauses, transaction support via `DaoTransaction`, paged enumeration through `DaoCollection`, and a schema initialization pipeline that can auto-create tables from DAO type metadata.

## Key Classes

| Class | Description |
|---|---|
| `Database` | Abstract base for all database connections. Manages connection pooling, SQL execution, DataTable/DataSet retrieval, schema ensuring, and reader-based queries. |
| `Dao` | Abstract Data Access Object base class. Tracks new/dirty values, supports Save/Commit/Delete lifecycle with before/after events, handles Insert vs Update logic, and provides column value accessors. |
| `Db` | Static utility class for resolving databases by type or connection name via the default `DatabaseContainer`. Entry point for `BeginTransaction` and `EnsureSchema`. |
| `DatabaseContainer` | Registry mapping connection names to `IDatabase` instances. Lazily initializes databases through `DatabaseInitializers` on first access. |
| `SqlStringBuilder` | Fluent SQL statement builder supporting SELECT, INSERT, UPDATE, DELETE, WHERE, AND, ORDER BY with parameterized queries. |
| `QuerySet` | Extends `SqlStringBuilder` to execute multiple SQL statements and collect multiple result sets (selects, inserts, counts). |
| `QueryFilter` | Fluent filter builder with C# operator overloads (`==`, `!=`, `<`, `>`, `&&`, `\|\|`) for constructing parameterized WHERE clauses. |
| `DaoCollection<C, T>` | Typed collection of DAO instances backed by a `DataTable`. Supports paging, parent-child associations, batch commit, and batch delete. |
| `DaoTransaction` | Transaction wrapper that tracks inserts, updates, and deletes for rollback support. Auto-rolls-back on dispose if not committed. |
| `SchemaWriter` | Abstract SQL DDL generator. Writes CREATE TABLE and ALTER TABLE ADD CONSTRAINT statements from DAO type metadata (column attributes, foreign keys). |
| `SchemaInitializer` | Initializes a database schema by resolving a schema context type and registrar caller, then calling `EnsureSchema`. |
| `Hydrator` | Loads child collections for a DAO instance, providing eager-loading support. |
| `ConnectionStringResolvers` | Chain-of-responsibility pattern for resolving connection strings by name. |
| `ConfiguredDatabaseFactory` | Abstract factory for creating `Database` instances from configuration, with auto-discovery of factory subclasses. |
| `SQLiteDatabase` | Built-in SQLite database implementation using `System.Data.SQLite`. Supports file-based databases with directory auto-creation. |
| `Registrar` | Static holder for the default registrar action. Provider-specific registrars (e.g., `SQLiteRegistrar`) register services into `DependencyProvider`. |
| `Query` | Static entry point for creating `QueryFilter` instances with `Query.Where("column")` syntax. |
| `PagedEnumerator<T>` | Base class for page-based enumeration over collections. |
| `DatabaseProvider<T>` | Abstract generic class that auto-sets `Database` properties on object instances via reflection. |

## Dependencies

### Project References
- `bam.base` -- Core utilities, extension methods, logging, dependency injection
- `bam.configuration` -- Configuration and application name providers

### Package References
- `mongocsharpdriver` 2.30.0
- `Stub.System.Data.SQLite.Core.NetStandard` 1.0.119

### Target Framework
- `net10.0`

## Usage Examples

### Creating a database and ensuring schema
```csharp
// SQLite (built-in provider)
var db = new SQLiteDatabase("/path/to/data", "MyApp");
db.TryEnsureSchema<MyDao>();
```

### Setting the default database for a DAO type
```csharp
Db.For<MyDao>(new SQLiteDatabase("./data", "mydb"));
```

### Saving a DAO instance
```csharp
var item = new MyDao();
item.Name = "Example";
item.Save(); // inserts if new, updates if existing
```

### Querying with QueryFilter
```csharp
QueryFilter filter = Query.Where("Name") == "Example";
// Or with operator syntax:
QueryFilter filter2 = Query.Where("Age") > 21;
QueryFilter combined = filter.And(filter2);
```

### Using SqlStringBuilder directly
```csharp
ISqlStringBuilder sql = db.GetSqlStringBuilder();
sql.Select<MyDao>().Where(new AssignValue("Name", "Example")).Go();
DataTable results = sql.GetDataTable(db);
```

### Transactions
```csharp
using (var tx = Db.BeginTransaction<MyDao>())
{
    var item = new MyDao { Name = "test" };
    item.Save(tx.Database);
    tx.Commit(); // or let it auto-rollback on dispose
}
```

### DaoCollection usage
```csharp
DaoCollection<MyColumns, MyDao> collection = new DaoCollection<MyColumns, MyDao>(query, load: true);
foreach (MyDao dao in collection)
{
    Console.WriteLine(dao.Name);
}
collection.Commit(); // batch-commit all modified items
```

## Known Gaps / Not Yet Implemented

- **`UniversalIdResolver`** -- `GetId()` throws `NotImplementedException`. Stub class for universal ID resolution.
- **`UniversalDeterministicIdResolver`** -- `GetId()` throws `NotImplementedException`. Stub class for deterministic ID resolution.
- **`SQLiteDatabaseProvider`** -- `GetAppDatabase`, `GetAppDatabaseFor`, `GetSysDatabase`, and `GetSysDatabaseFor` methods all throw `NotImplementedException`. Only `SetDatabases` is implemented.
- **`InComparison.ParameterInfo`** -- `SetNumber()` and `Operator` property throw `NotImplementedException` (internal parameter info helper, not used externally).
- **`StaticDatabaseInitializer`** -- `Ignore` methods throw `NotImplementedException`.
- **`AppPaths`** -- Contains a TODO to derive a proper `AppPaths` instance representing all relevant path information.
- **`Database.PrepareCommand`** -- Contains a TODO to refactor connection parameter handling.
- **`Query{C,T}`** -- Contains a TODO to add `FilterInspector` operations.
- **`_Data/SqlStringBuilder`** -- Contains a TODO to reimplement using `RoslynCompiler` and Handlebars templates; `ExecuteDynamicReader` is not implemented on this platform.
