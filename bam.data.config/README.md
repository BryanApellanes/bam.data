# bam.data.config

Database configuration module that provides a unified factory for creating database instances from structured configuration data.

## Overview

bam.data.config is a small configuration bridge that maps declarative database configuration to concrete `Database` instances. Its central class, `DatabaseConfig`, encapsulates the connection name, server name, database name, credentials, and database type (SQLite, MSSQL, PostgreSQL, or MySQL) needed to construct a ready-to-use database connection.

The module supports loading configuration from YAML files, both from a profile directory (`~/.bam/`) and from arbitrary file paths. It serves as the glue layer between the abstract `bam.data` core and the concrete provider projects (`bam.data.mssql`, `bam.data.mysql`, `bam.data.postgres`), referencing all of them so that any configured database type can be instantiated without the caller needing to reference every provider individually.

This project is useful when you want to externalize database configuration into files and have a single entry point (`DatabaseConfig.GetDatabase()`) that returns the correct provider-specific `Database` subclass.

## Key Classes

| Class | Description |
|---|---|
| `DatabaseConfig` | Configuration POCO that maps a `RelationalDatabaseTypes` enum value to a factory method creating the corresponding `Database` subclass (`SQLiteDatabase`, `MsSqlDatabase`, `NpgsqlDatabase`, or `MySqlDatabase`). Supports YAML-based loading from profile or arbitrary paths. |

## Dependencies

### Project References
- `bam.base` -- Core utilities, extension methods, dependency injection
- `bam.configuration` -- Application configuration and name providers
- `bam.data` -- Core data access layer (Database, Dao, SQLiteDatabase)
- `bam.data.mssql` -- Microsoft SQL Server provider
- `bam.data.mysql` -- MySQL provider
- `bam.data.oracle` -- Oracle provider
- `bam.data.postgres` -- PostgreSQL provider

### Package References
- None (all dependencies are project references)

### Target Framework
- `net10.0`

## Usage Examples

### Loading config from the default profile
```csharp
DatabaseConfig[] configs = DatabaseConfig.LoadProfileConfigs();
Database db = configs[0].GetDatabase();
```

### Loading config from a specific YAML file
```csharp
DatabaseConfig[] configs = DatabaseConfig.LoadConfigs("/path/to/DatabaseConfigs.yaml");
Database db = configs[0].GetDatabase();
```

### Creating a config programmatically
```csharp
var config = new DatabaseConfig
{
    ConnectionName = "MyDb",
    ServerName = "localhost",
    DatabaseName = "myapp",
    DatabaseType = RelationalDatabaseTypes.Postgres,
    Credentials = new DatabaseCredentials
    {
        UserId = "admin",
        Password = "secret"
    }
};
Database db = config.GetDatabase();
```

### Shorthand: get the first configured database
```csharp
Database db = DatabaseConfig.GetFirstDatabase("/path/to/config.yaml");
```

## Known Gaps / Not Yet Implemented

- **Firebird not included in `RelationalDatabaseTypes`** -- The `RelationalDatabaseTypes` enum only includes SQLite, MsSql, MySql, and Postgres. Firebird and Oracle are not represented, so `DatabaseConfig` cannot create Firebird databases through the factory. Oracle is referenced as a project dependency but has no entry in the factory dictionary.
