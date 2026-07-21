using Bam.Data;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

[UnitTestMenu("Index DDL should")]
public class IndexDdlShould : UnitTestMenuContainer
{
    public IndexDdlShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void RenderPlainUniqueAndCompositeIndexesFromBaseSyntax()
    {
        When.A<MsSqlSqlStringBuilder>("renders plain, unique, and composite indexes with base syntax",
            new MsSqlSqlStringBuilder(),
            (builder) =>
            {
                builder.WriteCreateIndexes(typeof(IndexTestTableDao));
                return new BaseSyntaxOutcome(builder.ToString(), builder.ColumnNameFormatter, builder.TableNameFormatter);
            })
        .TheTest
        .ShouldPass<BaseSyntaxOutcome>((because, outcome) =>
        {
            string table = outcome.TableNameFormatter("IndexTestTable");
            because.ItsTrue("the composite index covers both columns in declared order with per-column direction",
                outcome.Sql.Contains($"CREATE INDEX ix_IndexTestTable_TenantId_CreatedAt ON {table} ({outcome.ColumnNameFormatter("TenantId")}, {outcome.ColumnNameFormatter("CreatedAt")} DESC)"));
            because.ItsTrue("the property-level descending index renders DESC",
                outcome.Sql.Contains($"CREATE INDEX ix_IndexTestTable_CreatedAt ON {table} ({outcome.ColumnNameFormatter("CreatedAt")} DESC)"));
            because.ItsTrue("the unique index renders UNIQUE",
                outcome.Sql.Contains($"CREATE UNIQUE INDEX ix_IndexTestTable_Email ON {table} ({outcome.ColumnNameFormatter("Email")})"));
            because.ItsTrue("base syntax has no existence guard, matching CREATE TABLE",
                !outcome.Sql.Contains("IF NOT EXISTS"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderPostgresIndexSyntaxWithExistenceGuard()
    {
        When.A<NpgsqlSqlStringBuilder>("renders PostgreSQL index syntax with an existence guard",
            new NpgsqlSqlStringBuilder(),
            (builder) =>
            {
                builder.WriteCreateIndexes(typeof(IndexTestTableDao));
                return builder.ToString();
            })
        .TheTest
        .ShouldPass<string>((because, sql) =>
        {
            because.ItsTrue("the composite index renders with quoted columns and per-column direction",
                sql.Contains("CREATE INDEX IF NOT EXISTS ix_IndexTestTable_TenantId_CreatedAt ON IndexTestTable (\"TenantId\", \"CreatedAt\" DESC)"));
            because.ItsTrue("the property-level descending index renders with the existence guard",
                sql.Contains("CREATE INDEX IF NOT EXISTS ix_IndexTestTable_CreatedAt ON IndexTestTable (\"CreatedAt\" DESC)"));
            because.ItsTrue("the unique index renders UNIQUE with the existence guard",
                sql.Contains("CREATE UNIQUE INDEX IF NOT EXISTS ix_IndexTestTable_Email ON IndexTestTable (\"Email\")"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderSqliteIndexesWithExistenceGuard()
    {
        When.A<SQLiteSqlStringBuilder>("renders SQLite indexes with an existence guard",
            new SQLiteSqlStringBuilder(),
            (builder) =>
            {
                builder.WriteCreateIndexes(typeof(IndexTestTableDao));
                return builder.ToString();
            })
        .TheTest
        .ShouldPass<string>((because, sql) =>
        {
            because.ItsTrue("SQLite index DDL carries IF NOT EXISTS",
                sql.Contains("CREATE INDEX IF NOT EXISTS ix_IndexTestTable_CreatedAt ON "));
            because.ItsTrue("unique indexes carry the guard after UNIQUE",
                sql.Contains("CREATE UNIQUE INDEX IF NOT EXISTS ix_IndexTestTable_Email ON "));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void TruncateOracleIndexIdentifiersToThirtyCharacters()
    {
        When.A<OracleSqlStringBuilder>("truncates Oracle index identifiers to thirty characters",
            new OracleSqlStringBuilder(),
            (builder) =>
            {
                builder.WriteCreateIndexes(typeof(LongNameIndexTestTableDao));
                return builder.ToString();
            })
        .TheTest
        .ShouldPass<string>((because, sql) =>
        {
            string derivedName = "ix_ALongTableNameForIdentifierLimits_DescriptionText";
            string truncatedName = derivedName.Substring(0, 30);
            because.ItsTrue("the derived index name is truncated to thirty characters",
                sql.Contains($"CREATE INDEX {truncatedName} ON "));
            because.ItsTrue("the untruncated name does not appear", !sql.Contains(derivedName));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void DeclareFirebirdDirectionAtTheIndexLevel()
    {
        When.A<FirebirdSqlSqlStringBuilder>("declares Firebird sort direction at the index level",
            new FirebirdSqlSqlStringBuilder(),
            (builder) =>
            {
                builder.WriteCreateIndexes(typeof(DescendingIndexTestTableDao));
                string uniformSql = builder.ToString();
                bool mixedThrew = false;
                try
                {
                    new FirebirdSqlSqlStringBuilder().WriteCreateIndexes(typeof(IndexTestTableDao));
                }
                catch (NotSupportedException)
                {
                    mixedThrew = true;
                }
                return new FirebirdDirectionOutcome(uniformSql, mixedThrew);
            })
        .TheTest
        .ShouldPass<FirebirdDirectionOutcome>((because, outcome) =>
        {
            because.ItsTrue("a uniformly descending index renders DESCENDING at the index level",
                outcome.UniformSql.Contains("CREATE DESCENDING INDEX ix_DescendingIndexTestTable_CreatedAt ON "));
            because.ItsTrue("per-column direction tokens are not emitted", !outcome.UniformSql.Contains(" DESC)") && !outcome.UniformSql.Contains(" DESC,"));
            because.ItsTrue("an index mixing descending with non-descending columns is rejected", outcome.MixedThrew);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderMySqlIndexesWithBaseSyntax()
    {
        When.A<MySqlSqlStringBuilder>("renders MySql indexes with base syntax through its formatters",
            new MySqlSqlStringBuilder(),
            (builder) =>
            {
                builder.WriteCreateIndexes(typeof(IndexTestTableDao));
                return new BaseSyntaxOutcome(builder.ToString(), builder.ColumnNameFormatter, builder.TableNameFormatter);
            })
        .TheTest
        .ShouldPass<BaseSyntaxOutcome>((because, outcome) =>
        {
            string table = outcome.TableNameFormatter("IndexTestTable");
            because.ItsTrue("the composite index renders MySql-quoted columns with per-column direction",
                outcome.Sql.Contains($"CREATE INDEX ix_IndexTestTable_TenantId_CreatedAt ON {table} ({outcome.ColumnNameFormatter("TenantId")}, {outcome.ColumnNameFormatter("CreatedAt")} DESC)"));
            because.ItsTrue("the unique index renders UNIQUE",
                outcome.Sql.Contains($"CREATE UNIQUE INDEX ix_IndexTestTable_Email ON {table} ({outcome.ColumnNameFormatter("Email")})"));
            because.ItsTrue("MySql base syntax has no existence guard, matching CREATE TABLE",
                !outcome.Sql.Contains("IF NOT EXISTS"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void FailFastOnAccessMethodOptionsWithoutProviderSupport()
    {
        When.A<MsSqlSqlStringBuilder>("fails fast on access-method options without provider support",
            new MsSqlSqlStringBuilder(),
            (msSqlWriter) =>
            {
                int guards = 0;
                SchemaWriter[] writers = new SchemaWriter[]
                {
                    msSqlWriter,
                    new MySqlSqlStringBuilder(),
                    new OracleSqlStringBuilder(),
                    new FirebirdSqlSqlStringBuilder(),
                    new SQLiteSqlStringBuilder()
                };
                foreach (SchemaWriter writer in writers)
                {
                    try
                    {
                        writer.WriteCreateIndexes(typeof(VectorTestTableDao));
                    }
                    catch (NotSupportedException)
                    {
                        guards++;
                    }
                }
                return guards;
            })
        .TheTest
        .ShouldPass<int>((because, guards) =>
        {
            because.ItsTrue("all five non-Postgres writers reject access-method-bearing indexes", guards == 5);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RejectInvalidIndexDeclarations()
    {
        When.A<MsSqlSqlStringBuilder>("rejects invalid index declarations",
            new MsSqlSqlStringBuilder(),
            (builder) =>
            {
                bool noColumnThrew = false;
                try
                {
                    builder.WriteCreateIndexes(typeof(NoColumnIndexTestTableDao));
                }
                catch (InvalidOperationException)
                {
                    noColumnThrew = true;
                }
                bool unknownColumnThrew = false;
                try
                {
                    new MsSqlSqlStringBuilder().WriteCreateIndexes(typeof(UnknownColumnIndexTestTableDao));
                }
                catch (InvalidOperationException)
                {
                    unknownColumnThrew = true;
                }
                return new InvalidDeclarationOutcome(noColumnThrew, unknownColumnThrew);
            })
        .TheTest
        .ShouldPass<InvalidDeclarationOutcome>((because, outcome) =>
        {
            because.ItsTrue("an index on a property with no column attribute is rejected", outcome.NoColumnThrew);
            because.ItsTrue("a class-level index naming an unknown column is rejected", outcome.UnknownColumnThrew);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RejectDuplicateIndexNames()
    {
        When.A<MsSqlSqlStringBuilder>("rejects declarations resolving to the same index name",
            new MsSqlSqlStringBuilder(),
            (builder) =>
            {
                bool duplicateThrew = false;
                try
                {
                    builder.WriteCreateIndexes(typeof(DuplicateNameIndexTestTableDao));
                }
                catch (InvalidOperationException)
                {
                    duplicateThrew = true;
                }
                return duplicateThrew;
            })
        .TheTest
        .ShouldPass<bool>((because, duplicateThrew) =>
        {
            because.ItsTrue("a property-level and class-level declaration deriving the same name are rejected — on guarded dialects the collision would otherwise silently drop the later definition", duplicateThrew);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void InheritClassLevelIndexDeclarations()
    {
        When.A<MsSqlSqlStringBuilder>("inherits class-level index declarations from base Dao classes",
            new MsSqlSqlStringBuilder(),
            (builder) =>
            {
                builder.WriteCreateIndexes(typeof(InheritedIndexTestTableDao));
                return builder.ToString();
            })
        .TheTest
        .ShouldPass<string>((because, sql) =>
        {
            because.ItsTrue("a class-level index declared on a base Dao class renders for the derived type's table",
                sql.Contains("CREATE INDEX ix_InheritedIndexTestTable_CreatedAt ON "));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void WriteNothingWhenNoIndexesAreDeclared()
    {
        When.A<MsSqlSqlStringBuilder>("writes nothing when no indexes are declared",
            new MsSqlSqlStringBuilder(),
            (builder) =>
            {
                builder.WriteCreateIndexes(typeof(JsonUuidTestTableDao));
                return builder.ToString();
            })
        .TheTest
        .ShouldPass<string>((because, sql) =>
        {
            because.ItsTrue("no index DDL is emitted", !sql.Contains("CREATE INDEX") && !sql.Contains("CREATE UNIQUE INDEX"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    private sealed record BaseSyntaxOutcome(string Sql, Func<string, string> ColumnNameFormatter, Func<string, string> TableNameFormatter);

    private sealed record FirebirdDirectionOutcome(string UniformSql, bool MixedThrew);

    private sealed record InvalidDeclarationOutcome(bool NoColumnThrew, bool UnknownColumnThrew);
}
