using NCalc.Domain;
using NCalc.Factories;
using NCalc.Tests.TestData;
using Newtonsoft.Json;

namespace NCalc.Tests;

[Trait("Category","Async")]
public class AsyncTests
{
    [Theory]
    [ClassData(typeof(EvaluationTestData))]
    public async Task ShouldEvaluateAsync(string expression, object expected)
    {
        Assert.Equal(expected, await new AsyncExpression(expression).EvaluateAsync());
    }

    [Fact]
    public async Task ShouldEvaluateAsyncFunction()
    {
        var expression = new AsyncExpression("database_operation('SELECT FOO') == 'FOO'");
        expression.EvaluateFunctionAsync += async (name, args) =>
        {
            if (name == "database_operation")
            {
                //My heavy database work.
                await Task.Delay(100);

                args.Result = "FOO";
            }
        };
        
        var result = await expression.EvaluateAsync();
        Assert.Equal(true,result);
    }
    
    [Fact]
    public async Task ShouldEvaluateAsyncParameter()
    {
        var expression = new AsyncExpression("(a + b) == 'Leo'");
        expression.Parameters["b"] = new AsyncExpression("'eo'");
        expression.EvaluateParameterAsync += (name, args) =>
        {
            if (name == "a")
            {
                args.Result = "L";
            }

            return Task.CompletedTask;
        };
        
        var result = await expression.EvaluateAsync();
        Assert.Equal(true,result);
    }
    
    [Fact]
    public async Task ShouldEvaluateArrayParameters()
    {
        var e = new AsyncExpression("x * x", ExpressionOptions.IterateParameters)
        {
            Parameters =
            {
                ["x"] = new [] { 0, 1, 2, 3, 4 }
            }
        };

        var result = (IList<object>)await e.EvaluateAsync();

        Assert.NotNull(result);
        Assert.Equal(0, result[0]);
        Assert.Equal(1, result[1]);
        Assert.Equal(4, result[2]);
        Assert.Equal(9, result[3]);
        Assert.Equal(16, result[4]);
    }
    
    [Theory]
    [ClassData(typeof(BuiltInFunctionsTestData))]
    public async Task ShouldHandleBuiltInFunctions(string expression, object expected, double? tolerance)
    {
        var result = await new AsyncExpression(expression).EvaluateAsync();

        if (tolerance.HasValue)
        {
            Assert.Equal((double)expected, (double)result, precision: 15);
        }
        else
        {
            Assert.Equal(expected, result);
        }
    }
    
    [Theory]
    [ClassData(typeof(ValuesTestData))]
    public async Task ShouldParseValues(string input, object expectedValue)
    {
        var expression = new AsyncExpression(input);
        var result = await expression.EvaluateAsync();
        
        if (expectedValue is double expectedDouble)
        {
            Assert.Equal(expectedDouble, (double)result, precision: 15);
        }
        else
        {
            Assert.Equal(expectedValue, result);
        }
    }
    
    [Theory]
    [ClassData(typeof(NullCheckTestData))]
    public async Task ShouldAllowOperatorsWithNulls(string expression, object expected)
    {
        var e = new AsyncExpression(expression, ExpressionOptions.AllowNullParameter);
        Assert.Equal(expected, await e.EvaluateAsync());
    }
    
    [Theory]
    [ClassData(typeof(WaterLevelCheckTestData))]
    public async Task SerializeAndDeserializeShouldWork(string expression, bool expected, double inputValue)
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

        Expression.CacheEnabled = false;

        var exp = new AsyncExpression(deserialized)
        {
            Parameters = new Dictionary<string, object>
            {
                { "waterlevel", inputValue }
            }
        };

        object evaluated;
        try
        {
            evaluated = await exp.EvaluateAsync();
        }
        catch
        {
            evaluated = false;
        }

        // Assert
        Assert.Equal(expected, evaluated);
    }
}