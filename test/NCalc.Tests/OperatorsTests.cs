using NCalc.Factories;
using System.Threading.Tasks;

namespace NCalc.Tests;

[Property("Category", "Operators")]
public class OperatorsTests
{
    [Test]
    public async Task ShouldShortCircuitCoalesceOperator()
    {
        var expression = new Expression("[foo] ?? fallback()")
        {
            Parameters = { ["foo"] = "value" },
            Functions = { ["fallback"] = _ => throw new InvalidOperationException() }
        };

        await Assert.That(expression.Evaluate(CancellationToken.None)).IsEqualTo("value");
    }

    [Test]
    public async Task ShouldRespectCoalesceAssociativityAndPrecedence()
    {
        var logicalExpression = LogicalExpressionFactory.Create("[a] ?? [b] ?? [c]", cancellationToken: CancellationToken.None);
        await Assert.That(logicalExpression).IsTypeOf<BinaryExpression>();
        await Assert.That(((BinaryExpression)logicalExpression).Type).IsEqualTo(BinaryExpressionType.Coalesce);
        await Assert.That(((BinaryExpression)logicalExpression).RightExpression)
            .IsTypeOf<BinaryExpression>();
    }

    [Test]
    public async Task ShouldAllowBinaryHandlerToOverrideCoalesceOperator()
    {
        var expression = new Expression("[foo] ?? false")
        {
            Parameters = { ["foo"] = null }
        };
        expression.EvaluateBinary += args =>
        {
            if (args.BinaryExpression.Type == BinaryExpressionType.Coalesce)
                args.Result = "handled";
        };

        await Assert.That(expression.Evaluate(CancellationToken.None)).IsEqualTo("handled");
    }

    [Test]
    public async Task ShouldEvaluateNullCoalesceOperandOnlyOnce()
    {
        var evaluations = 0;
        var expression = new Expression("[foo] ?? true");
        expression.DynamicParameters["foo"] = _ =>
        {
            evaluations++;
            return null;
        };
        expression.EvaluateBinary += args => _ = args.LeftValue();

        await Assert.That((bool)expression.Evaluate(CancellationToken.None)!).IsTrue();
        await Assert.That(evaluations).IsEqualTo(1);
    }

    [Test]
    public async Task Should_Preserve_Signed_Zero_For_Unary_Negation()
    {
        var expression = new Expression("-amount")
        {
            Parameters =
            {
                ["amount"] = 0.0
            }
        };

        await Assert.That(BitConverter.DoubleToInt64Bits((double)expression.Evaluate(CancellationToken.None)!))
            .IsEqualTo(BitConverter.DoubleToInt64Bits(-0.0d));
    }
}
