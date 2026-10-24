using Bam.Data;
using Bam.Data.Postgres;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

[UnitTestMenu("Database infos should")]
public class DatabaseInfosShould : UnitTestMenuContainer
{
    public DatabaseInfosShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void RecordDatabasesConstructedOnManyThreadsAtOnce()
    {
        When.A<int>("constructs 20,000 Postgres databases across 64 threads at once (bam.data#23)",
            20000,
            (count) =>
            {
                string marker = "infos-" + Guid.NewGuid().ToString("N");
                int failures = 0;
                Parallel.For(0, count, new ParallelOptions { MaxDegreeOfParallelism = 64 }, index =>
                {
                    try
                    {
                        // No connection is opened: constructing a database only records it.
                        PostgresDatabase unused = new PostgresDatabase($"Host=127.0.0.1;Port=1;Database={marker}-{index};Username=nobody;Password=x", $"{marker}-{index}");
                    }
                    catch (InvalidOperationException)
                    {
                        Interlocked.Increment(ref failures);
                    }
                });
                int recorded = Database.InfosSnapshot().Count(info => info.ConnectionName?.StartsWith(marker, StringComparison.Ordinal) == true);
                return new ConcurrencyOutcome(failures, recorded, count);
            })
        .TheTest
        .ShouldPass<ConcurrencyOutcome>((because, outcome) =>
        {
            because.ItsTrue("no construction fails on a corrupted set", outcome.Failures == 0);
            because.ItsTrue($"every database is recorded: {outcome.Recorded} of {outcome.Expected}", outcome.Recorded == outcome.Expected);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    private sealed record ConcurrencyOutcome(int Failures, int Recorded, int Expected);
}
