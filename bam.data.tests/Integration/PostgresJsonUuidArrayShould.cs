using System.Text.Json.Nodes;
using Bam.Data.Npgsql;
using Bam.Data.Postgres;
using Bam.Data.Tests.Helpers;
using Bam.Test;
using Bam.Test.Integration;
using Bam.Tests;
using Npgsql;

namespace Bam.Data.Tests.Integration;

[IntegrationTestMenu("Postgres json and uuid array should", "pgju")]
public class PostgresJsonUuidArrayShould : IntegrationTestMenuContainer
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

        EnsureSchemaStatus schemaStatus = db.TryEnsureSchema<JsonUuidTestTableDao>();
        System.Console.WriteLine($"[postgres-json-uuid] TryEnsureSchema returned: {schemaStatus}");
        db.ExecuteSql("DELETE FROM JsonUuidTestTable");
        return db;
    }

    private static JsonUuidTestTableDao LoadSingleRow(IDatabase database)
    {
        ISqlStringBuilder sql = database.GetSqlStringBuilder();
        sql.Select(typeof(JsonUuidTestTableDao));
        System.Data.DataTable data = database.GetDataTable(sql);
        JsonUuidTestTableDao loaded = new JsonUuidTestTableDao(database);
        foreach (System.Data.DataColumn dataColumn in data.Columns)
        {
            loaded.SetValue(dataColumn.ColumnName, data.Rows[0][dataColumn]);
        }
        return loaded;
    }

    [IntegrationTest]
    public void CreateJsonbAndUuidArrayColumnsWithDefaults()
    {
        PostgresDatabase db = SetupDb();

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("creates a schema containing jsonb and uuid[] columns with their defaults", (database) =>
        {
            // unquoted DDL identifiers fold to lowercase in the catalogs; quoted column
            // names ("Metadata") preserve case
            string metadataType = database.QuerySingleColumn<string>(
                "SELECT udt_name FROM information_schema.columns WHERE table_name = 'jsonuuidtesttable' AND column_name = 'Metadata'")
                .FirstOrDefault()!;
            string uuidArrayType = database.QuerySingleColumn<string>(
                "SELECT udt_name FROM information_schema.columns WHERE table_name = 'jsonuuidtesttable' AND column_name = 'RelatedEpisodeIds'")
                .FirstOrDefault()!;
            string metadataDefault = database.QuerySingleColumn<string>(
                "SELECT column_default FROM information_schema.columns WHERE table_name = 'jsonuuidtesttable' AND column_name = 'Metadata'")
                .FirstOrDefault()!;
            string uuidArrayDefault = database.QuerySingleColumn<string>(
                "SELECT column_default FROM information_schema.columns WHERE table_name = 'jsonuuidtesttable' AND column_name = 'RelatedEpisodeIds'")
                .FirstOrDefault()!;
            return new SchemaShapeOutcome(metadataType, uuidArrayType, metadataDefault, uuidArrayDefault);
        })
        .TheTest
        .ShouldPass<SchemaShapeOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("the Metadata column is native jsonb", "jsonb".Equals(outcome.MetadataType));
            because.ItsTrue("the RelatedEpisodeIds column is a native uuid array", "_uuid".Equals(outcome.UuidArrayType));
            because.ItsTrue("the Metadata DEFAULT literal survived DDL",
                outcome.MetadataDefault != null && outcome.MetadataDefault.Contains("'{}'"));
            because.ItsTrue("the RelatedEpisodeIds DEFAULT literal survived DDL",
                outcome.UuidArrayDefault != null && outcome.UuidArrayDefault.Contains("'{}'"));
        })
        .SoBeHappy(_ => { })
        .UnlessItFailed();
    }

    [IntegrationTest]
    public void RoundTripJsonAndUuidArrayThroughTheDaoLayer()
    {
        PostgresDatabase db = SetupDb();

        // nested + unicode document per the design's risk mitigation for driver-side
        // Jsonb binding replacing explicit ::jsonb casts
        string nestedUnicodeJson = "{\"a\":{\"b\":\"héllo ☃\"},\"n\":[1,2,3]}";

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("round-trips a nested unicode JSON document and a uuid array through Dao save and load", (database) =>
        {
            Guid first = Guid.NewGuid();
            Guid second = Guid.NewGuid();
            JsonUuidTestTableDao saved = new JsonUuidTestTableDao(database)
            {
                Metadata = new Json(nestedUnicodeJson),
                RelatedEpisodeIds = new Guid[] { first, second }
            };
            saved.Save(database);

            JsonUuidTestTableDao loaded = LoadSingleRow(database);
            return new RoundTripOutcome(first, second, loaded.Metadata, loaded.RelatedEpisodeIds);
        })
        .TheTest
        .ShouldPass<RoundTripOutcome>((because, _, outcome) =>
        {
            // jsonb normalizes stored text (whitespace, key order), so equality is semantic,
            // not ordinal
            because.ItsTrue("the loaded JSON is semantically equal to what was saved",
                outcome.Metadata != null && JsonNode.DeepEquals(JsonNode.Parse(outcome.Metadata.Value), JsonNode.Parse(nestedUnicodeJson)));
            because.ItsTrue("the nested unicode content survived the driver round-trip",
                outcome.Metadata != null && outcome.Metadata.Value.Contains("héllo ☃"));
            because.ItsTrue("the uuid array round-trips element-for-element",
                outcome.RelatedEpisodeIds != null && outcome.RelatedEpisodeIds.Length == 2
                    && outcome.RelatedEpisodeIds[0] == outcome.First && outcome.RelatedEpisodeIds[1] == outcome.Second);
        })
        .SoBeHappy(_ => { })
        .UnlessItFailed();
    }

    [IntegrationTest]
    public void ApplyDdlDefaultsWhenValuesAreOmitted()
    {
        PostgresDatabase db = SetupDb();

        After.Setup(reg => reg.Set<IDatabase>(db))
        .When<IDatabase>("inserts a row with every defaulted column omitted", (database) =>
        {
            database.ExecuteSql("INSERT INTO JsonUuidTestTable DEFAULT VALUES");

            JsonUuidTestTableDao loaded = LoadSingleRow(database);
            return new DefaultsOutcome(loaded.Metadata, loaded.RelatedEpisodeIds);
        })
        .TheTest
        .ShouldPass<DefaultsOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("the omitted JSON column took its DDL default", new Json("{}").Equals(outcome.Metadata));
            because.ItsTrue("the omitted uuid array column took its DDL default (empty array)",
                outcome.RelatedEpisodeIds != null && outcome.RelatedEpisodeIds.Length == 0);
        })
        .SoBeHappy(_ => { })
        .UnlessItFailed();
    }

    private sealed record SchemaShapeOutcome(string MetadataType, string UuidArrayType, string? MetadataDefault, string? UuidArrayDefault);
    private sealed record RoundTripOutcome(Guid First, Guid Second, Json? Metadata, Guid[]? RelatedEpisodeIds);
    private sealed record DefaultsOutcome(Json? Metadata, Guid[]? RelatedEpisodeIds);
}
