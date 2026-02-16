# bam.data.mysql

MySQL database provider for the bam.data ORM framework.

## Overview

bam.data.mysql implements the bam.data provider interface for MySQL databases. It supplies the `MySqlDatabase` class (a `Database` subclass) along with MySQL-specific implementations of SQL string building, schema writing, parameter building, query set execution, data type translation, and connection string resolution.

The project follows the standard bam.data registrar pattern: when a `MySqlDatabase` is instantiated, it creates a `DependencyProvider`, sets the `DbProviderFactory` to `MySqlClientFactory.Instance`, and calls `MySqlRegistrar.Register` to wire up MySQL-specific service implementations. The `MySqlSqlStringBuilder` is used for both general SQL building and schema DDL generation. Column names in MySQL are unquoted (no brackets or double quotes), which the provider handles by setting `ColumnNameProvider` to return plain column names.

The `MySqlDatabase` class also overrides `GetLongValue` to handle MySQL's int-to-long conversions, since MySQL drivers may return int values where long is expected.

## Key Classes

| Class | Description |
|---|---|
| `MySqlDatabase` | MySQL-specific `Database` subclass. Resolves connection strings, registers MySQL services, handles SSL configuration, and overrides long value conversion for MySQL's int returns. |
| `MySqlRegistrar` | Static registrar that wires up `MySqlParameterBuilder`, `MySqlSqlStringBuilder`, `MySqlQuerySet`, and `MySqlDataTypeTranslator` into a `DependencyProvider`. |
| `MySqlRegistrarCaller` | `IRegistrarCaller` implementation that delegates to `MySqlRegistrar` for use with `SchemaInitializer`. |
| `MySqlSqlStringBuilder` | `SchemaWriter` subclass generating MySQL-compatible DDL. Used as both the `SqlStringBuilder` and `SchemaWriter` implementation. |
| `MySqlParameterBuilder` | Builds `MySqlParameter` instances from `IParameterInfo` for parameterized queries. |
| `MySqlQuerySet` | `QuerySet` subclass that uses `SELECT LAST_INSERT_ID()` to retrieve auto-generated IDs after insert operations. |
| `MySqlDataTypeTranslator` | Maps .NET types and bam.data `DataTypes` to MySQL data type names. |
| `MySqlConnectionStringResolver` | Builds MySQL connection strings from server name, database name, credentials, and SSL settings. |
| `MySqlCredentials` | MySQL credentials (UserId, Password) extending `DatabaseCredentials`. |
| `MySqlDatabaseInitializer` | `DefaultDatabaseInitializer` subclass for auto-registering MySQL services from configuration. |
| `MySqlFormatProvider` | Provides MySQL-specific SQL formatting. |

## Dependencies

### Project References
- `bam.base` -- Core utilities, extension methods, dependency injection
- `bam.configuration` -- Configuration and application name providers
- `bam.data` -- Core data access layer (Database, Dao, SqlStringBuilder, etc.)

### Package References
- `BouncyCastle.Cryptography` 2.6.2 (required by MySql.Data for SSL)
- `MySql.Data` 9.5.0

### Target Framework
- `net10.0`

## Usage Examples

### Creating a MySQL database connection
```csharp
var creds = new MySqlCredentials
{
    UserId = "root",
    Password = "password"
};
var db = new MySqlDatabase("localhost", "myapp", creds);
```

### Creating with SSL disabled
```csharp
var db = new MySqlDatabase("localhost", "myapp", "MyConnectionName", creds, ssl: false);
```

### Creating from a raw connection string
```csharp
var db = new MySqlDatabase(
    "Server=localhost;Database=myapp;User Id=root;Password=password;",
    "MyConnectionName"
);
```

### Setting as the default database for a DAO type
```csharp
Db.For<MyDao>(new MySqlDatabase("localhost", "myapp", creds));
```

### Ensuring schema
```csharp
db.TryEnsureSchema<MyDao>();
```

### Registering MySQL services manually
```csharp
MySqlRegistrar.Register(db);
// or by connection name:
MySqlRegistrar.Register("MyConnectionName");
```

## Known Gaps / Not Yet Implemented

- No known gaps. All interfaces are fully implemented.
