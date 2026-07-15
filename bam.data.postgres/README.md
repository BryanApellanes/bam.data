# bam.data.postgres

PostgreSQL database provider for the bam.data ORM framework.

## Overview

bam.data.postgres implements the bam.data provider interface for PostgreSQL databases using the Npgsql driver. The project provides two parallel class hierarchies: an `Npgsql`-namespaced set of classes (the core implementation) and a `Postgres`-namespaced set of convenience aliases. The primary database class is `NpgsqlDatabase`, with `PostgresDatabase` serving as a thin subclass that adds a static `Create` method.

The project follows the standard bam.data registrar pattern: when an `NpgsqlDatabase` is instantiated, it creates a `DependencyProvider`, sets the `DbProviderFactory` to `NpgsqlFactory.Instance`, and calls `NpgsqlRegistrar.Register` to wire up PostgreSQL-specific service implementations. The `NpgsqlSqlStringBuilder` is used for both general SQL building and schema DDL generation. Column names are unquoted by default (no brackets), and database names are automatically lowercased to follow PostgreSQL conventions.

Like the Oracle provider, PostgreSQL commands are split by line and executed individually, since the Npgsql driver handles multi-statement batches differently. The provider also handles int-to-long value conversions for PostgreSQL's integer returns.

## Key Classes

| Class | Description |
|---|---|
| `NpgsqlDatabase` | PostgreSQL-specific `Database` subclass using Npgsql. Handles connection string resolution, line-split SQL execution, database creation, and int-to-long value conversion. |
| `PostgresDatabase` | Thin subclass of `NpgsqlDatabase` providing a convenience `Create` static method and the `Postgres` namespace alias. |
| `NpgsqlRegistrar` | Static registrar wiring up Npgsql-specific services. Aliased as `PostgresRegistrar` in the `Bam.Data` namespace. |
| `PostgresRegistrar` | Static registrar (in `Bam.Data` namespace) that wires up `NpgsqlParameterBuilder`, `NpgsqlSqlStringBuilder`, `NpgsqlQuerySet`, and the default `DataTypeTranslator`. |
| `NpgsqlRegistrarCaller` / `PostgresRegistrarCaller` | `IRegistrarCaller` implementations for use with `SchemaInitializer`. |
| `NpgsqlSqlStringBuilder` / `PostgresSqlStringBuilder` | `SchemaWriter` subclass generating PostgreSQL-compatible DDL with SERIAL primary keys and PostgreSQL data types. |
| `NpgsqlParameterBuilder` / `PostgresParameterBuilder` | Builds `NpgsqlParameter` instances from `IParameterInfo` for parameterized queries. |
| `NpgsqlQuerySet` / `PostgresQuerySet` | `QuerySet` subclass using `SELECT CURRVAL` or similar for identity retrieval after inserts. |
| `NpgsqlConnectionStringResolver` / `PostgresConnectionStringResolver` | Builds PostgreSQL connection strings from server name, database name, and credentials. |
| `NpgsqlCredentials` / `PostgresCredentials` | PostgreSQL credentials (UserId, Password) extending `DatabaseCredentials`. |
| `NpgsqlDatabaseInitializer` / `PostgresDatabaseInitializer` | Database initializer for PostgreSQL-specific schema setup. |
| `NpgsqlForeignKeyDescriptor` / `PostgresForeignKeyDescriptor` | Descriptor classes for foreign key metadata (table, column, referenced table, referenced column). |
| `NpgsqlFormatProvider` / `PostgresFormatProvider` | Provides PostgreSQL-specific SQL formatting. |

## Dependencies

### Project References
- `bam.base` -- Core utilities, extension methods, dependency injection
- `bam.configuration` -- Configuration and application name providers
- `bam.data` -- Core data access layer (Database, Dao, SqlStringBuilder, etc.)

### Package References
- `Npgsql` 10.0.1

### Target Framework
- `net10.0`

## Usage Examples

### Creating a PostgreSQL database connection
```csharp
var creds = new NpgsqlCredentials
{
    UserId = "postgres",
    Password = "password"
};
var db = new NpgsqlDatabase("localhost", "myapp", creds);
```

