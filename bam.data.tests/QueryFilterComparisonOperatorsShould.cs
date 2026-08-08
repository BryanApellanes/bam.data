using Bam.Data;
using Bam.Data.Tests.Dao;
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

    [UnitTest]
    public void RenderExpectedComparatorTokenForEveryGenericOperatorOverloadVariant()
    {
        When.A<QueryFilter<TestItemColumns>>("renders the expected comparison token for every QueryFilter<C> operator overload variant",
            new QueryFilter<TestItemColumns>(Column),
            (_) => BuildGenericComparatorMatrix())
        .TheTest
        .ShouldPass<List<ComparatorTokensOutcome>>((because, _, outcomes) =>
        {
            because.ItsTrue("all 12 QueryFilter<C> overload variants were exercised", outcomes.Count == 12);
            foreach (ComparatorTokensOutcome outcome in outcomes)
            {
                because.ItsTrue($"QueryFilter<C> {outcome.Variant}: == renders '='", outcome.Equal.Equals("="));
                because.ItsTrue($"QueryFilter<C> {outcome.Variant}: != renders '<>'", outcome.NotEqual.Equals("<>"));
                because.ItsTrue($"QueryFilter<C> {outcome.Variant}: < renders '<'", outcome.LessThan.Equals("<"));
                because.ItsTrue($"QueryFilter<C> {outcome.Variant}: > renders '>'", outcome.GreaterThan.Equals(">"));
                because.ItsTrue($"QueryFilter<C> {outcome.Variant}: <= renders '<='", outcome.LessThanOrEqual.Equals("<="));
                because.ItsTrue($"QueryFilter<C> {outcome.Variant}: >= renders '>='", outcome.GreaterThanOrEqual.Equals(">="));
            }
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RenderRelationalTokensForGenericObjectOverloads()
    {
        // QueryFilter<C>'s object overloads are relational-only (no ==/!=). A plain object value is used
        // deliberately: routing a QueryValue through them embeds the QueryValue itself (tracked as #12).
        When.A<QueryFilter<TestItemColumns>>("renders relational comparators for the QueryFilter<C> object overloads",
            new QueryFilter<TestItemColumns>(Column),
            (_) => new GenericObjectTokensOutcome(
                ComparatorToken(GenericWhere() < (object)21),
                ComparatorToken(GenericWhere() > (object)21),
                ComparatorToken(GenericWhere() <= (object)21),
                ComparatorToken(GenericWhere() >= (object)21)))
        .TheTest
        .ShouldPass<GenericObjectTokensOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("QueryFilter<C> object: < renders '<'", outcome.LessThan.Equals("<"));
            because.ItsTrue("QueryFilter<C> object: > renders '>'", outcome.GreaterThan.Equals(">"));
            because.ItsTrue("QueryFilter<C> object: <= renders '<='", outcome.LessThanOrEqual.Equals("<="));
            because.ItsTrue("QueryFilter<C> object: >= renders '>='", outcome.GreaterThanOrEqual.Equals(">="));
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

    private static List<ComparatorTokensOutcome> BuildGenericComparatorMatrix()
    {
        DateTime date = new DateTime(2026, 1, 1);
        return new List<ComparatorTokensOutcome>
        {
            Tokens("int",
                GenericWhere() == 21,
                GenericWhere() != 21,
                GenericWhere() < 21,
                GenericWhere() > 21,
                GenericWhere() <= 21,
                GenericWhere() >= 21),
            Tokens("uint",
                GenericWhere() == (uint)21,
                GenericWhere() != (uint)21,
                GenericWhere() < (uint)21,
                GenericWhere() > (uint)21,
                GenericWhere() <= (uint)21,
                GenericWhere() >= (uint)21),
            Tokens("long",
                GenericWhere() == 21L,
                GenericWhere() != 21L,
                GenericWhere() < 21L,
                GenericWhere() > 21L,
                GenericWhere() <= 21L,
                GenericWhere() >= 21L),
            Tokens("ulong",
                GenericWhere() == (ulong)21,
                GenericWhere() != (ulong)21,
                GenericWhere() < (ulong)21,
                GenericWhere() > (ulong)21,
                GenericWhere() <= (ulong)21,
                GenericWhere() >= (ulong)21),
            Tokens("decimal",
                GenericWhere() == 21m,
                GenericWhere() != 21m,
                GenericWhere() < 21m,
                GenericWhere() > 21m,
                GenericWhere() <= 21m,
                GenericWhere() >= 21m),
            Tokens("int?",
                GenericWhere() == (int?)21,
                GenericWhere() != (int?)21,
                GenericWhere() < (int?)21,
                GenericWhere() > (int?)21,
                GenericWhere() <= (int?)21,
                GenericWhere() >= (int?)21),
            Tokens("uint?",
                GenericWhere() == (uint?)21,
                GenericWhere() != (uint?)21,
                GenericWhere() < (uint?)21,
                GenericWhere() > (uint?)21,
                GenericWhere() <= (uint?)21,
                GenericWhere() >= (uint?)21),
            Tokens("ulong?",
                GenericWhere() == (ulong?)21,
                GenericWhere() != (ulong?)21,
                GenericWhere() < (ulong?)21,
                GenericWhere() > (ulong?)21,
                GenericWhere() <= (ulong?)21,
                GenericWhere() >= (ulong?)21),
            Tokens("decimal?",
                GenericWhere() == (decimal?)21,
                GenericWhere() != (decimal?)21,
                GenericWhere() < (decimal?)21,
                GenericWhere() > (decimal?)21,
                GenericWhere() <= (decimal?)21,
                GenericWhere() >= (decimal?)21),
            Tokens("string",
                GenericWhere() == "21",
                GenericWhere() != "21",
                GenericWhere() < "21",
                GenericWhere() > "21",
                GenericWhere() <= "21",
                GenericWhere() >= "21"),
            Tokens("DateTime",
                GenericWhere() == date,
                GenericWhere() != date,
                GenericWhere() < date,
                GenericWhere() > date,
                GenericWhere() <= date,
                GenericWhere() >= date),
            Tokens("DateTime?",
                GenericWhere() == (DateTime?)date,
                GenericWhere() != (DateTime?)date,
                GenericWhere() < (DateTime?)date,
                GenericWhere() > (DateTime?)date,
                GenericWhere() <= (DateTime?)date,
                GenericWhere() >= (DateTime?)date)
        };
    }

    // A concrete column token (not a bare QueryFilter<C>) because the ulong ==/!= overloads route through
    // ToQueryValue, which requires the KeyColumn property that generated DAO column types declare.
    private static QueryFilter<TestItemColumns> GenericWhere()
    {
        return new TestItemColumns(Column);
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

    private sealed record GenericObjectTokensOutcome(string LessThan, string GreaterThan, string LessThanOrEqual, string GreaterThanOrEqual);
}
