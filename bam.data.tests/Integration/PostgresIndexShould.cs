using Bam.Data.Npgsql;
using Bam.Data.Postgres;
using Bam.Data.Tests.Helpers;
using Bam.Test;
using Bam.Test.Integration;
using Bam.Tests;
using Npgsql;

namespace Bam.Data.Tests.Integration;

[IntegrationTestMenu("Postgres index should", "pgix")]
public class PostgresIndexShould : IntegrationTestMenuContainer
{
    private const string ContainerName = "bam-data-test-postgres";
    private const string Password = "BamTest1!";

    private static PostgresDatabase SetupDb()
    {
        NpgsqlConnection.ClearAllPools();

        PostgresDatabase db = new PostgresDatabase("localhost", "bamtest",
            new NpgsqlCredentials { UserId = "postgres", Password = Password });

        PodmanContainerHelper.WaitForReady(ContainerName, 60, () =>
        {
            using var conn = new NpgsqlConnection(db.ConnectionString);
            conn.Open();
            return true;
        });

        EnsureSchemaStatus schemaStatus = db.TryEnsureSchema<IndexTestTableDao>();
        System.Console.WriteLine($"[postgres-index] TryEnsureSchema returned: {schemaStatus}");
        return db;
    }

    [IntegrationTest]
    public void CreateDeclaredIndexesIdempotently()
    {
        PostgresDatabase db = SetupDb();

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("creates declared plain, unique, and composite indexes, twice", (database) =>
        {
            // unquoted DDL identifiers fold to lowercase in the catalogs; quoted column
            // names ("CreatedAt") preserve case inside indexdef
            NpgsqlSqlStringBuilder indexWriter = new NpgsqlSqlStringBuilder();
            indexWriter.WriteCreateIndexes(typeof(IndexTestTableDao));
            database.ExecuteSql((ISqlStringBuilder)indexWriter);

            string?[] indexDefinitions = new string?[]
            {
                database.QuerySingleColumn<string>(
                    "SELECT indexdef FROM pg_indexes WHERE tablename = 'indextesttable' AND indexname = 'ix_indextesttable_tenantid_createdat'").FirstOrDefault(),
                database.QuerySingleColumn<string>(
                    "SELECT indexdef FROM pg_indexes WHERE tablename = 'indextesttable' AND indexname = 'ix_indextesttable_createdat'").FirstOrDefault(),
                database.QuerySingleColumn<string>(
                    "SELECT indexdef FROM pg_indexes WHERE tablename = 'indextesttable' AND indexname = 'ix_indextesttable_email'").FirstOrDefault()
            };
            return indexDefinitions;
        })
        .TheTest
        .ShouldPass(because =>
        {
            string?[] results = (string?[])because.Result;
            because.ItsTrue("the composite index exists covering both columns with the declared direction",
                results[0] != null && results[0]!.Contains("TenantId") && results[0]!.Contains("CreatedAt") && results[0]!.Contains("DESC"));
            because.ItsTrue("the descending single-column index exists",
                results[1] != null && results[1]!.Contains("CreatedAt") && results[1]!.Contains("DESC"));
            because.ItsTrue("the unique index exists",
                results[2] != null && results[2]!.Contains("CREATE UNIQUE INDEX"));
        })
        .SoBeHappy(_ => { })
        .UnlessItFailed();
    }
}