### Using the PostgresDatabase convenience class
```csharp
var creds = new PostgresCredentials
{
    UserId = "postgres",
    Password = "password"
};
var db = new PostgresDatabase("localhost", "myapp", creds);
```

### Creating from a raw connection string
```csharp
var db = new NpgsqlDatabase(
    "Host=localhost;Database=myapp;Username=postgres;Password=password",
    "MyConnectionName"
);
```

### Creating a new database on the server
```csharp
var creds = new PostgresCredentials
{
    UserId = "postgres",
    Password = "password"
};
PostgresDatabase.Create("localhost", "newdb", creds, port: 5432);
// or safely:
bool success = PostgresDatabase.TryCreate("localhost", "newdb", creds);
```

### Setting as the default database for a DAO type
```csharp
Db.For<MyDao>(new NpgsqlDatabase("localhost", "myapp", creds));
```

### Ensuring schema
```csharp
db.TryEnsureSchema<MyDao>();
```

### Registering PostgreSQL services manually
```csharp
PostgresRegistrar.Register(db);
// or by connection name:
PostgresRegistrar.Register("MyConnectionName");
```

## Vector / embedding columns (pgvector)

The PostgreSQL provider supports `vector(n)` columns, vector indexes, and nearest-neighbor
(KNN) querying via [pgvector](https://github.com/pgvector/pgvector). The server needs the
extension available (e.g. the `pgvector/pgvector` image); the schema writer emits
`CREATE EXTENSION IF NOT EXISTS vector` automatically when a schema declares vector columns,
which requires sufficient privileges — pre-provision the extension on locked-down hosts.

### Declaring a vector column and index
```csharp
[Table("Episode", "MyConnection")]
public class Episode : Dao
{
    [VectorColumn(1536, Name = "Embedding")]        // renders "Embedding" vector(1536)
    [VectorIndex(Lists = 100)]                      // CREATE INDEX ... USING ivfflat ("Embedding" vector_cosine_ops) WITH (lists = 100)
    public Vector? Embedding
    {
        get => GetVectorValue("Embedding");
        set => SetValue("Embedding", value!);
    }
}
```
`VectorIndexAttribute` defaults to ivfflat + cosine; set `Method = VectorIndexMethod.Hnsw`
or `Distance = VectorDistance.Euclidean/InnerProduct` to change the operator class. ivfflat
recall depends on the data present at index-creation time — create the index after loading
representative data, or reindex after bulk loads.

### Nearest-neighbor retrieval
```csharp
ISqlStringBuilder sql = db.GetSqlStringBuilder();
sql.Select(typeof(Episode))
   .Where(new QueryFilter(new NullComparison("Embedding", "IS NOT")))  // ivfflat skips null vectors
   .OrderByNearest("Embedding", queryVector, VectorDistance.Cosine)    // ORDER BY "Embedding" <=> :Embedding1::vector
   .Limit(10);
DataTable nearest = db.GetDataTable(sql);
```

### How values are marshalled
Vectors bind as pgvector text literals (`[0.1,0.2,...]`) with an unknown parameter type so
the server infers `vector` — no extra NuGet packages (the `Pgvector.Npgsql` plugin is not
used). On reads, `Select(Type)`/`Select<T>()` project vector columns as `::text` (Npgsql
cannot read `vector`-typed fields without the plugin) and `Dao.GetVectorValue` parses the
literal back into a `Vector`.

Other providers do not support vector columns and fail fast with `NotSupportedException`
at DDL/query-build time.

## Known Gaps / Not Yet Implemented

- **Uses default `DataTypeTranslator`** -- The PostgreSQL registrar uses the base `DataTypeTranslator` rather than a PostgreSQL-specific one. This may not handle all PostgreSQL-specific data type mappings (e.g., `JSONB`, `ARRAY`, `UUID`).
- **Namespace typo** -- The `NpgsqlForeignKeyDescriptor` class is in namespace `Bam.Data.Npqsql` (note the transposed 'q' and 's'), which may cause confusion or namespace resolution issues.
