# bam.data.mssql

Microsoft SQL Server database provider for the bam.data ORM framework.

## Overview

bam.data.mssql implements the bam.data provider interface for Microsoft SQL Server. It supplies the `MsSqlDatabase` class (a `Database` subclass) along with MSSQL-specific implementations of SQL string building, schema writing, parameter building, query set execution, data type translation, and connection string resolution.

The project follows the standard bam.data registrar pattern: when a `MsSqlDatabase` is instantiated, it creates a `DependencyProvider`, sets the `DbProviderFactory` to `SqlClientFactory.Instance`, and calls `MsSqlRegistrar.Register` to wire up MSSQL-specific service implementations. The registrar configures `MsSqlParameterBuilder` for building `SqlParameter` instances, `MsSqlSqlStringBuilder` for generating MSSQL-compatible DDL, `MsSqlQuerySet` for handling `SELECT @@IDENTITY` after inserts, and `MsSqlDataTypeTranslator` for mapping .NET types to SQL Server data types.

The `MsSqlDatabaseInitializer` extends `DefaultDatabaseInitializer` to automatically register MSSQL services when databases are initialized from connection strings in configuration.

## Key Classes

| Class | Description |
|---|---|
| `MsSqlDatabase` | SQL Server-specific `Database` subclass. Resolves connection strings via `MsSqlConnectionStringResolver`, registers MSSQL services on construction. |
| `MsSqlRegistrar` | Static registrar that wires up `MsSqlParameterBuilder`, `MsSqlSqlStringBuilder`, `MsSqlQuerySet`, and `MsSqlDataTypeTranslator` into a `DependencyProvider`. |
| `MsSqlRegistrarCaller` | `IRegistrarCaller` implementation that delegates to `MsSqlRegistrar` for use with `SchemaInitializer`. |
| `MsSqlSqlStringBuilder` | `SchemaWriter` subclass generating MSSQL-compatible DDL with IDENTITY columns and proper data type formatting (bigint, int, datetime, bit, decimal). |
| `MsSqlParameterBuilder` | Builds `SqlParameter` instances from `IParameterInfo` for parameterized queries. |
| `MsSqlQuerySet` | `QuerySet` subclass that uses `SELECT @@IDENTITY` to retrieve auto-generated IDs after insert operations. |
| `MsSqlDataTypeTranslator` | Maps .NET types and bam.data `DataTypes` to SQL Server data type names. |
| `MsSqlConnectionStringResolver` | Builds SQL Server connection strings from server name, database name, and optional credentials. |
| `MsSqlCredentials` | SQL Server credentials (UserId, Password) extending `DatabaseCredentials`. |
| `MsSqlDatabaseInitializer` | `DefaultDatabaseInitializer` subclass that auto-registers MSSQL services when creating databases from configuration. |
| `MsSqlExtensions` | Extension methods for MSSQL-specific operations. |
| `MsSqlPid` | Utility for MSSQL process ID operations. |

## Dependencies

### Project References
- `bam.base` -- Core utilities, extension methods, dependency injection
- `bam.configuration` -- Configuration and application name providers
- `bam.data` -- Core data access layer (Database, Dao, SqlStringBuilder, etc.)

### Package References
- `System.Data.SqlClient` 4.9.0

### Target Framework
- `net10.0`

## Usage Examples

### Creating a SQL Server database connection
```csharp
var creds = new MsSqlCredentials
{
    UserId = "sa",
    Password = "YourPassword"
};
var db = new MsSqlDatabase("localhost", "MyDatabase", creds);
```

### Creating with Windows authentication (no credentials)
```csharp
var db = new MsSqlDatabase("myserver", "MyDatabase");
```

### Creating from a raw connection string
```csharp
var db = new MsSqlDatabase(
    "Server=localhost;Database=MyDb;Trusted_Connection=True;",
    "MyConnectionName"
);
```

### Setting as the default database for a DAO type
```csharp
Db.For<MyDao>(new MsSqlDatabase("localhost", "MyDatabase", creds));
```

### Ensuring schema
```csharp
db.TryEnsureSchema<MyDao>();
```

### Using MsSqlDatabaseInitializer
```csharp
// Ignore specific connection names during auto-initialization
var initializer = new MsSqlDatabaseInitializer("IgnoreThisConnection");
```

### Registering MSSQL services manually
```csharp
MsSqlRegistrar.Register(db);
// or by DAO type:
MsSqlRegistrar.Register<MyDao>();
```

## Known Gaps / Not Yet Implemented

- No known gaps. All interfaces are fully implemented.
