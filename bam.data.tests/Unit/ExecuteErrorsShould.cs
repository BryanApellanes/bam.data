using System.Data;
using System.Data.Common;
using Bam.Data.SQLite;
using Bam.Test;

namespace Bam.Data.Tests.Unit;

/// <summary>
/// Covers how <see cref="Database"/> reports a failing statement (bam.data#21): <c>ExecuteSql</c>, <c>ExecuteReader</c>,
/// <c>ExecuteReader&lt;T&gt;</c> and <c>GetDataTableFromReader</c> throw the database's own exception after firing their
/// events, and only a schema script tolerates statements that fail because their objects already exist.
/// </summary>
[UnitTestMenu("Execute errors should", "exerr")]
public class ExecuteErrorsShould : UnitTestMenuContainer
{
    private const string MissingTableQuery = "SELECT * FROM NoSuchTable";

    [UnitTest]
    public void ThrowAndReportAFailingStatement()
    {
        When.A<SQLiteDatabase>("executes an INSERT into a table that does not exist",
            NewDatabase,
            database =>
            {
                int reported = 0;
                database.CommandException += (_, _) => reported++;
                try
                {
                    return new FailureOutcome(Describe(() => database.ExecuteSql("INSERT INTO NoSuchTable VALUES (1)")), reported);
                }
                finally
                {
                    Cleanup(database);
                }
            })
            .TheTest
            .ShouldPass<FailureOutcome>((because, outcome) =>
            {
                because.ItsTrue($"the database's exception propagates ({outcome.Thrown})", outcome.Thrown.StartsWith("DbException:"));
                because.ItsTrue("CommandException fired once", outcome.Reported == 1);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void ThrowFromEveryReader()
    {
        When.A<SQLiteDatabase>("reads from a table that does not exist through each reader",
            NewDatabase,
            database =>
            {
                int reported = 0;
                database.ReaderException += (_, _) => reported++;
                try
                {
                    string reader = Describe(() => database.ExecuteReader(MissingTableQuery, Array.Empty<DbParameter>()));
                    string typed = Describe(() => database.ExecuteReader<Row>(MissingTableQuery, Array.Empty<DbParameter>()).ToArray());
                    string table = Describe(() => database.GetDataTableFromReader(MissingTableQuery, CommandType.Text, Array.Empty<DbParameter>(), database.GetOpenDbConnection()));
                    return new ReaderOutcome(reader, typed, table, reported);
                }
                finally
                {
                    Cleanup(database);
                }
            })
            .TheTest
            .ShouldPass<ReaderOutcome>((because, outcome) =>
            {
                because.ItsTrue($"ExecuteReader throws the database's exception ({outcome.Reader})", outcome.Reader.StartsWith("DbException:"));
                because.ItsTrue($"ExecuteReader<T> throws it, not a NullReferenceException ({outcome.Typed})", outcome.Typed.StartsWith("DbException:"));
                because.ItsTrue($"GetDataTableFromReader throws it ({outcome.Table})", outcome.Table.StartsWith("DbException:"));
                because.ItsTrue("ReaderException fired for each", outcome.Reported == 3);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void TolerateExistingObjectsOnlyInSchemaScripts()
    {
        When.A<SQLiteDatabase>("ensures a schema, forces it again, then creates the same table directly",
            NewDatabase,
            database =>
            {
                try
                {
                    EnsureSchemaStatus first = database.TryEnsureSchema<Bam.Tests.IndexTestTableDao>();
                    EnsureSchemaStatus again = database.TryEnsureSchema(typeof(Bam.Tests.IndexTestTableDao), true, out Exception repeatFailure);
                    string direct = Describe(() => database.ExecuteSql("CREATE TABLE IndexTestTable (Id INTEGER)"));
                    return new SchemaOutcome(first, again, repeatFailure?.Message, direct);
                }
                finally
                {
                    Cleanup(database);
                }
            })
            .TheTest
            .ShouldPass<SchemaOutcome>((because, outcome) =>
            {
                because.ItsTrue("the first run creates the schema", outcome.First == EnsureSchemaStatus.Success);
                because.ItsTrue($"a forced re-run over existing tables still succeeds ({outcome.RepeatFailure})", outcome.Again == EnsureSchemaStatus.Success && outcome.RepeatFailure is null);
                because.ItsTrue($"the same statement outside a schema script throws ({outcome.Direct})", outcome.Direct.StartsWith("DbException:"));
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void ReleaseTheConnectionsTheTableReadersOpen()
    {
        When.A<CountingDatabase>("reads a table, and a missing table, through each overload that opens its own connection",
            NewCountingDatabase,
            database =>
            {
                try
                {
                    database.GetDataTableFromReader("SELECT 1 AS Id", Array.Empty<DbParameter>());
                    database.GetDataTableFromReader("SELECT 1 AS Id", new { });
                    int afterSuccess = database.Released;
                    Describe(() => database.GetDataTableFromReader(MissingTableQuery, Array.Empty<DbParameter>()));
                    Describe(() => database.GetDataTableFromReader(MissingTableQuery, new { }));
                    Describe(() => database.GetDataTableFromReader(MissingTableQuery, Array.Empty<DbParameter>(), out DbConnection _));
                    return new ReleaseOutcome(afterSuccess, database.Released - afterSuccess);
                }
                finally
                {
                    Cleanup(database);
                }
            })
            .TheTest
            .ShouldPass<ReleaseOutcome>((because, outcome) =>
            {
                because.ItsTrue($"both successful reads release their connection ({outcome.AfterSuccess})", outcome.AfterSuccess == 2);
                because.ItsTrue($"all three failed reads release theirs ({outcome.AfterFailure})", outcome.AfterFailure == 3);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    private static SQLiteDatabase NewDatabase()
    {
        string directory = Path.Combine(Path.GetTempPath(), "bam_data_tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(directory);
        return new SQLiteDatabase(directory, "BamDataExecuteErrors");
    }

    private static void Cleanup(SQLiteDatabase database)
    {
        try
        {
            string directory = Path.GetDirectoryName(database.DatabaseFile?.FullName) ?? string.Empty;
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
        catch (IOException)
        {
            // best effort: SQLite may still hold the file briefly
        }
    }

    /// <summary>Runs the action and names what it threw: "DbException: message", "OtherType: message" or "none".</summary>
    private static string Describe(Action action)
    {
        try
        {
            action();
            return "none";
        }
        catch (DbException exception)
        {
            return "DbException: " + exception.Message;
        }
        catch (Exception exception)
        {
            return exception.GetType().Name + ": " + exception.Message;
        }
    }

    private static CountingDatabase NewCountingDatabase()
    {
        string directory = Path.Combine(Path.GetTempPath(), "bam_data_tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(directory);
        return new CountingDatabase(directory);
    }

    /// <summary>A SQLite database that counts the connections released to it.</summary>
    private sealed class CountingDatabase : SQLiteDatabase
    {
        public CountingDatabase(string directory) : base(directory, "BamDataReleaseCount")
        {
        }

        public int Released { get; private set; }

        public override void ReleaseConnection(DbConnection conn)
        {
            Released++;
            base.ReleaseConnection(conn);
        }
    }

    private sealed class Row
    {
        public long Id { get; set; }
    }

    private sealed record FailureOutcome(string Thrown, int Reported);

    private sealed record ReaderOutcome(string Reader, string Typed, string Table, int Reported);

    private sealed record ReleaseOutcome(int AfterSuccess, int AfterFailure);

    private sealed record SchemaOutcome(EnsureSchemaStatus First, EnsureSchemaStatus Again, string? RepeatFailure, string Direct);
}
