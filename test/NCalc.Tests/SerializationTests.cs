using NCalc.Factories;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Threading.Tasks;

namespace NCalc.Tests;

[Property("Category", "Serialization")]
public class SerializationTests
{
    [Test]
    [MethodDataSource(typeof(WaterLevelCheckTestData), "GetEnumerator")]
    public async Task SerializeAndDeserializeShouldWork(string expression, bool expected, double inputValue)
    {
        var compiled = LogicalExpressionFactory.Create(expression, cancellationToken: CancellationToken.None);
        var serialized = JsonSerializer.Serialize(compiled);
        var deserialized = JsonSerializer.Deserialize<LogicalExpression>(serialized);

        var exp = new Expression(deserialized, ExpressionOptions.NoCache)
        {
            Parameters =
            {
                { "waterlevel", inputValue }
            }
        };

        object evaluated;
        try
        {
            evaluated = exp.Evaluate(CancellationToken.None);
        }
        catch
        {
            evaluated = false;
        }

        // Assert
        await Assert.That(evaluated).IsEqualTo(expected);
    }

#if NET
    [Test]
    public async Task SystemTextJsonPolymorphicSerializeAndDeserializeShouldWork()
    {
        var expression = LogicalExpressionFactory.Create("1 == 1", cancellationToken: CancellationToken.None);
        var expressionJson = JsonSerializer.Serialize(expression);
        await Assert.That(JsonSerializer.Deserialize<LogicalExpression>(expressionJson) is BinaryExpression).IsTrue();
    }
#endif

    [Test]
    public async Task Binary_Expression_Serialization_Test()
    {
        await Assert.That(new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false))
        .ToExpressionString()).IsEqualTo("True and False");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Div, new ValueExpression(1), new ValueExpression(2)).ToExpressionString()).IsEqualTo("1 / 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Equal, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 = 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Greater, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 > 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.GreaterOrEqual, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 >= 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Lesser, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 < 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.LesserOrEqual, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 <= 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Minus, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 - 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Modulo, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 % 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.NotEqual, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 != 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Or, new ValueExpression(true), new ValueExpression(false))
                .ToExpressionString()).IsEqualTo("True or False");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(1), new ValueExpression(2)).ToExpressionString()).IsEqualTo("1 + 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Times, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 * 2");
    }

    [Test]
    public async Task Unary_Expression_Serialization_Test()
    {
        await Assert.That(new UnaryExpression(UnaryExpressionType.Negate,
                new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true),
                        new ValueExpression(false)))
                .ToExpressionString()).IsEqualTo("-(True and False)");
        await Assert.That(new UnaryExpression(UnaryExpressionType.Not,
                    new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true),
                        new ValueExpression(false)))
                .ToExpressionString()).IsEqualTo("!(True and False)");
    }

    [Test]
    public async Task Function_Serialization_Test()
    {
        await Assert.That(new Function(new Identifier("test"), new LogicalExpressionList([
            new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false)),
            new UnaryExpression(UnaryExpressionType.Negate,
                new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false)))
        ])).ToExpressionString()).IsEqualTo("test(True and False, -(True and False))");

        await Assert.That(new Function(new Identifier("Sum"), new LogicalExpressionList([
            new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(1), new ValueExpression(2))
        ])).ToExpressionString()).IsEqualTo("Sum(1 + 2)");
    }

    [Test]
    public async Task Value_Serialization_Test()
    {
        await Assert.That(new ValueExpression(true).ToExpressionString()).IsEqualTo("True");
        await Assert.That(new ValueExpression(false).ToExpressionString()).IsEqualTo("False");
        await Assert.That(new ValueExpression(1).ToExpressionString()).IsEqualTo("1");
        await Assert.That(new ValueExpression(1.234).ToExpressionString()).IsEqualTo("1.234");
        await Assert.That(new ValueExpression("hello").ToExpressionString()).IsEqualTo("'hello'");
        await Assert.That(new ValueExpression('c').ToExpressionString()).IsEqualTo("'c'");
        await Assert.That(new ValueExpression(new DateTime(2009, 1, 1)).ToExpressionString()).IsEqualTo("#" + new DateTime(2009, 1, 1) + "#");
    }

    [Test]
    public async Task ArraySerializationTest()
    {
        var trueArrayExpression = new LogicalExpressionList([new ValueExpression(true)]);
        var helloWorldArrayExpression = new LogicalExpressionList([new ValueExpression("Hello"), new ValueExpression("World")]);
        await Assert.That(trueArrayExpression.ToExpressionString()).IsEqualTo("(True)");
        await Assert.That(helloWorldArrayExpression.ToExpressionString()).IsEqualTo("('Hello','World')");
        await Assert.That(new LogicalExpressionList([]).ToExpressionString()).IsEqualTo("()");
        await Assert.That(new LogicalExpressionList([trueArrayExpression, helloWorldArrayExpression]).ToExpressionString()).IsEqualTo("((True),('Hello','World'))");
    }

    [Test]
    public async Task FunctionWithParametersSerializationTest()
    {
        var expr = new Expression("Max([a], [b])");
        expr.Parameters["a"] = 5;
        expr.Parameters["b"] = 10;
        expr.Evaluate(CancellationToken.None);

        var exprString = expr.LogicalExpression.ToExpressionString();
        await Assert.That(exprString).IsEqualTo("Max([a], [b])");
    }

    [Test]
    public async Task ShouldSerializeExpressionWithParameterValues()
    {
        var expression = new Expression("[ValueA] + [ValueB]");
        expression.Parameters["ValueA"] = 10;
        expression.Parameters["ValueB"] = 15;

        await Assert.That(expression.ToExpressionString(evaluateParameters: true)).IsEqualTo("10 + 15");
    }

    [Test]
    public async Task ShouldNotSerializeExpressionWithParameterValuesByDefault()
    {
        var expression = new Expression("[ValueA] + [ValueB]");
        expression.Parameters["ValueA"] = 10;
        expression.Parameters["ValueB"] = 15;

        await Assert.That(expression.ToExpressionString()).IsEqualTo("([ValueA]) + ([ValueB])");
    }

    [Test]
    public async Task ShouldSerializeLogicalExpressionWithContextParameterValues()
    {
        var context = new ExpressionContext(new Dictionary<string, object>
        {
            ["ValueA"] = 10,
            ["ValueB"] = 15
        });

        var expression = new Expression("[ValueA] + [ValueB]", context);

        await Assert.That(expression.ToExpressionString(evaluateParameters: true, cancellationToken: CancellationToken.None)).IsEqualTo("10 + 15");
    }

    [Test]
    public async Task ShouldSerializeExpressionWithDynamicParameterValues()
    {
        var expression = new Expression("[ValueA] + [ValueB]")
        {
            Parameters =
            {
                ["ValueA"] = 10
            },
            DynamicParameters =
            {
                ["ValueB"] = _ => 15
            }
        };

        await Assert.That(expression.ToExpressionString(evaluateParameters: true)).IsEqualTo("10 + 15");
    }

    [Test]
    public async Task ShouldKeepUnknownParametersWhenSerializingWithParameterValues()
    {
        var expression = new Expression("[ValueA] + [ValueB]")
        {
            Parameters =
            {
                ["ValueA"] = 10
            }
        };

        await Assert.That(expression.ToExpressionString(evaluateParameters: true)).IsEqualTo("10 + [ValueB]");
    }
}
