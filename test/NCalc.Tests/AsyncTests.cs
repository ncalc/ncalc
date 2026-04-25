using NCalc.Domain;
using NCalc.Factories;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Threading.Tasks;

namespace NCalc.Tests;

[Property("Category", "Async")]
public class AsyncTests
{
    [Test]
    [MethodDataSource(typeof(EvaluationTestData), "GetEnumerator")]
    public async Task ShouldEvaluateAsync(string expression, object expected)
    {
        var e =  new AsyncExpression(expression);
        var res = await e.EvaluateAsync(CancellationToken.None);
        await Assert.That(res).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldEvaluateAsyncFunction()
    {
        var expression = new AsyncExpression("database_operation('SELECT FOO') == 'FOO'");
        expression.Functions["database_operation"] = async (_) =>
        {
            // My heavy database work.
            await Task.Delay(1);

            return "FOO";
        };

        var result = await expression.EvaluateAsync(CancellationToken.None);
        await Assert.That(result).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateAsyncParameter()
    {
        var expression = new AsyncExpression("(a + b) == 'Leo'");
        expression.Parameters["b"] = new AsyncExpression("'eo'");
        expression.DynamicParameters["a"] = async _ =>
        {
            await Task.Delay(1);
            return "L";
        };

        var result = await expression.EvaluateAsync(CancellationToken.None);
        await Assert.That(result).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateAsyncFunctionHandler()
    {
        var expression = new AsyncExpression("database_operation('SELECT FOO') == 'FOO'");
        expression.EvaluateFunctionAsync += async (name, args) =>
        {
            if (name == "database_operation")
            {
                //My heavy database work.
                await Task.Delay(100, args.CancellationToken);

                args.Result = "FOO";
            }
        };

        var result = await expression.EvaluateAsync(CancellationToken.None);
        await Assert.That(result).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateAsyncParameterHandler()
    {
        var expression = new AsyncExpression("(a + b) == 'Leo'");
        expression.Parameters["b"] = new AsyncExpression("'eo'");
        expression.EvaluateParameterAsync += (name, args) =>
        {
            if (name == "a")
            {
                args.Result = "L";
            }

            return default;
        };

        var result = await expression.EvaluateAsync(CancellationToken.None);
        await Assert.That(result).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateArrayParameters()
    {
        var e = new AsyncExpression("x * x", ExpressionOptions.IterateParameters)
        {
            Parameters =
            {
                ["x"] = new [] { 0, 1, 2, 3, 4 }
            }
        };

        var result = (IList<object>)await e.EvaluateAsync(CancellationToken.None);

        await Assert.That(result).IsNotNull();
        await Assert.That(result[0]).IsEqualTo(0);
        await Assert.That(result[1]).IsEqualTo(1);
        await Assert.That(result[2]).IsEqualTo(4);
        await Assert.That(result[3]).IsEqualTo(9);
        await Assert.That(result[4]).IsEqualTo(16);
    }

    [Test]
    [MethodDataSource(typeof(BuiltInFunctionsTestData), "GetEnumerator")]
    public async Task ShouldHandleBuiltInFunctions(string expression, object expected, double? tolerance)
    {
        var e = new AsyncExpression(expression);
        var result = await e.EvaluateAsync(CancellationToken.None);

        if (tolerance.HasValue)
        {
            // TODO: TUnit migration - xUnit Assert.Equal had additional argument(s) (precision: 15) that could not be converted.
            await Assert.That((double)result).IsEqualTo((double)expected);
        }
        else
        {
            await Assert.That(result).IsEqualTo(expected);
        }
    }

    [Test]
    [MethodDataSource(typeof(ValuesTestData), "GetEnumerator")]
    public async Task ShouldParseValues(string input, object expectedValue)
    {
        var expression = new AsyncExpression(input);
        var result = await expression.EvaluateAsync(CancellationToken.None);

        if (expectedValue is double expectedDouble)
        {
            // TODO: TUnit migration - xUnit Assert.Equal had additional argument(s) (precision: 15) that could not be converted.
            await Assert.That((double)result).IsEqualTo(expectedDouble);
        }
        else
        {
            await Assert.That(result).IsEqualTo(expectedValue);
        }
    }

    [Test]
    [MethodDataSource(typeof(NullCheckTestData), "GetEnumerator")]
    public async Task ShouldAllowOperatorsWithNulls(string expression, object expected)
    {
        var e = new AsyncExpression(expression, ExpressionOptions.AllowNullParameter);
        await Assert.That(await e.EvaluateAsync(CancellationToken.None)).IsEqualTo(expected);
    }

    [Test]
    [MethodDataSource(typeof(WaterLevelCheckTestData), "GetEnumerator")]
    public async Task SerializeAndDeserializeShouldWork(string expression, bool expected, double inputValue)
    {
        var compiled = LogicalExpressionFactory.Create(expression, ct: CancellationToken.None);
        var serialized = JsonSerializer.Serialize(compiled);
        var deserialized = JsonSerializer.Deserialize<LogicalExpression>(serialized);

        var exp = new AsyncExpression(deserialized, ExpressionOptions.NoCache)
        {
            Parameters =
            {
                { "waterlevel", inputValue }
            }
        };

        object evaluated;
        try
        {
            evaluated = await exp.EvaluateAsync(CancellationToken.None);
        }
        catch
        {
            evaluated = false;
        }

        // Assert
        await Assert.That(evaluated).IsEqualTo(expected);
    }

    [Test]
    [Arguments("1a + ]", true)]
    [Arguments("sergio +", true)]
    [Arguments("42 == 42", false)]
    public async Task HasErrorsIssue239(string expressionString, bool hasError)
    {
        var expression = new AsyncExpression(expressionString);
        await Assert.That(expression.HasErrors(CancellationToken.None)).IsEqualTo(hasError);
    }

    [Test]
    public async Task ShouldEvaluateSubExpressionsAsync()
    {
        var volume = new Expression("[surface] * h");
        var surface = new Expression("[l] * [L]");
        volume.Parameters["surface"] = surface;
        volume.Parameters["h"] = 3;
        surface.Parameters["l"] = 1;
        surface.Parameters["L"] = 2;

        await Assert.That(volume.Evaluate(CancellationToken.None)).IsEqualTo(6);
    }

    [Test]
    public async Task ShouldEvaluateTernaryAsync()
    {
        var e = new AsyncExpression("1 == 1 ? 42 : 3/0");
        await Assert.That(await e.EvaluateAsync(CancellationToken.None)).IsEqualTo(42);
    }

    [Test]
    public async Task ShouldEvaluatePowAsync()
    {
        var e = new AsyncExpression("2**2");
        await Assert.That(await e.EvaluateAsync(CancellationToken.None)).IsEqualTo(4d);
    }
}
