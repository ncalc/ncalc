using NCalc.Domain;
using NCalc.Factories;
using NCalc.Tests.TestData;
using Newtonsoft.Json;

namespace NCalc.Tests;

[Trait("Category", "Async")]
public class AsyncTests
{
    [Theory]
    [ClassData(typeof(EvaluationTestData))]
    public async Task ShouldEvaluateAsync(string expression, object expected)
    {
        Assert.Equal(expected, await new Expression(expression).EvaluateAsync());
    }

    [Fact]
    public async Task ShouldEvaluateAsyncFunction()
    {
        var expression = new Expression("database_operation('SELECT FOO') == 'FOO'");
        expression.AsyncFunctions["database_operation"] = async (_) =>
        {
            // My heavy database work.
            await Task.Delay(1);

            return "FOO";
        };

        var result = await expression.EvaluateAsync();
        Assert.Equal(true, result);
    }

    [Fact]
    public async Task ShouldEvaluateAsyncParameter()
    {
        var expression = new Expression("(a + b) == 'Leo'");
        expression.Parameters["b"] = new Expression("'eo'");
        expression.AsyncDynamicParameters["a"] = async _ =>
        {
            await Task.Delay(1);
            return "L";
        };

        var result = await expression.EvaluateAsync();
        Assert.Equal(true, result);
    }

    [Fact]
    public async Task ShouldEvaluateAsyncFunctionHandler()
    {
        var expression = new Expression("database_operation('SELECT FOO') == 'FOO'");
        expression.EvaluateFunction += async (name, args) =>
        {
            if (name == "database_operation")
            {
                //My heavy database work.
                await Task.Delay(100);

                args.Result = "FOO";
            }
        };

        var result = await expression.EvaluateAsync();
        Assert.Equal(true, result);
    }

    [Fact]
    public async Task ShouldEvaluateAsyncParameterHandler()
    {
        var expression = new Expression("(a + b) == 'Leo'");
        expression.Parameters["b"] = new Expression("'eo'");
        expression.EvaluateParameter += (name, args) =>
        {
            if (name == "a")
            {
                args.Result = "L";
            }

            return default;
        };

        var result = await expression.EvaluateAsync();
        Assert.Equal(true, result);
    }

    [Fact]
    public async Task ShouldEvaluateArrayParameters()
    {
        var e = new Expression("x * x", ExpressionOptions.IterateParameters)
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
        var result = await new Expression(expression).EvaluateAsync();

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
        var expression = new Expression(input);
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
        var e = new Expression(expression, ExpressionOptions.AllowNullParameter);
        Assert.Equal(expected, await e.EvaluateAsync());
    }

    [Theory]
    [ClassData(typeof(WaterLevelCheckTestData))]
    public async Task SerializeAndDeserializeShouldWork(string expression, bool expected, double inputValue)
    {
        var compiled = LogicalExpressionFactory.Create(expression);
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
            evaluated = await exp.EvaluateAsync();
        }
        catch
        {
            evaluated = false;
        }

        // Assert
        Assert.Equal(expected, evaluated);
    }

    [Theory]
    [InlineData("1a + ]", true)]
    [InlineData("sergio +", true)]
    [InlineData("42 == 42", false)]
    public void HasErrorsIssue239(string expressionString, bool hasError)
    {
        var expression = new Expression(expressionString);
        Assert.Equal(hasError, expression.HasErrors());
    }

    [Fact]
    public void ShouldEvaluateSubExpressionsAsync()
    {
        var volume = new Expression("[surface] * h");
        var surface = new Expression("[l] * [L]");
        volume.Parameters["surface"] = surface;
        volume.Parameters["h"] = 3;
        surface.Parameters["l"] = 1;
        surface.Parameters["L"] = 2;

        Assert.Equal(6, volume.Evaluate());
    }

    [Fact]
    public async Task ShouldEvaluateTernaryAsync()
    {
        Assert.Equal(42, await new Expression("1 == 1 ? 42 : 3/0").EvaluateAsync());
    }

    [Fact]
    public async Task ShouldEvaluatePowAsync()
    {
        Assert.Equal(4d, await new Expression("2**2").EvaluateAsync());
    }
}