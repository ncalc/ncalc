using System.Threading.Tasks;

namespace NCalc.Tests;

[Property("Category", "Evaluations")]
public class InOperatorTests
{
    [Test]
    public async Task ShouldEvaluateInOperatorWithList()
    {
        var context = new ExpressionContext();
        context.StaticParameters["PageState"] = "Insert";
        await Assert.That(new Expression("{PageState} in ('Insert','Update')", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateInOperatorWithString()
    {
        var context = new ExpressionContext();
        context.StaticParameters["PageState"] = "Insert";

        await Assert.That(new Expression("{PageState} in 'Insert a quote, you must.'", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateNotInOperator()
    {
        var context = new ExpressionContext();
        context.StaticParameters["PageState"] = "Import";
        await Assert.That(new Expression("{PageState} not in  ('Insert','Update')", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task InOperatorShouldRespectStringComparer()
    {
        ExpressionContext context = ExpressionOptions.CaseInsensitiveStringComparer;
        context.StaticParameters["PageState"] = "Insert";
        await Assert.That(new Expression("{PageState} in ('INSERT','UPDATE')", context)
            .Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateTrueInOperatorWithObjects()
    {
        await Assert.That(new Expression("{tap_int_status} in (5)")
        {
            Parameters = { { "tap_int_status", 5 } }
        }.Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateFalseInOperatorWithObjects()
    {
        await Assert.That(new Expression("{PageState} in 4")
        {
            Parameters = { { "PageState", "Insert" } }
        }.Evaluate(CancellationToken.None)).IsEqualTo(false);
    }

    [Test]
    public async Task ShouldEvaluateIntInOperatorWithParameters()
    {
        var x = 3;
        int[] y = [1, 2, 3];

        var expression = new Expression("{x} in {y}", ExpressionOptions.None);
        expression.Parameters["x"] = x;
        expression.Parameters["y"] = y;

        await Assert.That((bool)expression.Evaluate(CancellationToken.None)).IsTrue();
    }

    [Test]
    public async Task ShouldEvaluateStringInOperatorWithIntParameters()
    {
        var x = "3";
        int[] y = [1, 2, 3];

        var expression = new Expression("{x} in {y}", ExpressionOptions.None);
        expression.Parameters["x"] = x;
        expression.Parameters["y"] = y;

        await Assert.That((bool)expression.Evaluate(CancellationToken.None)).IsTrue();
    }
}