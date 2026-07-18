using Bam.Data.Npgsql;
using Bam.Data.Postgres;
using Bam.Data.Schema;
using Bam.Data.Tests.Helpers;
using Bam.Test;
using Bam.Test.Integration;
using Bam.Tests;
using Npgsql;

namespace Bam.Data.Tests.Integration;

[IntegrationTestMenu("Postgres vector should", "pgv")]
public class PostgresVectorShould : IntegrationTestMenuContainer
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

        EnsureSchemaStatus schemaStatus = db.TryEnsureSchema<VectorTestTableDao>();
        System.Console.WriteLine($"[postgres-vector] TryEnsureSchema returned: {schemaStatus}");
        db.ExecuteSql("DELETE FROM VectorTestTable");
        return db;
    }

    [IntegrationTest]
    public void CreateVectorSchemaWithIvfflatIndex()
    {
        PostgresDatabase db = SetupDb();

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("creates a schema containing a vector column and its index", (database) =>
        {
            // unquoted DDL identifiers fold to lowercase in the catalogs; quoted column
            // names ("Embedding") preserve case
            string columnType = database.QuerySingleColumn<string>(
                "SELECT udt_name FROM information_schema.columns WHERE table_name = 'vectortesttable' AND column_name = 'Embedding'")
                .FirstOrDefault()!;
            string indexDefinition = database.QuerySingleColumn<string>(
                "SELECT indexdef FROM pg_indexes WHERE tablename = 'vectortesttable' AND indexname = 'ix_vectortesttable_embedding'")
                .FirstOrDefault()!;
            return new string?[] { columnType, indexDefinition };
        })
        .TheTest
        .ShouldPass(because =>
        {
            string?[] results = (string?[])because.Result;
            because.ItsTrue("the Embedding column is a pgvector vector", "vector".Equals(results[0]));
            because.ItsTrue("the ivfflat index exists with the cosine operator class",
                results[1] != null && results[1]!.Contains("ivfflat") && results[1]!.Contains("vector_cosine_ops") && results[1]!.Contains("lists='50'"));
        })
        .SoBeHappy(_ => { })
        .UnlessItFailed();
    }

    [IntegrationTest]
    public void InsertAndRetrieveNearestByCosineDistance()
    {
        PostgresDatabase db = SetupDb();

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("inserts embeddings and retrieves nearest-first", (database) =>
        {
            try
            {
            new VectorTestTableDao(database) { Label = "exact", Embedding = new Vector(new float[] { 1f, 0f, 0f }) }.Save(database);
            new VectorTestTableDao(database) { Label = "close", Embedding = new Vector(new float[] { 0.9f, 0.1f, 0f }) }.Save(database);
            new VectorTestTableDao(database) { Label = "orthogonal", Embedding = new Vector(new float[] { 0f, 1f, 0f }) }.Save(database);
            new VectorTestTableDao(database) { Label = "unembedded" }.Save(database);

            ISqlStringBuilder sql = database.GetSqlStringBuilder();
            NullComparison embeddingNotNull = new NullComparison("Embedding", "IS NOT")
            {
                ColumnNameFormatter = (c) => $"\"{c}\""
            };
            sql.Select("VectorTestTable", "\"Label\"")
                .Where(new QueryFilter(embeddingNotNull))
                .OrderByNearest("Embedding", new Vector(new float[] { 1f, 0f, 0f }), VectorDistance.Cosine)
                .Limit(2);
            System.Data.DataTable results = database.GetDataTable(sql);
            return results.Rows.Cast<System.Data.DataRow>().Select(row => row["Label"].ToString()).ToArray();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[postgres-vector] ordering failed: {ex}");
                throw;
            }
        })
        .TheTest
        .ShouldPass(because =>
        {
            string?[] labels = (string?[])because.Result;
            because.ItsTrue("two rows were returned by the limit", labels.Length == 2);
            because.ItsTrue("the exact match is nearest", "exact".Equals(labels[0]));
            because.ItsTrue("the close match is second", "close".Equals(labels[1]));
        })
        .SoBeHappy(_ => { })
        .UnlessItFailed();
    }

    [IntegrationTest]
    public void HydrateVectorValuesThroughTheDaoLayer()
    {
        PostgresDatabase db = SetupDb();

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("round-trips a vector through Dao save and load", (database) =>
        {
            VectorTestTableDao saved = new VectorTestTableDao(database) { Label = "roundtrip", Embedding = new Vector(new float[] { 0.25f, -0.5f, 0.75f }) };
            saved.Save(database);

            ISqlStringBuilder sql = database.GetSqlStringBuilder();
            sql.Select(typeof(VectorTestTableDao)).Where("Label", "roundtrip");
            System.Data.DataTable data = database.GetDataTable(sql);
            VectorTestTableDao loaded = new VectorTestTableDao(database);
            foreach (System.Data.DataColumn dataColumn in data.Columns)
            {
                loaded.SetValue(dataColumn.ColumnName, data.Rows[0][dataColumn]);
            }
            return loaded.Embedding!;
        })
        .TheTest
        .ShouldPass(because =>
        {
            Vector loaded = (Vector)because.Result;
            because.ItsTrue("the loaded embedding equals what was saved", new Vector(new float[] { 0.25f, -0.5f, 0.75f }).Equals(loaded));
        })
        .SoBeHappy(_ => { })
        .UnlessItFailed();
    }

    [IntegrationTest]
    public void ReverseExtractVectorColumnsWithDimensions()
    {
        PostgresDatabase db = SetupDb();

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("reverse-extracts the vector column with its dimension", (database) =>
        {
            try
            {
                NpgsqlSchemaExtractor extractor = new NpgsqlSchemaExtractor((NpgsqlDatabase)database)
                {
                    TableSchema = "public"
                };
                DataTypes dataType = extractor.GetColumnDataType("vectortesttable", "Embedding");
                string maxLength = extractor.GetColumnMaxLength("vectortesttable", "Embedding");
                return new object[] { dataType, maxLength };
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[postgres-vector] extraction failed: {ex}");
                throw;
            }
        })
        .TheTest
        .ShouldPass(because =>
        {
            object[] results = (object[])because.Result;
            because.ItsTrue("the column reverse-extracts as DataTypes.Vector", (DataTypes)results[0] == DataTypes.Vector);
            because.ItsTrue("the dimension reverse-extracts from atttypmod", "3".Equals(results[1]));
        })
        .SoBeHappy(_ => { })
        .UnlessItFailed();
    }
}
