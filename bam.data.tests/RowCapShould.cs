using Bam.Data;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

[UnitTestMenu("SelectTop row cap should")]
public class RowCapShould : UnitTestMenuContainer
{
    public RowCapShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void RenderTSqlTopOnTheBaseBuilder()
    {
        When.A<SqlStringBuilder>("renders T-SQL TOP for SelectTop",
            new SqlStringBuilder(),
            (builder) => builder.SelectTop(5, "TestTable", "Name").ToString())
        .TheTest
        .ShouldPass<string>((because, _, sql) =>
        {
            because.ItsTrue("the SELECT clause carries TOP", sql.Equals("SELECT TOP 5 Name FROM [TestTable] "));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderTSqlTopOnTheMsSqlBuilder()
    {
        When.A<MsSqlSqlStringBuilder>("renders T-SQL TOP for SelectTop",
            new MsSqlSqlStringBuilder(),
            (builder) => builder.SelectTop(5, "TestTable", "Name").ToString())
        .TheTest
        .ShouldPass<string>((because, _, sql) =>
        {
            because.ItsTrue("the SELECT clause carries TOP", sql.Contains("SELECT TOP 5 Name FROM "));
            because.ItsTrue("no trailing LIMIT is rendered", !sql.Contains("LIMIT"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderTrailingLimitAfterWhereOnPostgres()
    {
        When.A<NpgsqlSqlStringBuilder>("renders a trailing LIMIT after the WHERE clause",
            new NpgsqlSqlStringBuilder(),
            (builder) =>
            {
                builder.SelectTop(5, "TestTable", "Name");
                builder.Where("Name", "test");
                return builder.ToString();
            })
        .TheTest
        .ShouldPass<string>((because, _, sql) =>
        {
            because.ItsTrue("no T-SQL TOP is rendered", !sql.Contains("TOP"));
            because.ItsTrue("a LIMIT clause is rendered", sql.Contains(" LIMIT 5"));
            because.ItsTrue("the LIMIT clause comes after the WHERE clause", sql.IndexOf("WHERE") < sql.IndexOf(" LIMIT 5"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderTrailingLimitOnMySql()
    {
        When.A<MySqlSqlStringBuilder>("renders a trailing LIMIT for SelectTop",
            new MySqlSqlStringBuilder(),
            (builder) => builder.SelectTop(3, "TestTable", "Name").ToString())
        .TheTest
        .ShouldPass<string>((because, _, sql) =>
        {
            because.ItsTrue("no T-SQL TOP is rendered", !sql.Contains("TOP"));
            because.ItsTrue("the statement ends with the LIMIT clause", sql.EndsWith(" LIMIT 3"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderTrailingLimitOnSQLite()
    {
        When.A<SQLiteSqlStringBuilder>("renders a trailing LIMIT for SelectTop",
            new SQLiteSqlStringBuilder(),
            (builder) => builder.SelectTop(3, "TestTable", "Name").ToString())
        .TheTest
        .ShouldPass<string>((because, _, sql) =>
        {
            because.ItsTrue("no T-SQL TOP is rendered", !sql.Contains("TOP"));
            because.ItsTrue("the statement ends with the LIMIT clause", sql.EndsWith(" LIMIT 3"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderSelectFirstOnFirebird()
    {
        When.A<FirebirdSqlSqlStringBuilder>("renders FIRST inside the SELECT clause",
            new FirebirdSqlSqlStringBuilder(),
            (builder) => builder.SelectTop(7, "TestTable", "Name").ToString())
        .TheTest
        .ShouldPass<string>((because, _, sql) =>
        {
            because.ItsTrue("the SELECT clause carries FIRST", sql.StartsWith("SELECT FIRST 7 Name FROM "));
            because.ItsTrue("no T-SQL TOP is rendered", !sql.Contains("TOP"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderFetchFirstRowsOnlyOnOracle()
    {
        When.A<OracleSqlStringBuilder>("renders a trailing FETCH FIRST n ROWS ONLY",
            new OracleSqlStringBuilder(),
            (builder) => builder.SelectTop(4, "TestTable", "Name").ToString())
        .TheTest
        .ShouldPass<string>((because, _, sql) =>
        {
            because.ItsTrue("no T-SQL TOP is rendered", !sql.Contains(" TOP "));
            because.ItsTrue("the statement ends with the FETCH FIRST clause", sql.EndsWith(" FETCH FIRST 4 ROWS ONLY"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void FunnelTypedTopThroughTheProviderRowCap()
    {
        When.A<OracleSqlStringBuilder>("no longer throws for typed Top and renders its dialect",
            new OracleSqlStringBuilder(),
            (builder) => builder.Top<VectorTestTableDao>(2).ToString())
        .TheTest
        .ShouldPass<string>((because, _, sql) =>
        {
            because.ItsTrue("the statement ends with the FETCH FIRST clause", sql.EndsWith(" FETCH FIRST 2 ROWS ONLY"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void FlushThePendingCapExactlyOnceOnGo()
    {
        When.A<NpgsqlSqlStringBuilder>("flushes the pending cap once for Go then ToString",
            new NpgsqlSqlStringBuilder(),
            (builder) =>
            {
                builder.SelectTop(5, "TestTable", "Name");
                builder.Go();
                return builder.ToString();
            })
        .TheTest
        .ShouldPass<string>((because, _, sql) =>
        {
            int firstIndex = sql.IndexOf(" LIMIT 5");
            int lastIndex = sql.LastIndexOf(" LIMIT 5");
            because.ItsTrue("the LIMIT clause is rendered", firstIndex >= 0);
            because.ItsTrue("the LIMIT clause is rendered exactly once", firstIndex == lastIndex);
            because.ItsTrue("the LIMIT clause comes before the statement separator", sql.IndexOf(" LIMIT 5") < sql.IndexOf(";"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderThePendingCapIdempotently()
    {
        When.A<NpgsqlSqlStringBuilder>("renders the same SQL for repeated ToString calls",
            new NpgsqlSqlStringBuilder(),
            (builder) =>
            {
                builder.SelectTop(5, "TestTable", "Name");
                string firstRender = builder.ToString();
                string secondRender = builder.ToString();
                return new RenderPairOutcome(firstRender, secondRender);
            })
        .TheTest
        .ShouldPass<RenderPairOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("both renders are identical", outcome.FirstRender.Equals(outcome.SecondRender));
            because.ItsTrue("both renders end with the LIMIT clause", outcome.FirstRender.EndsWith(" LIMIT 5") && outcome.SecondRender.EndsWith(" LIMIT 5"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderNoCapForPlainSelect()
    {
        When.A<NpgsqlSqlStringBuilder>("renders no cap for a plain Select",
            new NpgsqlSqlStringBuilder(),
            (builder) => builder.Select("TestTable", "Name").ToString())
        .TheTest
        .ShouldPass<string>((because, _, sql) =>
        {
            because.ItsTrue("no LIMIT clause is rendered", !sql.Contains("LIMIT"));
            because.ItsTrue("no T-SQL TOP is rendered", !sql.Contains("TOP"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void ClearThePendingCapOnReset()
    {
        When.A<NpgsqlSqlStringBuilder>("clears a pending cap when Reset is called",
            new NpgsqlSqlStringBuilder(),
            (builder) =>
            {
                builder.SelectTop(5, "TestTable", "Name");
                builder.Reset();
                builder.Select("TestTable", "Name");
                return builder.ToString();
            })
        .TheTest
        .ShouldPass<string>((because, _, sql) =>
        {
            because.ItsTrue("no LIMIT clause survives the reset", !sql.Contains("LIMIT"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderNonSelectStatementsUnchanged()
    {
        When.A<SqlStringBuilder>("renders a plain Select exactly as before the row cap seam",
            new SqlStringBuilder(),
            (builder) => builder.Select("TestTable", "Name").ToString())
        .TheTest
        .ShouldPass<string>((because, _, sql) =>
        {
            because.ItsTrue("render parity is preserved", sql.Equals("SELECT Name FROM [TestTable] "));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    private sealed record RenderPairOutcome(string FirstRender, string SecondRender);
}
