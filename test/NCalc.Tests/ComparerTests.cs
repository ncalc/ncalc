 using NCalc.Exceptions;
using NCalc.Helpers;

namespace NCalc.Tests;

[Property("Category", "Comparer")]
public class ComparerTests
{
    [Test]
    public async Task ShouldCompareAllGeneratedNumericTypesUsingMostPreciseType()
    {
        var cultureInfo = CultureInfo.InvariantCulture;

        await Assert.That(TypeHelper.CompareUsingMostPreciseType(1m, 1, StringComparer.Ordinal, cultureInfo))
            .IsEqualTo(ComparisonResult.Equal);
        await Assert.That(TypeHelper.CompareUsingMostPreciseType(1d, 2f, StringComparer.Ordinal, cultureInfo))
            .IsEqualTo(ComparisonResult.Less);
        await Assert.That(TypeHelper.CompareUsingMostPreciseType(1f, 2L, StringComparer.Ordinal, cultureInfo))
            .IsEqualTo(ComparisonResult.Less);
        await Assert.That(TypeHelper.CompareUsingMostPreciseType((ulong)3, 2L, StringComparer.Ordinal, cultureInfo))
            .IsEqualTo(ComparisonResult.Greater);
        await Assert.That(TypeHelper.CompareUsingMostPreciseType(3L, (uint)2, StringComparer.Ordinal, cultureInfo))
            .IsEqualTo(ComparisonResult.Greater);
        await Assert.That(TypeHelper.CompareUsingMostPreciseType((uint)1, 2, StringComparer.Ordinal, cultureInfo))
            .IsEqualTo(ComparisonResult.Less);
        await Assert.That(TypeHelper.CompareUsingMostPreciseType((ushort)1, (short)2, StringComparer.Ordinal, cultureInfo))
            .IsEqualTo(ComparisonResult.Less);
        await Assert.That(TypeHelper.CompareUsingMostPreciseType((short)1, (byte)2, StringComparer.Ordinal, cultureInfo))
            .IsEqualTo(ComparisonResult.Less);
        await Assert.That(TypeHelper.CompareUsingMostPreciseType((byte)2, (sbyte)2, StringComparer.Ordinal, cultureInfo))
            .IsEqualTo(ComparisonResult.Equal);
        await Assert.That(TypeHelper.CompareUsingMostPreciseType("2", (sbyte)2, StringComparer.Ordinal, cultureInfo))
            .IsEqualTo(ComparisonResult.Equal);
    }

    [Test]
    public async Task ExpressionDoesNotDefineNullParameterWithoutNullOption()
    {
        var e = new Expression("'a string' == null");

        var ex = Assert.Throws<NCalcParameterNotDefinedException>(() =>
            e.Evaluate(CancellationToken.None));
        await Assert.That(ex.Message).Contains("not defined");
    }
}
