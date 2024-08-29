namespace NCalc.Tests;

[Trait("Category", "Evaluations")]
public class InOperatorTests
{
    [Fact]
    public void ShouldEvaluateInOperatorWithList()
    {
        var context = new ExpressionContext();
        context.StaticParameters["PageState"] = "Insert";
        Assert.Equal(true, new Expression("{PageState} in ('Insert','Update')", context).Evaluate());
    }

    [Fact]
    public void ShouldEvaluateInOperatorWithString()
    {
        var context = new ExpressionContext();
        context.StaticParameters["PageState"] = "Insert";

        Assert.Equal(true, new Expression("{PageState} in 'Insert a quote, you must.'", context).Evaluate());
    }

    [Fact]
    public void ShouldEvaluateNotInOperator()
    {
        var context = new ExpressionContext();
        context.StaticParameters["PageState"] = "Import";
        Assert.Equal(true, new Expression("{PageState} not in  ('Insert','Update')", context).Evaluate());
    }

    [Fact]
    public void InOperatorShouldRespectStringComparer()
    {
        ExpressionContext context = ExpressionOptions.CaseInsensitiveStringComparer;
        context.StaticParameters["PageState"] = "Insert";
        Assert.Equal(true, new Expression("{PageState} in ('INSERT','UPDATE')", context).Evaluate());
    }

    [Fact]
    public void ShouldEvaluateTrueInOperatorWithObjects()
    {
        Assert.Equal(true, new Expression("{tap_int_status} in (5)")
        {
            Parameters = { { "tap_int_status", 5 } }
        }.Evaluate());
    }

    [Fact]
    public void ShouldEvaluateFalseInOperatorWithObjects()
    {
        Assert.Equal(false, new Expression("{PageState} in 4")
        {
            Parameters = { { "PageState", 4 } }
        }.Evaluate());
    }
}