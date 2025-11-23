namespace NCalc.Tests;

[Trait("Category", "Evaluations")]
public class InOperatorTests
{
    [Fact]
    public void ShouldEvaluateInOperatorWithList()
    {
        var context = new ExpressionContext();
        context.StaticParameters["PageState"] = "Insert";
        Assert.Equal(true, new Expression("{PageState} in ('Insert','Update')", context)
            .Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldEvaluateInOperatorWithString()
    {
        var context = new ExpressionContext();
        context.StaticParameters["PageState"] = "Insert";

        Assert.Equal(true, new Expression("{PageState} in 'Insert a quote, you must.'", context)
            .Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldEvaluateNotInOperator()
    {
        var context = new ExpressionContext();
        context.StaticParameters["PageState"] = "Import";
        Assert.Equal(true, new Expression("{PageState} not in  ('Insert','Update')", context)
            .Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void InOperatorShouldRespectStringComparer()
    {
        ExpressionContext context = ExpressionOptions.CaseInsensitiveStringComparer;
        context.StaticParameters["PageState"] = "Insert";
        Assert.Equal(true, new Expression("{PageState} in ('INSERT','UPDATE')", context)
            .Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldEvaluateTrueInOperatorWithObjects()
    {
        Assert.Equal(true, new Expression("{tap_int_status} in (5)")
        {
            Parameters = { { "tap_int_status", 5 } }
        }.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldEvaluateFalseInOperatorWithObjects()
    {
        Assert.Equal(false, new Expression("{PageState} in 4")
        {
            Parameters = { { "PageState", "Insert" } }
        }.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldEvaluateIntInOperatorWithParameters()
    {
        var x = 3;
        int[] y = [1, 2, 3];

        var expression = new Expression("{x} in {y}", ExpressionOptions.None);
        expression.Parameters["x"] = x;
        expression.Parameters["y"] = y;

        Assert.True((bool)expression.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldEvaluateStringInOperatorWithIntParameters()
    {
        var x = "3";
        int[] y = [1, 2, 3];

        var expression = new Expression("{x} in {y}", ExpressionOptions.None);
        expression.Parameters["x"] = x;
        expression.Parameters["y"] = y;

        Assert.True((bool)expression.Evaluate(TestContext.Current.CancellationToken));
    }
}