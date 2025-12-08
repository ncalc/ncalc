using NCalc.Exceptions;

namespace NCalc.Tests;

[Property("Category", "Comparer")]
public class ComparerTests
{
    [Test]
    public async Task Should_Use_Case_Insensitive_Comparer_Issue_85(CancellationToken cancellationToken)
    {
        var eif = new Expression("PageState == 'LIST'", ExpressionOptions.CaseInsensitiveStringComparer);
        eif.Parameters["PageState"] = "List";

        await Assert.That((bool)eif.Evaluate(cancellationToken)).IsTrue();
    }

    [Test]
    [Arguments(null, 2, false)]
    [Arguments(2, 2L, true)]
    [Arguments("Hello", "World", false)]
    public async Task Compare_Using_Most_Precise_Type_Issue_102(object a, object b, bool expectedResult, CancellationToken cancellationToken)
    {
        var issueExp = new Expression("a == b")
        {
            Parameters =
            {
                ["a"] = a,
                ["b"] = b
            }
        };

        await Assert.That((bool)issueExp.Evaluate(cancellationToken)).IsEqualTo(expectedResult);
    }

    [Test]
    public async Task ExpressionShouldHandleNullRightParameters(CancellationToken cancellationToken)
    {
        var e = new Expression("'a string' == null", ExpressionOptions.AllowNullParameter);
        await Assert.That((bool)e.Evaluate(cancellationToken)).IsFalse();
    }

    [Test]
    public async Task ExpressionShouldHandleNullLeftParameters(CancellationToken cancellationToken)
    {
        var e = new Expression("null == 'a string'", ExpressionOptions.AllowNullParameter);
        await Assert.That((bool)e.Evaluate(cancellationToken)).IsFalse();
    }

    [Test]
    public async Task ExpressionShouldHandleNullBothParameters(CancellationToken cancellationToken)
    {
        var e = new Expression("null == null", ExpressionOptions.AllowNullParameter);
        await Assert.That((bool)e.Evaluate(cancellationToken)).IsTrue();
    }

    [Test]
    public async Task ShouldCompareNullToNull(CancellationToken cancellationToken)
    {
        var e = new Expression("[x] = null", ExpressionOptions.AllowNullParameter);
        e.Parameters["x"] = null;

        await Assert.That((bool)e.Evaluate(cancellationToken)).IsTrue();
    }

    [Test]
    public async Task ShouldCompareNullableToNonNullable(CancellationToken cancellationToken)
    {
        var e = new Expression("[x] = 5", ExpressionOptions.AllowNullParameter);

        e.Parameters["x"] = (int?)5;
        await Assert.That((bool)e.Evaluate(cancellationToken)).IsTrue();

        e.Parameters["x"] = (int?)6;
        await Assert.That((bool)e.Evaluate(cancellationToken)).IsFalse();
    }

    [Test]
    public async Task ShouldCompareNullableNullToNonNullable(CancellationToken cancellationToken)
    {
        var e = new Expression("[x] = 5", ExpressionOptions.AllowNullParameter);

        e.Parameters["x"] = null;
        await Assert.That((bool)e.Evaluate(cancellationToken)).IsFalse();
    }

    [Test]
    public async Task ShouldCompareNullToString(CancellationToken cancellationToken)
    {
        var e = new Expression("[x] = 'foo'", ExpressionOptions.AllowNullParameter);
        e.Parameters["x"] = null;

        await Assert.That((bool)e.Evaluate(cancellationToken)).IsFalse();
    }

    [Test]
    public async Task ExpressionDoesNotDefineNullParameterWithoutNullOption(CancellationToken cancellationToken)
    {
        var e = new Expression("'a string' == null");

        var ex = await Assert.That(() =>
            e.Evaluate(cancellationToken)).ThrowsExactly<NCalcParameterNotDefinedException>();
        await Assert.That(ex.Message).Contains("not defined");
    }

    [Test]
    [Arguments("(10/null)<0")]
    [Arguments("(10/null)>0")]
    public async Task CompareWithNullShouldReturnFalse(string expression, CancellationToken cancellationToken)
    {
        var e = new Expression(expression, ExpressionOptions.AllowNullParameter);
        e.Parameters["x"] = null;

        await Assert.That((bool)e.Evaluate(cancellationToken)).IsFalse();
    }

    [Test]
    public async Task ShouldCompareInequlityWithStrictTypeMatching(CancellationToken cancellationToken)
    {
        var e = new Expression("1 != ''",
            ExpressionOptions.NoStringTypeCoercion | ExpressionOptions.StrictTypeMatching);

        await Assert.That((bool)e.Evaluate(cancellationToken)).IsTrue();
    }

    [Test]
    public async Task ShouldCompareWithEmptyString(CancellationToken cancellationToken)
    {
        var e = new Expression("1 == ''");
        await Assert.That((bool)e.Evaluate(cancellationToken)).IsFalse();
    }
}