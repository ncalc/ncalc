using NCalc.Exceptions;
using System.Threading.Tasks;

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