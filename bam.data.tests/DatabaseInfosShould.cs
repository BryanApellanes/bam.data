using System.Reflection;
using System.Runtime.CompilerServices;
using Bam.Data;
using Bam.Data.Npgsql;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

/// <summary>
/// Covers the <see cref="Database.Infos"/> registry and connection-string handling: concurrent construction
/// (bam.data#23), no connection strings and bounded growth (bam.data#22), credential-free messages (bam.data#24) and
/// null-safe equality.
/// </summary>
[UnitTestMenu("Database infos should")]
public class DatabaseInfosShould : UnitTestMenuContainer
{
    private const string Secret = "TopSecretPassword";

    public DatabaseInfosShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void RegisterEveryDatabaseConstructedConcurrently()
    {
        When.A<int>("constructs databases on many threads at once",
            () => 64 * 50,
            count =>
            {
                string run = Guid.NewGuid().ToString("N");
                NpgsqlDatabase[] built = new NpgsqlDatabase[count];
                Exception? failure = null;
                try
                {
                    Parallel.For(0, count, new ParallelOptions { MaxDegreeOfParallelism = 64 },
                        index => built[index] = new NpgsqlDatabase($"Host=db;Database=d{index};Username=u;Password={Secret}"));
                }
                catch (Exception exception)
                {
                    failure = exception;
                }
                for (int index = 0; index < count; index++)
                {
                    if (built[index] is not null)
                    {
                        built[index].ConnectionName = $"{run}-{index}";
                    }
                }
                HashSet<string> registered = new HashSet<string>(Database.Infos.Select(info => info.ConnectionName ?? string.Empty));
                int missing = Enumerable.Range(0, count).Count(index => !registered.Contains($"{run}-{index}"));
                GC.KeepAlive(built);
                return new ConcurrencyOutcome(failure?.ToString(), missing);
            })
            .TheTest
            .ShouldPass<ConcurrencyOutcome>((because, outcome) =>
            {
                because.ItsTrue($"no construction throws ({outcome.Failure})", outcome.Failure is null);
                because.ItsTrue($"every database is registered ({outcome.Missing} missing)", outcome.Missing == 0);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void ExposeNoConnectionString()
    {
        When.A<NpgsqlDatabase>("registers a database whose connection string carries a password",
            () => new NpgsqlDatabase($"Host=db;Database=d;Username=u;Password={Secret}") { ConnectionName = "infos-secret-" + Guid.NewGuid().ToString("N") },
            database =>
            {
                DatabaseInfo info = Database.Infos.Single(candidate => candidate.ConnectionName == database.ConnectionName);
                string[] values = typeof(DatabaseInfo)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(property => property.GetValue(info)?.ToString() ?? string.Empty)
                    .Append(info.ToString())
                    .ToArray();
                GC.KeepAlive(database);
                return values;
            })
            .TheTest
            .ShouldPass<string[]>((because, values) =>
            {
                because.ItsTrue("no public value carries the password", values.All(value => !value.Contains(Secret)));
                because.ItsTrue("no public value carries the connection string", values.All(value => !value.Contains("Host=db")));
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void LetAnUnreferencedDatabaseDropOut()
    {
        When.A<int>("registers a database, drops every reference to it and collects",
            () => 0,
            _ =>
            {
                WeakReference reference = Construct();
                for (int attempt = 0; attempt < 10 && reference.IsAlive; attempt++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
                return reference.IsAlive;
            })
            .TheTest
            .ShouldPass<bool>((because, alive) => because.ItsTrue("the registered database was collected", !alive))
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void KeepTheConnectionStringOutOfTheNameError()
    {
        When.A<NpgsqlDatabase>("asks for the name of a database whose connection string names none",
            () => new NpgsqlDatabase($"Host=secret-host;Username=u;Password={Secret}"),
            database =>
            {
                try
                {
                    return "no exception: " + database.Name;
                }
                catch (InvalidOperationException exception)
                {
                    return exception.Message;
                }
            })
            .TheTest
            .ShouldPass<string>((because, message) =>
            {
                because.ItsTrue($"the name lookup fails ({message})", message.StartsWith("Unable to determine the database name"));
                because.ItsTrue("the message carries no password", !message.Contains(Secret));
                because.ItsTrue("the message carries no connection-string value", !message.Contains("secret-host"));
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void CompareDatabasesWithoutThrowing()
    {
        When.A<NpgsqlDatabase>("compares a database with null, another type and a database without a connection string",
            () => new NpgsqlDatabase($"Host=db;Database=d;Username=u;Password={Secret}"),
            database => new EqualityOutcome(
                database.Equals(null),
                database.Equals("Host=db"),
                database.Equals(new NpgsqlDatabase()),
                database.Equals(new NpgsqlDatabase($"Host=db;Database=d;Username=u;Password={Secret}"))))
            .TheTest
            .ShouldPass<EqualityOutcome>((because, outcome) =>
            {
                because.ItsTrue("null is not equal", !outcome.Null);
                because.ItsTrue("another type is not equal", !outcome.OtherType);
                because.ItsTrue("a database without a connection string is not equal", !outcome.NoConnectionString);
                because.ItsTrue("the same connection string is equal", outcome.Same);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference Construct()
    {
        return new WeakReference(new NpgsqlDatabase($"Host=db;Database=d;Username=u;Password={Secret}"));
    }

    private sealed record ConcurrencyOutcome(string? Failure, int Missing);

    private sealed record EqualityOutcome(bool Null, bool OtherType, bool NoConnectionString, bool Same);
}
