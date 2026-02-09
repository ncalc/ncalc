using NCalc.Domain;
using NCalc.Factories;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NCalc.Tests;

[Trait("Category", "Serialization")]
public class SerializationTests
{
    [Theory]
    [ClassData(typeof(WaterLevelCheckTestData))]
    public void SerializeAndDeserializeShouldWork(string expression, bool expected, double inputValue)
    {
        var compiled = LogicalExpressionFactory.Create(expression, ct: TestContext.Current.CancellationToken);
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
            evaluated = exp.Evaluate(TestContext.Current.CancellationToken);
        }
        catch
        {
            evaluated = false;
        }

        // Assert
        Assert.Equal(expected, evaluated);
    }

#if NET
    [Fact]
    public void SystemTextJsonPolymorphicSerializeAndDeserializeShouldWork()
    {
        var expression = LogicalExpressionFactory.Create("1 == 1", ct: TestContext.Current.CancellationToken);
        var expressionJson = JsonSerializer.Serialize(expression);
        Assert.True(JsonSerializer.Deserialize<LogicalExpression>(expressionJson) is BinaryExpression);
    }
#endif

    [Fact]
    public void Binary_Expression_Serialization_Test()
    {
        Assert.Equal("True and False",
    new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false))
        .ToString());
        Assert.Equal("1 / 2",
            new BinaryExpression(BinaryExpressionType.Div, new ValueExpression(1), new ValueExpression(2)).ToString());
        Assert.Equal("1 = 2",
            new BinaryExpression(BinaryExpressionType.Equal, new ValueExpression(1), new ValueExpression(2))
                .ToString());
        Assert.Equal("1 > 2",
            new BinaryExpression(BinaryExpressionType.Greater, new ValueExpression(1), new ValueExpression(2))
                .ToString());
        Assert.Equal("1 >= 2",
            new BinaryExpression(BinaryExpressionType.GreaterOrEqual, new ValueExpression(1), new ValueExpression(2))
                .ToString());
        Assert.Equal("1 < 2",
            new BinaryExpression(BinaryExpressionType.Lesser, new ValueExpression(1), new ValueExpression(2))
                .ToString());
        Assert.Equal("1 <= 2",
            new BinaryExpression(BinaryExpressionType.LesserOrEqual, new ValueExpression(1), new ValueExpression(2))
                .ToString());
        Assert.Equal("1 - 2",
            new BinaryExpression(BinaryExpressionType.Minus, new ValueExpression(1), new ValueExpression(2))
                .ToString());
        Assert.Equal("1 % 2",
            new BinaryExpression(BinaryExpressionType.Modulo, new ValueExpression(1), new ValueExpression(2))
                .ToString());
        Assert.Equal("1 != 2",
            new BinaryExpression(BinaryExpressionType.NotEqual, new ValueExpression(1), new ValueExpression(2))
                .ToString());
        Assert.Equal("True or False",
            new BinaryExpression(BinaryExpressionType.Or, new ValueExpression(true), new ValueExpression(false))
                .ToString());
        Assert.Equal("1 + 2",
            new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(1), new ValueExpression(2)).ToString());
        Assert.Equal("1 * 2",
            new BinaryExpression(BinaryExpressionType.Times, new ValueExpression(1), new ValueExpression(2))
                .ToString());
    }

    [Fact]
    public void Unary_Expression_Serialization_Test()
    {
        Assert.Equal("-(True and False)",
            new UnaryExpression(UnaryExpressionType.Negate,
                    new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true),
                        new ValueExpression(false)))
                .ToString());
        Assert.Equal("!(True and False)",
            new UnaryExpression(UnaryExpressionType.Not,
                    new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true),
                        new ValueExpression(false)))
                .ToString());
    }

    [Fact]
    public void Function_Serialization_Test()
    {
        Assert.Equal("test(True and False, -(True and False))", new Function(new Identifier("test"), [
            new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false)),
            new UnaryExpression(UnaryExpressionType.Negate,
                new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false)))
        ]).ToString());

        Assert.Equal("Sum(1 + 2)", new Function(new Identifier("Sum"), [
            new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(1), new ValueExpression(2))
        ]).ToString());
    }

    [Fact]
    public void Value_Serialization_Test()
    {
        Assert.Equal("True", new ValueExpression(true).ToString());
        Assert.Equal("False", new ValueExpression(false).ToString());
        Assert.Equal("1", new ValueExpression(1).ToString());
        Assert.Equal("1.234", new ValueExpression(1.234).ToString());
        Assert.Equal("'hello'", new ValueExpression("hello").ToString());
        Assert.Equal("'c'", new ValueExpression('c').ToString());
        Assert.Equal("#" + new DateTime(2009, 1, 1) + "#", new ValueExpression(new DateTime(2009, 1, 1)).ToString());
    }

    [Fact]
    public void ArraySerializationTest()
    {
        var trueArrayExpression = new LogicalExpressionList([new ValueExpression(true)]);
        var helloWorldArrayExpression = new LogicalExpressionList([new ValueExpression("Hello"), new ValueExpression("World")]);
        Assert.Equal("(True)", trueArrayExpression.ToString());
        Assert.Equal("('Hello','World')", helloWorldArrayExpression.ToString());
        Assert.Equal("()", new LogicalExpressionList([]).ToString());
        Assert.Equal("((True),('Hello','World'))", new LogicalExpressionList([trueArrayExpression, helloWorldArrayExpression]).ToString());
    }

    [Fact]
    public void FunctionWithParametersSerializationTest()
    {
        var expr = new Expression("Max([a], [b])");
        expr.Parameters["a"] = 5;
        expr.Parameters["b"] = 10;
        expr.Evaluate(TestContext.Current.CancellationToken);

        var exprString = expr.LogicalExpression.ToString();
        Assert.Equal("Max([a], [b])", exprString);
    }
}