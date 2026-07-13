 using NCalc.Exceptions;
using NCalc.Helpers;

namespace NCalc.Tests;

[Property("Category", "Comparer")]
public class ComparerTests
{
    [Test]
    public async Task Should_Use_Case_Insensitive_Comparer_Issue_85()
    {
        var eif = new Expression("PageState == 'LIST'", ExpressionOptions.CaseInsensitiveStringComparer);
        eif.Parameters["PageState"] = "List";

        await Assert.That((bool)eif.Evaluate(CancellationToken.None)).IsTrue();
    }

    [Test]
    [Arguments(null, 2, false)]
    [Arguments(2, 2L, true)]
    [Arguments("Hello", "World", false)]
    public async Task Compare_Using_Most_Precise_Type_Issue_102(object a, object b, bool expectedResult)
    {
        var issueExp = new Expression("a == b")
        {
            Parameters =
            {
                ["a"] = a,
                ["b"] = b
            }
        };

        await Assert.That((bool)issueExp.Evaluate(CancellationToken.None)).IsEqualTo(expectedResult);
    }

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
    public async Task ExpressionShouldHandleNullRightParameters()
    {
        var e = new Expression("'a string' == null", ExpressionOptions.AllowNullParameter);
        await Assert.That((bool)e.Evaluate(CancellationToken.None)).IsFalse();
    }

    [Test]
    public async Task ExpressionShouldHandleNullLeftParameters()
    {
        var e = new Expression("null == 'a string'", ExpressionOptions.AllowNullParameter);
        await Assert.That((bool)e.Evaluate(CancellationToken.None)).IsFalse();
    }

    [Test]
    public async Task ExpressionShouldHandleNullBothParameters()
    {
        var e = new Expression("null == null", ExpressionOptions.AllowNullParameter);
        await Assert.That((bool)e.Evaluate(CancellationToken.None)).IsTrue();
    }

    [Test]
    public async Task ShouldCompareNullToNull()
    {
        var e = new Expression("[x] = null", ExpressionOptions.AllowNullParameter);
        e.Parameters["x"] = null;

        await Assert.That((bool)e.Evaluate(CancellationToken.None)).IsTrue();
    }

    [Test]
    public async Task ShouldCompareNullableToNonNullable()
    {
        var e = new Expression("[x] = 5", ExpressionOptions.AllowNullParameter);

        e.Parameters["x"] = (int?)5;
        await Assert.That((bool)e.Evaluate(CancellationToken.None)).IsTrue();

        e.Parameters["x"] = (int?)6;
        await Assert.That((bool)e.Evaluate(CancellationToken.None)).IsFalse();
    }

    [Test]
    public async Task ShouldCompareNullableNullToNonNullable()
    {
        var e = new Expression("[x] = 5", ExpressionOptions.AllowNullParameter);

        e.Parameters["x"] = null;
        await Assert.That((bool)e.Evaluate(CancellationToken.None)).IsFalse();
    }

    [Test]
    public async Task ShouldCompareNullToString()
    {
        var e = new Expression("[x] = 'foo'", ExpressionOptions.AllowNullParameter);
        e.Parameters["x"] = null;

        await Assert.That((bool)e.Evaluate(CancellationToken.None)).IsFalse();
    }

    [Test]
    public async Task ExpressionDoesNotDefineNullParameterWithoutNullOption()
    {
        var e = new Expression("'a string' == null");

        var ex = Assert.Throws<NCalcParameterNotDefinedException>(() =>
            e.Evaluate(CancellationToken.None));
        await Assert.That(ex.Message).Contains("not defined");
    }

    [Test]
    [Arguments("(10/null)<0")]
    [Arguments("(10/null)>0")]
    public async Task CompareWithNullShouldReturnFalse(string expression)
    {
        var e = new Expression(expression, ExpressionOptions.AllowNullParameter);
        e.Parameters["x"] = null;

        await Assert.That((bool)e.Evaluate(CancellationToken.None)).IsFalse();
    }

    [Test]
    public async Task ShouldCompareInequlityWithStrictTypeMatching()
    {
        var e = new Expression("1 != ''",
            ExpressionOptions.NoStringTypeCoercion | ExpressionOptions.StrictTypeMatching);

        await Assert.That((bool)e.Evaluate(CancellationToken.None)).IsTrue();
    }

    [Test]
    public async Task ShouldCompareWithEmptyString()
    {
        var e = new Expression("1 == ''");
        await Assert.That((bool)e.Evaluate(CancellationToken.None)).IsFalse();
    }
}
