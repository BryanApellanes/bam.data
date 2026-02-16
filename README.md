# bam.data

## Running Tests

### Unit Tests

```bash
dotnet run --project bam.data.tests/bam.data.tests.csproj -- --ut
```

### Integration Tests

Integration tests require database containers to be running. A helper script starts all four containers via podman:

```bash
bash start-test-containers.sh
```

This starts (or skips if already running):

| Container | Image | Port |
|-----------|-------|------|
| bam-data-test-postgres | postgres:16 | 5432 |
| bam-data-test-mssql | mcr.microsoft.com/mssql/server:2022-latest | 1433 |
| bam-data-test-mysql | mysql:8.0 | 3306 |
| bam-data-test-oracle | gvenzl/oracle-xe:21-slim | 1521 |

Run all integration tests:

```bash
dotnet run --project bam.data.tests/bam.data.tests.csproj -- --it
```

Run a specific database's tests using `--it=<selector>`:

```bash
dotnet run --project bam.data.tests/bam.data.tests.csproj -- --it=pg   # Postgres
dotnet run --project bam.data.tests/bam.data.tests.csproj -- --it=ms   # MSSQL
dotnet run --project bam.data.tests/bam.data.tests.csproj -- --it=my   # MySQL
dotnet run --project bam.data.tests/bam.data.tests.csproj -- --it=or   # Oracle
```

After all integration tests complete, the `[AfterIntegrationTests]` teardown automatically drops the `TestItem` table from each database.