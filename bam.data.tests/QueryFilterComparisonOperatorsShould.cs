using Bam.Data;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

[UnitTestMenu("QueryFilter comparison operators should")]
public class QueryFilterComparisonOperatorsShould : UnitTestMenuContainer
{
    private const string Column = "Age";

    public QueryFilterComparisonOperatorsShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void RenderInclusiveLessThanOrEqualForQueryValue()
    {
        When.A<QueryFilter>("renders the inclusive <= comparator for the QueryValue overload",
            Query.Where(Column),
            (filter) => ComparatorToken(filter <= Query.Value(21)))
        .TheTest
        .ShouldPass<string>((because, _, token) =>
        {
            because.ItsTrue("the rendered comparator is '<=' so boundary-equal rows are included", token.Equals("<="));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderInclusiveGreaterThanOrEqualForQueryValue()
    {
        When.A<QueryFilter>("renders the inclusive >= comparator for the QueryValue overload",
            Query.Where(Column),
            (filter) => ComparatorToken(filter >= Query.Value(21)))
        .TheTest
        .ShouldPass<string>((because, _, token) =>
        {
            because.ItsTrue("the rendered comparator is '>=' so boundary-equal rows are included", token.Equals(">="));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderExpectedComparatorTokenForEveryOperatorOverloadVariant()
    {
        When.A<QueryFilter>("renders the expected comparison token for every operator overload variant",
            new QueryFilter(Column),
            (_) => BuildComparatorMatrix())
        .TheTest
        .ShouldPass<List<ComparatorTokensOutcome>>((because, _, outcomes) =>
        {
            because.ItsTrue("all 14 overload variants were exercised", outcomes.Count == 14);
            foreach (ComparatorTokensOutcome outcome in outcomes)
            {
                because.ItsTrue($"{outcome.Variant}: == renders '='", outcome.Equal.Equals("="));
                because.ItsTrue($"{outcome.Variant}: != renders '<>'", outcome.NotEqual.Equals("<>"));
                because.ItsTrue($"{outcome.Variant}: < renders '<'", outcome.LessThan.Equals("<"));
                because.ItsTrue($"{outcome.Variant}: > renders '>'", outcome.GreaterThan.Equals(">"));
                because.ItsTrue($"{outcome.Variant}: <= renders '<='", outcome.LessThanOrEqual.Equals("<="));
                because.ItsTrue($"{outcome.Variant}: >= renders '>='", outcome.GreaterThanOrEqual.Equals(">="));
            }
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderEqualityTokensForBoolOverloads()
    {
        When.A<QueryFilter>("renders equality comparators for the bool overloads",
            new QueryFilter(Column),
            (_) => new BoolTokensOutcome(
                ComparatorToken(Query.Where(Column) == true),
                ComparatorToken(Query.Where(Column) != true)))
        .TheTest
        .ShouldPass<BoolTokensOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("bool: == renders '='", outcome.Equal.Equals("="));
            because.ItsTrue("bool: != renders '<>'", outcome.NotEqual.Equals("<>"));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    private static List<ComparatorTokensOutcome> BuildComparatorMatrix()
    {
        DateTime date = new DateTime(2026, 1, 1);
        return new List<ComparatorTokensOutcome>
        {
            Tokens("QueryValue",
                Query.Where(Column) == Query.Value(21),
                Query.Where(Column) != Query.Value(21),
                Query.Where(Column) < Query.Value(21),
                Query.Where(Column) > Query.Value(21),
                Query.Where(Column) <= Query.Value(21),
                Query.Where(Column) >= Query.Value(21)),
            Tokens("int",
                Query.Where(Column) == 21,
                Query.Where(Column) != 21,
                Query.Where(Column) < 21,
                Query.Where(Column) > 21,
                Query.Where(Column) <= 21,
                Query.Where(Column) >= 21),
            Tokens("uint",
                Query.Where(Column) == (uint)21,
                Query.Where(Column) != (uint)21,
                Query.Where(Column) < (uint)21,
                Query.Where(Column) > (uint)21,
                Query.Where(Column) <= (uint)21,
                Query.Where(Column) >= (uint)21),
            Tokens("long",
                Query.Where(Column) == 21L,
                Query.Where(Column) != 21L,
                Query.Where(Column) < 21L,
                Query.Where(Column) > 21L,
                Query.Where(Column) <= 21L,
                Query.Where(Column) >= 21L),
            Tokens("ulong",
                Query.Where(Column) == (ulong)21,
                Query.Where(Column) != (ulong)21,
                Query.Where(Column) < (ulong)21,
                Query.Where(Column) > (ulong)21,
                Query.Where(Column) <= (ulong)21,
                Query.Where(Column) >= (ulong)21),
            Tokens("decimal",
                Query.Where(Column) == 21m,
                Query.Where(Column) != 21m,
                Query.Where(Column) < 21m,
                Query.Where(Column) > 21m,
                Query.Where(Column) <= 21m,
                Query.Where(Column) >= 21m),
            Tokens("int?",
                Query.Where(Column) == (int?)21,
                Query.Where(Column) != (int?)21,
                Query.Where(Column) < (int?)21,
                Query.Where(Column) > (int?)21,
                Query.Where(Column) <= (int?)21,
                Query.Where(Column) >= (int?)21),
            Tokens("uint?",
                Query.Where(Column) == (uint?)21,
                Query.Where(Column) != (uint?)21,
                Query.Where(Column) < (uint?)21,
                Query.Where(Column) > (uint?)21,
                Query.Where(Column) <= (uint?)21,
                Query.Where(Column) >= (uint?)21),
            Tokens("long?",
                Query.Where(Column) == (long?)21,
                Query.Where(Column) != (long?)21,
                Query.Where(Column) < (long?)21,
                Query.Where(Column) > (long?)21,
                Query.Where(Column) <= (long?)21,
                Query.Where(Column) >= (long?)21),
            Tokens("ulong?",
                Query.Where(Column) == (ulong?)21,
                Query.Where(Column) != (ulong?)21,
                Query.Where(Column) < (ulong?)21,
                Query.Where(Column) > (ulong?)21,
                Query.Where(Column) <= (ulong?)21,
                Query.Where(Column) >= (ulong?)21),
            Tokens("decimal?",
                Query.Where(Column) == (decimal?)21,
                Query.Where(Column) != (decimal?)21,
                Query.Where(Column) < (decimal?)21,
                Query.Where(Column) > (decimal?)21,
                Query.Where(Column) <= (decimal?)21,
                Query.Where(Column) >= (decimal?)21),
            Tokens("string",
                Query.Where(Column) == "21",
                Query.Where(Column) != "21",
                Query.Where(Column) < "21",
                Query.Where(Column) > "21",
                Query.Where(Column) <= "21",
                Query.Where(Column) >= "21"),
            Tokens("DateTime",
                Query.Where(Column) == date,
                Query.Where(Column) != date,
                Query.Where(Column) < date,
                Query.Where(Column) > date,
                Query.Where(Column) <= date,
                Query.Where(Column) >= date),
            Tokens("DateTime?",
                Query.Where(Column) == (DateTime?)date,
                Query.Where(Column) != (DateTime?)date,
                Query.Where(Column) < (DateTime?)date,
                Query.Where(Column) > (DateTime?)date,
                Query.Where(Column) <= (DateTime?)date,
                Query.Where(Column) >= (DateTime?)date)
        };
    }

    private static ComparatorTokensOutcome Tokens(string variant, QueryFilter equal, QueryFilter notEqual, QueryFilter lessThan, QueryFilter greaterThan, QueryFilter lessThanOrEqual, QueryFilter greaterThanOrEqual)
    {
        return new ComparatorTokensOutcome(
            variant,
            ComparatorToken(equal),
            ComparatorToken(notEqual),
            ComparatorToken(lessThan),
            ComparatorToken(greaterThan),
            ComparatorToken(lessThanOrEqual),
            ComparatorToken(greaterThanOrEqual));
    }

    private static string ComparatorToken(QueryFilter filter)
    {
        return filter.Filters.Single().Operator;
    }

    private sealed record ComparatorTokensOutcome(string Variant, string Equal, string NotEqual, string LessThan, string GreaterThan, string LessThanOrEqual, string GreaterThanOrEqual);

    private sealed record BoolTokensOutcome(string Equal, string NotEqual);
}
