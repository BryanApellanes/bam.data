# bam.data.firebird

Firebird SQL database provider for the bam.data ORM framework.

## Overview

bam.data.firebird implements the bam.data provider interface for Firebird SQL databases. It supplies the `FirebirdSqlDatabase` class (a `Database` subclass) along with Firebird-specific implementations of SQL string building, schema writing, parameter building, query set execution, and connection string resolution.

The project follows the same registrar pattern used by all bam.data providers: when a `FirebirdSqlDatabase` is instantiated, it creates a `DependencyProvider`, sets the `DbProviderFactory` to `FirebirdClientFactory.Instance`, and calls `FirebirdSqlRegistrar.Register` to wire up the Firebird-specific `IParameterBuilder`, `SchemaWriter`, `SqlStringBuilder`, and `QuerySet` implementations. This allows the core `Database` and `Dao` classes to work transparently with Firebird.

Column names in Firebird are quoted with double quotes (e.g., `"ColumnName"`) to preserve case sensitivity, which differs from the default bracket-based quoting used by MSSQL.

## Key Classes

| Class | Description |
|---|---|
| `FirebirdSqlDatabase` | Firebird-specific `Database` subclass. Resolves connection strings, registers Firebird services, and handles Firebird's int-to-long value conversions. |
| `FirebirdSqlRegistrar` | Static registrar that wires up `FirebirdSqlParameterBuilder`, `FirebirdSqlSqlStringBuilder`, and `FirebirdSqlQuerySet` into a `DependencyProvider`. |
| `FirebirdSqlRegistrarCaller` | `IRegistrarCaller` implementation that delegates to `FirebirdSqlRegistrar` for use with `SchemaInitializer`. |
| `FirebirdSqlSqlStringBuilder` | `SchemaWriter` subclass generating Firebird-compatible DDL (CREATE TABLE, column definitions, key columns). |
| `FirebirdSqlParameterBuilder` | Builds `FbParameter` instances from `IParameterInfo` for parameterized queries. |
| `FirebirdSqlQuerySet` | `QuerySet` subclass that overrides ID retrieval to use Firebird's `GEN_ID` or identity mechanism. |
| `FirebirdSqlConnectionStringResolver` | Resolves connection strings for Firebird given server name, database name, and credentials. |
| `FirebirdSqlCredentials` | Firebird-specific credentials (UserId, Password). |
| `FirebirdSqlFormatProvider` | Provides Firebird-specific SQL formatting. |
| `FirebirdSqlStringBuilder` | Low-level SQL string building utilities for Firebird syntax. |

## Dependencies

### Project References
- `bam.base` -- Core utilities, extension methods, dependency injection
- `bam.configuration` -- Configuration and application name providers
- `bam.data` -- Core data access layer (Database, Dao, SqlStringBuilder, etc.)

### Package References
- `FirebirdSql.Data.FirebirdClient` 10.3.4

### Target Framework
- `net10.0`

## Usage Examples

### Creating a Firebird database connection
```csharp
var creds = new FirebirdSqlCredentials
{
    UserId = "SYSDBA",
    Password = "masterkey"
};
var db = new FirebirdSqlDatabase("localhost", "mydb", creds);
```

### Using with a named connection
```csharp
var db = new FirebirdSqlDatabase("localhost", "mydb", "MyConnectionName", creds);
Db.For("MyConnectionName", db);
```

### Ensuring schema
```csharp
db.TryEnsureSchema<MyDao>();
```

### Registering Firebird services manually
```csharp
FirebirdSqlRegistrar.Register(db);
// or by connection name:
FirebirdSqlRegistrar.Register("MyConnectionName");
```

## Known Gaps / Not Yet Implemented

- **No `IDataTypeTranslator` registered** -- Unlike MSSQL and MySQL providers which register a custom `IDataTypeTranslator`, the Firebird registrar does not register one. The default `DataTypeTranslator` from bam.data will be used, which may not handle all Firebird-specific data type mappings correctly.
- **Not included in `RelationalDatabaseTypes` enum** -- The `RelationalDatabaseTypes` enum in bam.data does not include a Firebird entry, so `DatabaseConfig` cannot create Firebird databases through the configuration factory.
