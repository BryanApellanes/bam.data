using Bam.Data;
using Bam.DependencyInjection;
using Bam.Test;
using Npgsql;
using NpgsqlTypes;

namespace Bam.Tests;

[UnitTestMenu("Json and uuid array data layer should")]
public class JsonAndUuidArrayDataShould : UnitTestMenuContainer
{
    public JsonAndUuidArrayDataShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void TranslateJsonThroughAllThreeMappings()
    {
        When.A<DataTypeTranslator>("translates Json through all three mappings",
            new DataTypeTranslator(),
            (translator) => new JsonTranslationOutcome(
                translator.EnumFromType(typeof(Json)),
                translator.TypeFromDataType(DataTypes.Json),
                translator.TranslateDataType("jsonb"),
                translator.TranslateDataType("json"),
                translator.TranslateDataType(" JSONB ")))
        .TheTest
        .ShouldPass<JsonTranslationOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("typeof(Json) maps to DataTypes.Json", outcome.FromClrType == DataTypes.Json);
            because.ItsTrue("DataTypes.Json maps to typeof(Json)", outcome.ClrType == typeof(Json));
            because.ItsTrue("jsonb translates to DataTypes.Json", outcome.FromJsonb == DataTypes.Json);
            because.ItsTrue("json translates to DataTypes.Json", outcome.FromJson == DataTypes.Json);
            because.ItsTrue("casing and spacing variants normalize", outcome.FromVariant == DataTypes.Json);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void TranslateUuidArrayThroughAllThreeMappings()
    {
        When.A<DataTypeTranslator>("translates uuid arrays through all three mappings",
            new DataTypeTranslator(),
            (translator) => new UuidArrayTranslationOutcome(
                translator.EnumFromType(typeof(Guid[])),
                translator.TypeFromDataType(DataTypes.UuidArray),
                translator.TranslateDataType("uuid[]"),
                translator.TranslateDataType(" UUID[] ")))
        .TheTest
        .ShouldPass<UuidArrayTranslationOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("typeof(Guid[]) maps to DataTypes.UuidArray", outcome.FromClrType == DataTypes.UuidArray);
            because.ItsTrue("DataTypes.UuidArray maps to typeof(Guid[])", outcome.ClrType == typeof(Guid[]));
            because.ItsTrue("uuid[] translates to DataTypes.UuidArray", outcome.FromUuidArray == DataTypes.UuidArray);
            because.ItsTrue("casing and spacing variants normalize", outcome.FromVariant == DataTypes.UuidArray);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderJsonbColumnDdlForPostgres()
    {
        When.A<NpgsqlSqlStringBuilder>("renders a jsonb column definition with its default",
            new NpgsqlSqlStringBuilder(),
            (builder) => builder.GetColumnDefinition(new JsonColumnAttribute("'[]'") { Name = "Lessons" }))
        .TheTest
        .ShouldPass<string>((because, _, definition) =>
        {
            because.ItsTrue("definition renders jsonb with DEFAULT before NOT NULL and no () artifact",
                definition.Equals("\"Lessons\" jsonb DEFAULT '[]' NOT NULL"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderUuidArrayColumnDdlForPostgres()
    {
        When.A<NpgsqlSqlStringBuilder>("renders a uuid[] column definition with its empty-array default",
            new NpgsqlSqlStringBuilder(),
            (builder) => builder.GetColumnDefinition(new UuidArrayColumnAttribute { Name = "RelatedEpisodeIds" }))
        .TheTest
        .ShouldPass<string>((because, _, definition) =>
        {
            because.ItsTrue("definition renders uuid[] with DEFAULT before NOT NULL and no () artifact",
                definition.Equals("\"RelatedEpisodeIds\" uuid[] DEFAULT '{}' NOT NULL"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void DegradeJsonToNearestTextTypePerProvider()
    {
        When.A<JsonColumnAttribute>("degrades to each provider's nearest text type",
            new JsonColumnAttribute("'{}'") { Name = "Metadata" },
            (jsonColumn) => new JsonDegradeOutcome(
                new SQLiteSqlStringBuilder().GetColumnDefinition(jsonColumn),
                new MySqlSqlStringBuilder().GetColumnDefinition(jsonColumn),
                new MsSqlSqlStringBuilder().GetColumnDefinition(jsonColumn),
                new OracleSqlStringBuilder().GetColumnDefinition(jsonColumn),
                new FirebirdSqlSqlStringBuilder().GetColumnDefinition(jsonColumn)))
        .TheTest
        .ShouldPass<JsonDegradeOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("SQLite degrades to TEXT with the default",
                outcome.SQLite.Equals("\"Metadata\" TEXT DEFAULT '{}' NOT NULL"));
            because.ItsTrue("MySql degrades to JSON with a parenthesized expression default",
                outcome.MySql.Equals("Metadata JSON DEFAULT ('{}') NOT NULL"));
            because.ItsTrue("MsSql degrades to NVARCHAR(MAX) with the default",
                outcome.MsSql.Equals("\"Metadata\" NVARCHAR(MAX) DEFAULT '{}' NOT NULL"));
            because.ItsTrue("Oracle degrades to CLOB with the default",
                outcome.Oracle.Equals("Metadata CLOB DEFAULT '{}' NOT NULL"));
            because.ItsTrue("Firebird degrades to BLOB SUB_TYPE TEXT with the default",
                outcome.Firebird.Contains("BLOB SUB_TYPE TEXT DEFAULT '{}' NOT NULL"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void FailFastOnUuidArrayForProvidersWithoutSupport()
    {
        When.A<UuidArrayColumnAttribute>("is rejected by providers without uuid[] support",
            new UuidArrayColumnAttribute { Name = "RelatedEpisodeIds" },
            (uuidArrayColumn) =>
            {
                int columnGuards = 0;
                SchemaWriter[] writers = new SchemaWriter[]
                {
                    new MsSqlSqlStringBuilder(),
                    new MySqlSqlStringBuilder(),
                    new OracleSqlStringBuilder(),
                    new FirebirdSqlSqlStringBuilder(),
                    new SQLiteSqlStringBuilder()
                };
                foreach (SchemaWriter writer in writers)
                {
                    try
                    {
                        writer.GetColumnDefinition(uuidArrayColumn);
                    }
                    catch (NotSupportedException)
                    {
                        columnGuards++;
                    }
                }
                return columnGuards;
            })
        .TheTest
        .ShouldPass<int>((because, _, columnGuards) =>
        {
            because.ItsTrue("all five non-Postgres providers throw NotSupportedException", columnGuards == 5);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void BindJsonAsJsonbTypedParameter()
    {
        When.A<NpgsqlParameterBuilder>("binds a Json value as a jsonb-typed parameter",
            new NpgsqlParameterBuilder(),
            (parameterBuilder) => (NpgsqlParameter)parameterBuilder.BuildParameter("Metadata", new Json("{\"a\":1}")))
        .TheTest
        .ShouldPass<NpgsqlParameter>((because, _, parameter) =>
        {
            because.ItsTrue("the parameter is jsonb typed", parameter.NpgsqlDbType == NpgsqlDbType.Jsonb);
            because.ItsTrue("the value is the raw JSON text", "{\"a\":1}".Equals(parameter.Value));
            because.ItsTrue("the name is prefixed", parameter.ParameterName.Equals(":Metadata"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void PassGuidArrayParameterThroughNatively()
    {
        When.A<NpgsqlParameterBuilder>("passes a Guid array through for native uuid[] binding",
            new NpgsqlParameterBuilder(),
            (parameterBuilder) =>
            {
                Guid[] ids = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
                NpgsqlParameter parameter = (NpgsqlParameter)parameterBuilder.BuildParameter("RelatedEpisodeIds", ids);
                return new GuidArrayBindingOutcome(ReferenceEquals(parameter.Value, ids), parameter.ParameterName);
            })
        .TheTest
        .ShouldPass<GuidArrayBindingOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("the Guid array reaches the driver untouched", outcome.ValueIsSameArray);
            because.ItsTrue("the name is prefixed", outcome.ParameterName.Equals(":RelatedEpisodeIds"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void HydrateJsonAndUuidArrayValueShapes()
    {
        When.A<JsonUuidTestTableDao>("hydrates JSON and uuid array values from every round-trip shape",
            new JsonUuidTestTableDao(),
            (dao) =>
            {
                Guid first = Guid.NewGuid();
                Guid second = Guid.NewGuid();

                dao.SetValue("Metadata", "{\"a\":1}");
                Json? fromText = dao.Metadata;

                dao.SetValue("Metadata", new Json("[1,2]"));
                Json? fromInstance = dao.Metadata;

                dao.SetValue("Metadata", DBNull.Value);
                Json? fromNull = dao.Metadata;

                dao.SetValue("RelatedEpisodeIds", new Guid[] { first, second });
                Guid[]? fromNative = dao.RelatedEpisodeIds;

                dao.SetValue("RelatedEpisodeIds", $"{{{first},{second}}}");
                Guid[]? fromLiteral = dao.RelatedEpisodeIds;

                dao.SetValue("RelatedEpisodeIds", "{}");
                Guid[]? fromEmptyLiteral = dao.RelatedEpisodeIds;

                dao.SetValue("RelatedEpisodeIds", DBNull.Value);
                Guid[]? fromDbNull = dao.RelatedEpisodeIds;

                return new HydrationOutcome(first, second, fromText, fromInstance, fromNull,
                    fromNative, fromLiteral, fromEmptyLiteral, fromDbNull);
            })
        .TheTest
        .ShouldPass<HydrationOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("raw text hydrates to Json", outcome.FromText!.Equals(new Json("{\"a\":1}")));
            because.ItsTrue("a Json instance passes through", outcome.FromInstance!.Equals(new Json("[1,2]")));
            because.ItsTrue("DBNull hydrates to null Json", outcome.FromNull == null);
            because.ItsTrue("a native Guid array passes through",
                outcome.FromNative!.Length == 2 && outcome.FromNative[0] == outcome.First && outcome.FromNative[1] == outcome.Second);
            because.ItsTrue("a postgres array literal parses",
                outcome.FromLiteral!.Length == 2 && outcome.FromLiteral[0] == outcome.First && outcome.FromLiteral[1] == outcome.Second);
            because.ItsTrue("an empty array literal hydrates to an empty array", outcome.FromEmptyLiteral!.Length == 0);
            because.ItsTrue("DBNull hydrates to null uuid array", outcome.FromDbNull == null);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    private sealed record JsonTranslationOutcome(DataTypes FromClrType, Type ClrType, DataTypes FromJsonb, DataTypes FromJson, DataTypes FromVariant);
    private sealed record UuidArrayTranslationOutcome(DataTypes FromClrType, Type ClrType, DataTypes FromUuidArray, DataTypes FromVariant);
    private sealed record JsonDegradeOutcome(string SQLite, string MySql, string MsSql, string Oracle, string Firebird);
    private sealed record GuidArrayBindingOutcome(bool ValueIsSameArray, string ParameterName);
    private sealed record HydrationOutcome(Guid First, Guid Second, Json? FromText, Json? FromInstance, Json? FromNull,
        Guid[]? FromNative, Guid[]? FromLiteral, Guid[]? FromEmptyLiteral, Guid[]? FromDbNull);
}
