using Bam.Data.SQLite;
using Bam.Test;

namespace Bam.Data.Tests.Unit;

[UnitTestMenu("SQLite index schema should", "sqix")]
public class SqliteIndexSchemaShould : UnitTestMenuContainer
{
    private static string GetTempDbDir()
    {
        string dir = Path.Combine(Path.GetTempPath(), "bam_data_tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void CleanupDb(SQLiteDatabase db)
    {
        try
        {
            string dir = Path.GetDirectoryName(db.DatabaseFile?.FullName) ?? string.Empty;
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }
        catch
        {
            // best effort cleanup
        }
    }

    [UnitTest]
    public void CreateDeclaredIndexesIdempotently()
    {
        SQLiteDatabase db = new SQLiteDatabase(GetTempDbDir(), "BamDataTest");

        When.A<SQLiteDatabase>("ensures a schema whose Dao declares indexes, twice",
            db,
            (database) =>
            {
                // cleanup runs in finally so a failed run does not leak the temp db directory
                try
                {
                    EnsureSchemaStatus firstStatus = database.TryEnsureSchema<Bam.Tests.IndexTestTableDao>();

                    Bam.Data.SQLiteSqlStringBuilder indexWriter = new Bam.Data.SQLiteSqlStringBuilder();
                    indexWriter.WriteCreateIndexes(typeof(Bam.Tests.IndexTestTableDao));
                    database.ExecuteSql((ISqlStringBuilder)indexWriter);

                    string[] indexNames = database.QuerySingleColumn<string>(
                        "SELECT name FROM sqlite_master WHERE type = 'index' AND tbl_name = 'IndexTestTable' AND name LIKE 'ix_%' ORDER BY name")
                        .ToArray();
                    string uniqueIndexSql = database.QuerySingleColumn<string>(
                        "SELECT sql FROM sqlite_master WHERE type = 'index' AND name = 'ix_IndexTestTable_Email'")
                        .FirstOrDefault() ?? string.Empty;
                    return new SqliteIndexOutcome(firstStatus, indexNames, uniqueIndexSql);
                }
                finally
                {
                    CleanupDb(database);
                }
            })
        .TheTest
        .ShouldPass<SqliteIndexOutcome>((because, outcome) =>
        {
            because.ItsTrue("the schema was created", outcome.FirstStatus == EnsureSchemaStatus.Success || outcome.FirstStatus == EnsureSchemaStatus.AlreadyDone);
            because.ItsTrue("all three declared indexes exist after a repeated write (IF NOT EXISTS)",
                outcome.IndexNames.Contains("ix_IndexTestTable_TenantId_CreatedAt")
                && outcome.IndexNames.Contains("ix_IndexTestTable_CreatedAt")
                && outcome.IndexNames.Contains("ix_IndexTestTable_Email"));
            because.ItsTrue("exactly the three declared ix_ indexes exist — the repeat created no duplicates", outcome.IndexNames.Length == 3);
            because.ItsTrue("the email index is unique", outcome.UniqueIndexSql.Contains("CREATE UNIQUE INDEX"));
        })
        .SoBeHappy(_ => { })
        .UnlessItFailed();
    }

    private sealed record SqliteIndexOutcome(EnsureSchemaStatus FirstStatus, string[] IndexNames, string UniqueIndexSql);
}
