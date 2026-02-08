using NCalc.Factories;

namespace NCalc.Tests;

[Trait("Category", "Operators")]
public class OperatorsTests
{
    [Theory]
    [InlineData("NOT true")]
    [InlineData("not true")]
    public void Should_Evaluate_Not_Unary_Operator(string expression)
    {
        var logicalExpression = LogicalExpressionFactory.Create(expression, ct: TestContext.Current.CancellationToken);
        var expr = new Expression(logicalExpression);
        Assert.False((bool)expr.Evaluate(TestContext.Current.CancellationToken)!);
    }

    [Theory]
    [InlineData("!true", false)]
    [InlineData("not false", true)]
    [InlineData("Not false", true)]
    [InlineData("NOT false", true)]
    [InlineData("-10", -10)]
    [InlineData("+20", 20)]
    [InlineData("2**-1", 0.5)]
    [InlineData("2**+2", 4.0)]
    [InlineData("2 * 3", 6)]
    [InlineData("6 / 2", 3d)]
    [InlineData("7 % 2", 1)]
    [InlineData("2 + 3", 5)]
    [InlineData("2 - 1", 1)]
    [InlineData("1 < 2", true)]
    [InlineData("1 > 2", false)]
    [InlineData("1 <= 2", true)]
    [InlineData("1 <= 1", true)]
    [InlineData("1 >= 2", false)]
    [InlineData("1 >= 1", true)]
    [InlineData("1 = 1", true)]
    [InlineData("1 == 1", true)]
    [InlineData("1 != 1", false)]
    [InlineData("1 <> 1", false)]
    [InlineData("1 & 1", 1UL)]
    [InlineData("1 | 1", 1UL)]
    [InlineData("1 ^ 1", 0UL)]
    [InlineData("~1", ~1UL)]
    [InlineData("2 >> 1", 1UL)]
    [InlineData("2 << 1", 4UL)]
    [InlineData("true && false", false)]
    [InlineData("True and False", false)]
    [InlineData("tRue aNd faLse", false)]
    [InlineData("TRUE ANd fALSE", false)]
    [InlineData("true AND FALSE", false)]
    [InlineData("true || false", true)]
    [InlineData("true or false", true)]
    [InlineData("true Or false", true)]
    [InlineData("true OR false", true)]
    [InlineData("if(true, 0, 1)", 0)]
    [InlineData("if(false, 0, 1)", 1)]
    public void ShouldEvaluateOperators(string expression, object expected)
    {
        Assert.Equal(expected, new Expression(expression).Evaluate(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("2+2+2+2", 8)]
    [InlineData("2*2*2*2", 16)]
    [InlineData("2*2+2", 6)]
    [InlineData("2+2*2", 6)]
    [InlineData("1 + 2 + 3 * 4 / 2", 9d)]
    [InlineData("18/2/2*3", 13.5)]
    [InlineData("-1 ** 2", -1d)]
    [InlineData("(-1) ** 2", 1d)]
    [InlineData("2 ** 3 ** 2", 512d)]
    [InlineData("(2 ** 3) ** 2", 64d)]
    [InlineData("2 * 3 ** 2", 18d)]
    [InlineData("2 ** 4 / 2", 8d)]
    public void ShouldHandleOperatorsPriority(string expression, object expected)
    {
        Assert.Equal(expected, new Expression(expression).Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Should_Compare_Bool_Issue_122()
    {
        var eif = new Expression("foo = true");
        eif.Parameters["foo"] = "true";

        Assert.Equal(true, eif.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Should_Use_Correct_BitwiseXOr_133()
    {
        var logicalExpression = LogicalExpressionFactory.Create(expression: "1 ^ 2", ct: TestContext.Current.CancellationToken);

        var serializedString = logicalExpression.ToString();

        Assert.Equal("1 ^ 2", serializedString);
        Assert.Equal(3UL, new Expression(logicalExpression).Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Should_Short_Circuit_Boolean_Expressions()
    {
        var e = new Expression("([a] != 0) && ([b]/[a]>2)");
        e.Parameters["a"] = 0;

        Assert.False((bool)e.Evaluate(TestContext.Current.CancellationToken)!);
    }

    [Theory]
    [InlineData("0 | 0", 0ul)]
    [InlineData("0 | 1", 1ul)]
    [InlineData("1 | 0", 1ul)]
    [InlineData("1 | 1", 1ul)]
    [InlineData("0 & 0", 0ul)]
    [InlineData("0 & 1", 0ul)]
    [InlineData("1 & 0", 0ul)]
    [InlineData("1 & 1", 1ul)]
    [InlineData("0 ^ 0", 0ul)]
    [InlineData("0 ^ 1", 1ul)]
    [InlineData("1 ^ 0", 1ul)]
    [InlineData("1 ^ 1", 0ul)]
    public void ShouldHandleSimpleBitwiseOperations(string expression, ulong expected)
    {
        var e = new Expression(expression);
        var result = e.Evaluate(TestContext.Current.CancellationToken);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1 = 1 ^ 2 = 2 || 3 = 3", true)]
    [InlineData("1 = 1 ^ 2 = 2 && 2 = 1", false)]
    public void ShouldRespectBitwiseOperatorPrecedence(string exp, bool expected)
    {
        var e = new Expression(exp);
        var result = e.Evaluate(TestContext.Current.CancellationToken);

        Assert.Equal(expected, result);
    }
}