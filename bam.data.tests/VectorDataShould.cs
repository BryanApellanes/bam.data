using Bam.Data;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

[UnitTestMenu("Vector data layer should")]
public class VectorDataShould : UnitTestMenuContainer
{
    public VectorDataShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void RenderVectorColumnDdlForPostgres()
    {
        When.A<NpgsqlSqlStringBuilder>("renders a vector column definition",
            new NpgsqlSqlStringBuilder(),
            (builder) => builder.GetColumnDefinition(new VectorColumnAttribute(1536) { Name = "Embedding" }))
        .TheTest
        .ShouldPass(because =>
        {
            string definition = (string)because.Result;
            because.ItsTrue("definition renders vector with its dimension", definition.Equals("\"Embedding\" vector(1536)"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void WriteVectorIndexDdlForPostgres()
    {
        When.A<NpgsqlSqlStringBuilder>("writes ivfflat index DDL from a VectorIndexAttribute",
            new NpgsqlSqlStringBuilder(),
            (builder) =>
            {
                builder.WriteCreateIndexes(typeof(VectorTestTableDao));
                return builder.ToString();
            })
        .TheTest
        .ShouldPass(because =>
        {
            string sql = (string)because.Result;
            because.ItsTrue("index DDL has the expected shape",
                sql.Contains("CREATE INDEX IF NOT EXISTS ix_VectorTestTable_Embedding ON VectorTestTable USING ivfflat (\"Embedding\" vector_cosine_ops) WITH (lists = 50)"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void OrderNearestWithParameterNumberingAndLimit()
    {
        When.A<NpgsqlSqlStringBuilder>("interleaves nearest ordering with prior filter parameters",
            new NpgsqlSqlStringBuilder(),
            (builder) =>
            {
                builder.Select("VectorTestTable", "Id", "Summary");
                builder.Where("Summary", "goal");
                builder.OrderByNearest("Embedding", new Vector(new float[] { 1f, 2f, 3f }), VectorDistance.Cosine);
                builder.Limit(10);
                return builder;
            })
        .TheTest
        .ShouldPass(because =>
        {
            NpgsqlSqlStringBuilder builder = (NpgsqlSqlStringBuilder)because.Result;
            string sql = builder.ToString();
            because.ItsTrue("ordering renders the cosine operator with a numbered cast parameter", sql.Contains("ORDER BY \"Embedding\" <=> :Embedding2::vector"));
            because.ItsTrue("limit clause is appended", sql.Contains("LIMIT 10"));
            because.ItsTrue("the query vector joined the parameter list", builder.Filters.OfType<VectorDistanceOrdering>().Any());
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void FailFastOnProvidersWithoutVectorSupport()
    {
        When.A<VectorColumnAttribute>("is rejected by providers without vector support",
            new VectorColumnAttribute(3) { Name = "Embedding" },
            (vectorColumn) =>
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
                        writer.GetColumnDefinition(vectorColumn);
                    }
                    catch (NotSupportedException)
                    {
                        columnGuards++;
                    }
                }
                bool orderingThrew = false;
                try
                {
                    SqlStringBuilder baseBuilder = new SqlStringBuilder();
                    baseBuilder.OrderByNearest("Embedding", new Vector(new float[] { 1f }), VectorDistance.Cosine);
                }
                catch (NotSupportedException)
                {
                    orderingThrew = true;
                }
                bool indexThrew = false;
                try
                {
                    new MsSqlSqlStringBuilder().WriteCreateIndexes(typeof(VectorTestTableDao));
                }
                catch (NotSupportedException)
                {
                    indexThrew = true;
                }
                return new int[] { columnGuards, orderingThrew ? 1 : 0, indexThrew ? 1 : 0 };
            })
        .TheTest
        .ShouldPass(because =>
        {
            int[] results = (int[])because.Result;
            because.ItsTrue("all five non-Postgres writers reject vector columns", results[0] == 5);
            because.ItsTrue("base OrderByNearest is not supported", results[1] == 1);
            because.ItsTrue("base WriteCreateIndexes rejects declared vector indexes", results[2] == 1);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void CoerceVectorParametersToPgvectorLiterals()
    {
        When.A<NpgsqlParameterBuilder>("coerces Vector parameter values to pgvector literals",
            new NpgsqlParameterBuilder(),
            (parameterBuilder) =>
            {
                VectorDistanceOrdering ordering = new VectorDistanceOrdering("Embedding", "<=>", new Vector(new float[] { 1.5f, 2f }), VectorDistance.Cosine, number: 1);
                System.Data.Common.DbParameter fromInfo = parameterBuilder.BuildParameter(ordering);
                System.Data.Common.DbParameter fromNamed = parameterBuilder.BuildParameter("Embedding", new Vector(new float[] { 3f }));
                return new System.Data.Common.DbParameter[] { fromInfo, fromNamed };
            })
        .TheTest
        .ShouldPass(because =>
        {
            System.Data.Common.DbParameter[] results = (System.Data.Common.DbParameter[])because.Result;
            because.ItsTrue("IParameterInfo path binds the literal text", "[1.5,2]".Equals(results[0].Value));
            because.ItsTrue("IParameterInfo path names the parameter by column and number", ":Embedding1".Equals(results[0].ParameterName));
            because.ItsTrue("named path binds the literal text", "[3]".Equals(results[1].Value));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void TranslateVectorDataTypeInAllDirections()
    {
        When.A<DataTypeTranslator>("translates the vector data type in all directions",
            new DataTypeTranslator(),
            (translator) => translator)
        .TheTest
        .ShouldPass(because =>
        {
            DataTypeTranslator translator = (DataTypeTranslator)because.Result;
            because.ItsTrue("CLR Vector maps to DataTypes.Vector", translator.EnumFromType(typeof(Vector)) == DataTypes.Vector);
            because.ItsTrue("DataTypes.Vector maps to CLR Vector", translator.TypeFromDataType(DataTypes.Vector) == typeof(Vector));
            because.ItsTrue("db type string vector maps to DataTypes.Vector", translator.TranslateDataType("vector") == DataTypes.Vector);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void HydrateVectorValuesFromLiteralsInstancesAndNull()
    {
        When.A<VectorTestTableDao>("hydrates vector values from literals, instances, and null",
            new VectorTestTableDao(),
            (dao) =>
            {
                dao.SetValue("Embedding", "[1,2,3]");
                Vector? fromLiteral = dao.Embedding;
                dao.SetValue("Embedding", new Vector(new float[] { 4f, 5f, 6f }));
                Vector? fromInstance = dao.Embedding;
                dao.SetValue("Embedding", DBNull.Value);
                Vector? fromNull = dao.Embedding;
                return new object?[] { fromLiteral, fromInstance, fromNull };
            })
        .TheTest
        .ShouldPass(because =>
        {
            object?[] results = (object?[])because.Result;
            because.ItsTrue("a stored literal hydrates to an equal Vector", new Vector(new float[] { 1f, 2f, 3f }).Equals(results[0]));
            because.ItsTrue("a stored Vector instance is returned as-is", new Vector(new float[] { 4f, 5f, 6f }).Equals(results[1]));
            because.ItsTrue("a null column hydrates to null", results[2] == null);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void PrependVectorExtensionInSchemaScript()
    {
        When.A<NpgsqlSqlStringBuilder>("prepends the vector extension when a schema declares vector columns",
            new NpgsqlSqlStringBuilder(),
            (builder) =>
            {
                builder.WriteSchemaScript(typeof(VectorTestTableDao));
                return builder.ToString();
            })
        .TheTest
        .ShouldPass(because =>
        {
            string sql = (string)because.Result;
            because.ItsTrue("extension creation precedes table creation", sql.IndexOf("CREATE EXTENSION IF NOT EXISTS vector") >= 0 && sql.IndexOf("CREATE EXTENSION IF NOT EXISTS vector") < sql.IndexOf("CREATE TABLE"));
            because.ItsTrue("the table declares the vector column", sql.Contains("\"Embedding\" vector(3)"));
            because.ItsTrue("the schema script includes the vector index", sql.Contains("USING ivfflat"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }
}
