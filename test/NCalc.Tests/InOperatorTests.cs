namespace NCalc.Tests;

[Property("Category", "Evaluations")]
public class InOperatorTests
{
    [Test]
    public async Task ShouldEvaluateInOperatorWithList(CancellationToken cancellationToken)
    {
        var context = new ExpressionContext();
        context.StaticParameters["PageState"] = "Insert";
        await Assert.That(new Expression("{PageState} in ('Insert','Update')", context)
            .Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateInOperatorWithString(CancellationToken cancellationToken)
    {
        var context = new ExpressionContext();
        context.StaticParameters["PageState"] = "Insert";

        await Assert.That(new Expression("{PageState} in 'Insert a quote, you must.'", context)
            .Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateNotInOperator(CancellationToken cancellationToken)
    {
        var context = new ExpressionContext();
        context.StaticParameters["PageState"] = "Import";
        await Assert.That(new Expression("{PageState} not in  ('Insert','Update')", context)
            .Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task InOperatorShouldRespectStringComparer(CancellationToken cancellationToken)
    {
        ExpressionContext context = ExpressionOptions.CaseInsensitiveStringComparer;
        context.StaticParameters["PageState"] = "Insert";
        await Assert.That(new Expression("{PageState} in ('INSERT','UPDATE')", context)
            .Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateTrueInOperatorWithObjects(CancellationToken cancellationToken)
    {
        await Assert.That(new Expression("{tap_int_status} in (5)")
        {
            Parameters = { { "tap_int_status", 5 } }
        }.Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateFalseInOperatorWithObjects(CancellationToken cancellationToken)
    {
        await Assert.That(new Expression("{PageState} in 4")
        {
            Parameters = { { "PageState", "Insert" } }
        }.Evaluate(cancellationToken)).IsEqualTo(false);
    }

    [Test]
    public async Task ShouldEvaluateIntInOperatorWithParameters(CancellationToken cancellationToken)
    {
        const int x = 3;
        int[] y = [1, 2, 3];

        var expression = new Expression("{x} in {y}", ExpressionOptions.None);
        expression.Parameters["x"] = x;
        expression.Parameters["y"] = y;

        await Assert.That((bool)expression.Evaluate(cancellationToken)).IsTrue();
    }

    [Test]
    public async Task ShouldEvaluateStringInOperatorWithIntParameters(CancellationToken cancellationToken)
    {
        const string x = "3";
        int[] y = [1, 2, 3];

        var expression = new Expression("{x} in {y}", ExpressionOptions.None);
        expression.Parameters["x"] = x;
        expression.Parameters["y"] = y;

        await Assert.That((bool)expression.Evaluate(cancellationToken)).IsTrue();
    }
}