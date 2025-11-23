using NCalc.Exceptions;

namespace NCalc.Tests;

[Trait("Category", "Comparer")]
public class ComparerTests
{
    [Fact]
    public void Should_Use_Case_Insensitive_Comparer_Issue_85()
    {
        var eif = new Expression("PageState == 'LIST'", ExpressionOptions.CaseInsensitiveStringComparer);
        eif.Parameters["PageState"] = "List";

        Assert.True((bool)eif.Evaluate(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(null, 2, false)]
    [InlineData(2, 2L, true)]
    [InlineData("Hello", "World", false)]
    public void Compare_Using_Most_Precise_Type_Issue_102(object a, object b, bool expectedResult)
    {
        var issueExp = new Expression("a == b")
        {
            Parameters =
            {
                ["a"] = a,
                ["b"] = b
            }
        };

        Assert.Equal(expectedResult, (bool)issueExp.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ExpressionShouldHandleNullRightParameters()
    {
        var e = new Expression("'a string' == null", ExpressionOptions.AllowNullParameter);
        Assert.False((bool)e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ExpressionShouldHandleNullLeftParameters()
    {
        var e = new Expression("null == 'a string'", ExpressionOptions.AllowNullParameter);
        Assert.False((bool)e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ExpressionShouldHandleNullBothParameters()
    {
        var e = new Expression("null == null", ExpressionOptions.AllowNullParameter);
        Assert.True((bool)e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldCompareNullToNull()
    {
        var e = new Expression("[x] = null", ExpressionOptions.AllowNullParameter);
        e.Parameters["x"] = null;

        Assert.True((bool)e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldCompareNullableToNonNullable()
    {
        var e = new Expression("[x] = 5", ExpressionOptions.AllowNullParameter);

        e.Parameters["x"] = (int?)5;
        Assert.True((bool)e.Evaluate(TestContext.Current.CancellationToken));

        e.Parameters["x"] = (int?)6;
        Assert.False((bool)e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldCompareNullableNullToNonNullable()
    {
        var e = new Expression("[x] = 5", ExpressionOptions.AllowNullParameter);

        e.Parameters["x"] = null;
        Assert.False((bool)e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldCompareNullToString()
    {
        var e = new Expression("[x] = 'foo'", ExpressionOptions.AllowNullParameter);
        e.Parameters["x"] = null;

        Assert.False((bool)e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ExpressionDoesNotDefineNullParameterWithoutNullOption()
    {
        var e = new Expression("'a string' == null");

        var ex = Assert.Throws<NCalcParameterNotDefinedException>(() =>
            e.Evaluate(TestContext.Current.CancellationToken));
        Assert.Contains("not defined", ex.Message);
    }

    [Theory]
    [InlineData("(10/null)<0")]
    [InlineData("(10/null)>0")]
    public void CompareWithNullShouldReturnFalse(string expression)
    {
        var e = new Expression(expression, ExpressionOptions.AllowNullParameter);
        e.Parameters["x"] = null;

        Assert.False((bool)e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldCompareInequlityWithStrictTypeMatching()
    {
        var e = new Expression("1 != ''",
            ExpressionOptions.NoStringTypeCoercion | ExpressionOptions.StrictTypeMatching);

        Assert.True((bool)e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldCompareWithEmptyString()
    {
        var e = new Expression("1 == ''");
        Assert.False((bool)e.Evaluate(TestContext.Current.CancellationToken));
    }
}