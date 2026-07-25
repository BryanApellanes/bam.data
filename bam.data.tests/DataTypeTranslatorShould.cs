using Bam.Data;
using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

[UnitTestMenu("DataTypeTranslator should")]
public class DataTypeTranslatorShould : UnitTestMenuContainer
{
    public DataTypeTranslatorShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void MapNullableValueTypesToTheirUnderlyingDataType()
    {
        When.A<DataTypeTranslator>("maps nullable value types to the same DataTypes as their underlying type",
            new DataTypeTranslator(),
            (translator) => new NullableMappingOutcome(
                translator.EnumFromType(typeof(DateTime?)),
                translator.EnumFromType(typeof(DateTime)),
                translator.EnumFromType(typeof(int?)),
                translator.EnumFromType(typeof(int)),
                translator.EnumFromType(typeof(bool?)),
                translator.EnumFromType(typeof(long?))))
        .TheTest
        .ShouldPass<NullableMappingOutcome>((because, outcome) =>
        {
            because.ItsTrue("DateTime? maps to DateTime, not Default (BryanApellanes/bam.data#8)", outcome.NullableDateTime == DataTypes.DateTime);
            because.ItsTrue("DateTime? matches the non-nullable DateTime mapping", outcome.NullableDateTime == outcome.NonNullableDateTime);
            because.ItsTrue("int? maps to Int", outcome.NullableInt == DataTypes.Int);
            because.ItsTrue("int? matches the non-nullable int mapping", outcome.NullableInt == outcome.NonNullableInt);
            because.ItsTrue("bool? maps to Boolean", outcome.NullableBool == DataTypes.Boolean);
            because.ItsTrue("long? maps to Long", outcome.NullableLong == DataTypes.Long);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    private sealed record NullableMappingOutcome(
        DataTypes NullableDateTime,
        DataTypes NonNullableDateTime,
        DataTypes NullableInt,
        DataTypes NonNullableInt,
        DataTypes NullableBool,
        DataTypes NullableLong);
}
