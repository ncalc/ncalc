using NCalc.Domain;
using NCalc.Factories;
using Newtonsoft.Json;

namespace NCalc.Tests;

[Trait("Category","Serialization")]
public class SerializationTests
{
    [Theory]
    [InlineData("(waterlevel > 1 AND waterlevel <= 3)", false, 3.2)]
    [InlineData("(waterlevel > 3 AND waterlevel <= 5)", true, 3.2)]
    [InlineData("(waterlevel > 1 AND waterlevel <= 3)", false, 3.1)]
    [InlineData("(waterlevel > 3 AND waterlevel <= 5)", true, 3.1)]
    [InlineData("(3 < waterlevel AND 5 >= waterlevel)", true, 3.1)]
    [InlineData("(3.2 < waterlevel AND 5.3 >= waterlevel)", true, 4)]
    public void SerializeAndDeserialize_ShouldWork(string expression, bool expected, double inputValue)
    {
        var compiled = LogicalExpressionFactory.Create(expression, ExpressionOptions.NoCache);
        var serialized = JsonConvert.SerializeObject(compiled, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All // We need this to allow serializing abstract classes
        });

        var deserialized = JsonConvert.DeserializeObject<LogicalExpression>(serialized, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });

        Expression.EnableCache = false;

        var exp = new Expression(deserialized)
        {
            Parameters = new Dictionary<string, object>
            {
                { "waterlevel", inputValue }
            }
        };

        object evaluated;
        try
        {
            evaluated = exp.Evaluate();
        }
        catch
        {
            evaluated = false;
        }

        // Assert
        Assert.Equal(expected, evaluated);
    }

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
        Assert.Equal("#" + new DateTime(2009, 1, 1) + "#", new ValueExpression(new DateTime(2009, 1, 1)).ToString());
    }
}