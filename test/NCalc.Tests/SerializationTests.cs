using NCalc.Domain;
using NCalc.Factories;
using NCalc.Tests.TestData;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NCalc.Tests;

[Property("Category", "Serialization")]
public class SerializationTests
{
    [Test]
    [MethodDataSource<WaterLevelCheckTestData>(nameof(WaterLevelCheckTestData.GetTestData))]
    public async Task SerializeAndDeserializeShouldWork(string expression, bool expected, double inputValue, CancellationToken cancellationToken)
    {
        var compiled = LogicalExpressionFactory.Create(expression, ct: cancellationToken);
        var serialized = JsonConvert.SerializeObject(compiled, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All // We need this to allow serializing abstract classes
        });

        var deserialized = JsonConvert.DeserializeObject<LogicalExpression>(serialized, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });

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
            evaluated = exp.Evaluate(cancellationToken);
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
    public async Task SystemTextJsonPolymorphicSerializeAndDeserializeShouldWork(CancellationToken cancellationToken)
    {
        var expression = LogicalExpressionFactory.Create("1 == 1", ct: cancellationToken);
        var expressionJson = JsonSerializer.Serialize(expression);
        await Assert.That(JsonSerializer.Deserialize<LogicalExpression>(expressionJson) is BinaryExpression).IsTrue();
    }
#endif

    [Test]
    public async Task Binary_Expression_Serialization_Test()
    {
        await Assert.That(new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false))
        .ToString()).IsEqualTo("True and False");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Div, new ValueExpression(1), new ValueExpression(2)).ToString()).IsEqualTo("1 / 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Equal, new ValueExpression(1), new ValueExpression(2))
                .ToString()).IsEqualTo("1 = 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Greater, new ValueExpression(1), new ValueExpression(2))
                .ToString()).IsEqualTo("1 > 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.GreaterOrEqual, new ValueExpression(1), new ValueExpression(2))
                .ToString()).IsEqualTo("1 >= 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Lesser, new ValueExpression(1), new ValueExpression(2))
                .ToString()).IsEqualTo("1 < 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.LesserOrEqual, new ValueExpression(1), new ValueExpression(2))
                .ToString()).IsEqualTo("1 <= 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Minus, new ValueExpression(1), new ValueExpression(2))
                .ToString()).IsEqualTo("1 - 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Modulo, new ValueExpression(1), new ValueExpression(2))
                .ToString()).IsEqualTo("1 % 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.NotEqual, new ValueExpression(1), new ValueExpression(2))
                .ToString()).IsEqualTo("1 != 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Or, new ValueExpression(true), new ValueExpression(false))
                .ToString()).IsEqualTo("True or False");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(1), new ValueExpression(2)).ToString()).IsEqualTo("1 + 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Times, new ValueExpression(1), new ValueExpression(2))
                .ToString()).IsEqualTo("1 * 2");
    }

    [Test]
    public async Task Unary_Expression_Serialization_Test()
    {
        await Assert.That(new UnaryExpression(UnaryExpressionType.Negate,
                    new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true),
                        new ValueExpression(false)))
                .ToString()).IsEqualTo("-(True and False)");
        await Assert.That(new UnaryExpression(UnaryExpressionType.Not,
                    new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true),
                        new ValueExpression(false)))
                .ToString()).IsEqualTo("!(True and False)");
    }

    [Test]
    public async Task Function_Serialization_Test()
    {
        await Assert.That(new Function(new Identifier("test"), [
            new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false)),
            new UnaryExpression(UnaryExpressionType.Negate,
                new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false)))
        ]).ToString()).IsEqualTo("test(True and False, -(True and False))");

        await Assert.That(new Function(new Identifier("Sum"), [
            new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(1), new ValueExpression(2))
        ]).ToString()).IsEqualTo("Sum(1 + 2)");
    }

    [Test]
    public async Task Value_Serialization_Test()
    {
        await Assert.That(new ValueExpression(true).ToString()).IsEqualTo("True");
        await Assert.That(new ValueExpression(false).ToString()).IsEqualTo("False");
        await Assert.That(new ValueExpression(1).ToString()).IsEqualTo("1");
        await Assert.That(new ValueExpression(1.234).ToString()).IsEqualTo("1.234");
        await Assert.That(new ValueExpression("hello").ToString()).IsEqualTo("'hello'");
        await Assert.That(new ValueExpression('c').ToString()).IsEqualTo("'c'");
        await Assert.That(new ValueExpression(new DateTime(2009, 1, 1)).ToString()).IsEqualTo("#" + new DateTime(2009, 1, 1) + "#");
    }

    [Test]
    public async Task ArraySerializationTest()
    {
        var trueArrayExpression = new LogicalExpressionList([new ValueExpression(true)]);
        var helloWorldArrayExpression = new LogicalExpressionList([new ValueExpression("Hello"), new ValueExpression("World")]);
        await Assert.That(trueArrayExpression.ToString()).IsEqualTo("(True)");
        await Assert.That(helloWorldArrayExpression.ToString()).IsEqualTo("('Hello','World')");
        await Assert.That(new LogicalExpressionList([]).ToString()).IsEqualTo("()");
        await Assert.That(new LogicalExpressionList([trueArrayExpression, helloWorldArrayExpression]).ToString()).IsEqualTo("((True),('Hello','World'))");
    }

    [Test]
    public async Task FunctionWithParametersSerializationTest(CancellationToken cancellationToken)
    {
        var expr = new Expression("Max([a], [b])");
        expr.Parameters["a"] = 5;
        expr.Parameters["b"] = 10;
        expr.Evaluate(cancellationToken);

        var exprString = expr.LogicalExpression.ToString();
        await Assert.That(exprString).IsEqualTo("Max([a], [b])");
    }
}