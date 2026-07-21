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
            using NpgsqlConnection connection = new NpgsqlConnection(db.ConnectionString);
            connection.Open();
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

            return new PostgresIndexOutcome(
                database.QuerySingleColumn<string>(
                    "SELECT indexdef FROM pg_indexes WHERE tablename = 'indextesttable' AND indexname = 'ix_indextesttable_tenantid_createdat'").FirstOrDefault(),
                database.QuerySingleColumn<string>(
                    "SELECT indexdef FROM pg_indexes WHERE tablename = 'indextesttable' AND indexname = 'ix_indextesttable_createdat'").FirstOrDefault(),
                database.QuerySingleColumn<string>(
                    "SELECT indexdef FROM pg_indexes WHERE tablename = 'indextesttable' AND indexname = 'ix_indextesttable_email'").FirstOrDefault());
        })
        .TheTest
        .ShouldPass<PostgresIndexOutcome>((because, outcome) =>
        {
            because.ItsTrue("the composite index covers both columns in declared order with direction on CreatedAt only",
                outcome.CompositeIndexDefinition != null && outcome.CompositeIndexDefinition.Contains("(\"TenantId\", \"CreatedAt\" DESC)"));
            because.ItsTrue("the descending single-column index exists with direction on CreatedAt",
                outcome.DescendingIndexDefinition != null && outcome.DescendingIndexDefinition.Contains("(\"CreatedAt\" DESC)"));
            because.ItsTrue("the unique index exists",
                outcome.UniqueIndexDefinition != null && outcome.UniqueIndexDefinition.Contains("CREATE UNIQUE INDEX"));
        })
        .SoBeHappy(_ => { })
        .UnlessItFailed();
    }

    private sealed record PostgresIndexOutcome(
        string? CompositeIndexDefinition,
        string? DescendingIndexDefinition,
        string? UniqueIndexDefinition);
}
