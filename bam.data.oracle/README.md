# bam.data.oracle

Oracle database provider for the bam.data ORM framework.

## Overview

bam.data.oracle implements the bam.data provider interface for Oracle databases using Oracle's managed data access client. It supplies the `OracleDatabase` class (a `Database` subclass) along with Oracle-specific implementations of SQL string building, schema writing, parameter building, query set execution, and connection string resolution.

Oracle databases have several unique behaviors that this provider handles: commands are split by line and executed individually (since Oracle does not support multi-statement batches in the same way as SQL Server), `BindByName` is enabled on all commands, decimal-based ID values are converted to long, and column name quoting is disabled (plain names instead of brackets). The parameter prefix is `:` instead of `@`.

The provider also includes `OracleSchemaInitializer` for convenient schema initialization, `OracleDatasetProvider` for building DataSets from Oracle queries, and `IPLSqlStringBuilder` for PL/SQL-specific operations including returning the ID parameter from INSERT statements.

## Key Classes

| Class | Description |
|---|---|
| `OracleDatabase` | Oracle-specific `Database` subclass. Splits multi-line SQL for execution, enables `BindByName`, handles decimal-to-long ID conversion, and uses `:` parameter prefix. |
| `OracleRegistrar` | Static registrar that wires up `OracleParameterBuilder`, `OracleSqlStringBuilder`, `OracleQuerySet`, and the default `DataTypeTranslator` into a `DependencyProvider`. |
| `OracleRegistrarCaller` | `IRegistrarCaller` implementation that delegates to `OracleRegistrar` for use with `SchemaInitializer`. |
| `OracleSqlStringBuilder` | `SchemaWriter` subclass generating Oracle-compatible DDL with Oracle-specific column definitions and sequences. |
| `OracleParameterBuilder` | Builds `OracleParameter` instances from `IParameterInfo` for parameterized queries. Uses `:` as the parameter prefix. |
| `OracleQuerySet` | `QuerySet` subclass handling Oracle's approach to identity retrieval after inserts. |
| `OracleConnectionStringResolver` | Builds Oracle connection strings from server name and credentials. |
| `OracleCredentials` | Oracle credentials (UserId, Password) extending `DatabaseCredentials`. |
| `OracleDatabaseInitializer` | Database initializer for Oracle-specific schema setup. |
| `OracleSchemaInitializer` | Convenience `SchemaInitializer` subclass pre-configured with `OracleRegistrarCaller`. |
| `OracleDatasetProvider` | Provides DataSet construction from Oracle query results. |
| `OracleFormatProvider` | Provides Oracle-specific SQL formatting. |
| `IPLSqlStringBuilder` | Interface for PL/SQL string builders that support `RETURNING` clauses with `OracleParameter` for ID retrieval. |

## Dependencies

### Project References
- `bam.base` -- Core utilities, extension methods, dependency injection
- `bam.configuration` -- Configuration and application name providers
- `bam.data` -- Core data access layer (Database, Dao, SqlStringBuilder, etc.)

### Package References
- `Oracle.ManagedDataAccess.Core` 23.26.0

### Target Framework
- `net10.0`

## Usage Examples

### Creating an Oracle database connection
```csharp
var creds = new OracleCredentials
{
    UserId = "myuser",
    Password = "mypassword"
};
var db = new OracleDatabase("myserver:1521/ORCL", creds);
```

### Creating with explicit server, connection name, and credentials
```csharp
var db = new OracleDatabase("myserver:1521/ORCL", "MyConnectionName", creds);
```

### Creating with userId and password directly
```csharp
var db = new OracleDatabase("myserver:1521/ORCL", "myuser", "mypassword");
```

### Setting as the default database for a DAO type
```csharp
Db.For<MyDao>(new OracleDatabase("myserver:1521/ORCL", creds));
```

### Using OracleSchemaInitializer
```csharp
var initializer = new OracleSchemaInitializer(typeof(MyDaoContext));
initializer.Initialize(logger, out Exception ex);
```

### Registering Oracle services manually
```csharp
OracleRegistrar.Register(db);
// or by DAO type:
OracleRegistrar.Register<MyDao>();
```

## Known Gaps / Not Yet Implemented

- **Not included in `RelationalDatabaseTypes` enum** -- The `RelationalDatabaseTypes` enum in bam.data does not include an Oracle entry, so `DatabaseConfig` in bam.data.config cannot create Oracle databases through the configuration factory (though Oracle is referenced as a project dependency in bam.data.config).
- **Uses default `DataTypeTranslator`** -- Unlike MSSQL and MySQL which register custom data type translators, the Oracle registrar uses the base `DataTypeTranslator`, which may not handle all Oracle-specific data type mappings.
