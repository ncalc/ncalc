using NCalc.Factories;

namespace NCalc.Tests;

[Property("Category", "Operators")]
public class OperatorsTests
{
    [Test]
    [Arguments("NOT true")]
    [Arguments("not true")]
    public async Task Should_Evaluate_Not_Unary_Operator(string expression, CancellationToken cancellationToken)
    {
        var logicalExpression = LogicalExpressionFactory.Create(expression, ct: cancellationToken);
        var expr = new Expression(logicalExpression);
        await Assert.That((bool)expr.Evaluate(cancellationToken)!).IsFalse();
    }

    [Test]
    [Arguments("!true", false)]
    [Arguments("not false", true)]
    [Arguments("Not false", true)]
    [Arguments("NOT false", true)]
    [Arguments("-10", -10)]
    [Arguments("+20", 20)]
    [Arguments("2**-1", 0.5)]
    [Arguments("2**+2", 4.0)]
    [Arguments("2 * 3", 6)]
    [Arguments("6 / 2", 3d)]
    [Arguments("7 % 2", 1)]
    [Arguments("2 + 3", 5)]
    [Arguments("2 - 1", 1)]
    [Arguments("1 < 2", true)]
    [Arguments("1 > 2", false)]
    [Arguments("1 <= 2", true)]
    [Arguments("1 <= 1", true)]
    [Arguments("1 >= 2", false)]
    [Arguments("1 >= 1", true)]
    [Arguments("1 = 1", true)]
    [Arguments("1 == 1", true)]
    [Arguments("1 != 1", false)]
    [Arguments("1 <> 1", false)]
    [Arguments("1 & 1", 1UL)]
    [Arguments("1 | 1", 1UL)]
    [Arguments("1 ^ 1", 0UL)]
    [Arguments("~1", ~1UL)]
    [Arguments("2 >> 1", 1UL)]
    [Arguments("2 << 1", 4UL)]
    [Arguments("true && false", false)]
    [Arguments("True and False", false)]
    [Arguments("tRue aNd faLse", false)]
    [Arguments("TRUE ANd fALSE", false)]
    [Arguments("true AND FALSE", false)]
    [Arguments("true || false", true)]
    [Arguments("true or false", true)]
    [Arguments("true Or false", true)]
    [Arguments("true OR false", true)]
    [Arguments("if(true, 0, 1)", 0)]
    [Arguments("if(false, 0, 1)", 1)]
    public async Task ShouldEvaluateOperators(string expression, object expected, CancellationToken cancellationToken)
    {
        await Assert.That(new Expression(expression).Evaluate(cancellationToken)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("2+2+2+2", 8)]
    [Arguments("2*2*2*2", 16)]
    [Arguments("2*2+2", 6)]
    [Arguments("2+2*2", 6)]
    [Arguments("1 + 2 + 3 * 4 / 2", 9d)]
    [Arguments("18/2/2*3", 13.5)]
    [Arguments("-1 ** 2", -1d)]
    [Arguments("(-1) ** 2", 1d)]
    [Arguments("2 ** 3 ** 2", 512d)]
    [Arguments("(2 ** 3) ** 2", 64d)]
    [Arguments("2 * 3 ** 2", 18d)]
    [Arguments("2 ** 4 / 2", 8d)]
    public async Task ShouldHandleOperatorsPriority(string expression, object expected, CancellationToken cancellationToken)
    {
        await Assert.That(new Expression(expression).Evaluate(cancellationToken)).IsEqualTo(expected);
    }

    [Test]
    public async Task Should_Compare_Bool_Issue_122(CancellationToken cancellationToken)
    {
        var eif = new Expression("foo = true");
        eif.Parameters["foo"] = "true";

        await Assert.That(eif.Evaluate(cancellationToken)).IsEqualTo(true);
    }

    [Test]
    public async Task Should_Use_Correct_BitwiseXOr_133(CancellationToken cancellationToken)
    {
        var logicalExpression = LogicalExpressionFactory.Create(expression: "1 ^ 2", ct: cancellationToken);

        var serializedString = logicalExpression.ToString();

        await Assert.That(serializedString).IsEqualTo("1 ^ 2");
        await Assert.That(new Expression(logicalExpression).Evaluate(cancellationToken)).IsEqualTo(3UL);
    }

    [Test]
    public async Task Should_Short_Circuit_Boolean_Expressions(CancellationToken cancellationToken)
    {
        var e = new Expression("([a] != 0) && ([b]/[a]>2)");
        e.Parameters["a"] = 0;

        await Assert.That((bool)e.Evaluate(cancellationToken)!).IsFalse();
    }
}