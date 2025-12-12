using NCalc.Domain;
using NCalc.Factories;
using NCalc.Tests.TestData;
using Newtonsoft.Json;

namespace NCalc.Tests;

[Property("Category", "Async")]
public class AsyncTests
{
    [Test]
    [MethodDataSource<EvaluationTestData>(nameof(EvaluationTestData.GetTestData))]
    public async Task ShouldEvaluateAsync(string expression, object expected, CancellationToken cancellationToken)
    {
        var e =  new AsyncExpression(expression);
        var res = await e.EvaluateAsync(cancellationToken);
        await Assert.That(res).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldEvaluateAsyncFunction(CancellationToken cancellationToken)
    {
        var expression = new AsyncExpression("database_operation('SELECT FOO') == 'FOO'");
        expression.Functions["database_operation"] = async (_) =>
        {
            // My heavy database work.
            await Task.Delay(1);

            return "FOO";
        };

        var result = await expression.EvaluateAsync(cancellationToken);
        await Assert.That(result).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateAsyncParameter(CancellationToken cancellationToken)
    {
        var expression = new AsyncExpression("(a + b) == 'Leo'");
        expression.Parameters["b"] = new AsyncExpression("'eo'");
        expression.DynamicParameters["a"] = async _ =>
        {
            await Task.Delay(1);
            return "L";
        };

        var result = await expression.EvaluateAsync(cancellationToken);
        await Assert.That(result).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateAsyncFunctionHandler(CancellationToken cancellationToken)
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

        var result = await expression.EvaluateAsync(cancellationToken);
        await Assert.That(result).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateAsyncParameterHandler(CancellationToken cancellationToken)
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

        var result = await expression.EvaluateAsync(cancellationToken);
        await Assert.That(result).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldEvaluateArrayParameters(CancellationToken cancellationToken)
    {
        var e = new AsyncExpression("x * x", ExpressionOptions.IterateParameters)
        {
            Parameters =
            {
                ["x"] = new [] { 0, 1, 2, 3, 4 }
            }
        };

        var result = (IList<object>)await e.EvaluateAsync(cancellationToken);

        await Assert.That(result).IsNotNull();
        await Assert.That(result[0]).IsEqualTo(0);
        await Assert.That(result[1]).IsEqualTo(1);
        await Assert.That(result[2]).IsEqualTo(4);
        await Assert.That(result[3]).IsEqualTo(9);
        await Assert.That(result[4]).IsEqualTo(16);
    }

    [Test]
    [MethodDataSource<BuiltInFunctionsTestData>(nameof(BuiltInFunctionsTestData.GetTestData))]
    public async Task ShouldHandleBuiltInFunctions(string expression, object expected, double? tolerance, CancellationToken cancellationToken)
    {
        var e = new AsyncExpression(expression);
        var result = await e.EvaluateAsync(cancellationToken);

        if (tolerance.HasValue)
        {
            await Assert.That((double)result).IsEqualTo((double)expected).Within(15);
        }
        else
        {
            await Assert.That(result).IsEqualTo(expected);
        }
    }

    [Test]
    [MethodDataSource<ValuesTestData>(nameof(ValuesTestData.GetTestData))]
    public async Task ShouldParseValues(string input, object expectedValue, CancellationToken cancellationToken)
    {
        var expression = new AsyncExpression(input);
        var result = await expression.EvaluateAsync(cancellationToken);

        if (expectedValue is double expectedDouble)
        {
            await Assert.That((double)result).IsEqualTo(expectedDouble).Within(15);
        }
        else
        {
            await Assert.That(result).IsEqualTo(expectedValue);
        }
    }

    [Test]
    [MethodDataSource<NullCheckTestData>(nameof(NullCheckTestData.GetTestData))]
    public async Task ShouldAllowOperatorsWithNulls(string expression, object expected, CancellationToken cancellationToken)
    {
        var e = new AsyncExpression(expression, ExpressionOptions.AllowNullParameter);
        await Assert.That(await e.EvaluateAsync(cancellationToken)).IsEqualTo(expected);
    }

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
            evaluated = await exp.EvaluateAsync(cancellationToken);
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
    public async Task HasErrorsIssue239(string expressionString, bool hasError, CancellationToken cancellationToken)
    {
        var expression = new AsyncExpression(expressionString);
        await Assert.That(expression.HasErrors(cancellationToken)).IsEqualTo(hasError);
    }

    [Test]
    public async Task ShouldEvaluateSubExpressionsAsync(CancellationToken cancellationToken)
    {
        var volume = new Expression("[surface] * h");
        var surface = new Expression("[l] * [L]");
        volume.Parameters["surface"] = surface;
        volume.Parameters["h"] = 3;
        surface.Parameters["l"] = 1;
        surface.Parameters["L"] = 2;

        await Assert.That(volume.Evaluate(cancellationToken)).IsEqualTo(6);
    }

    [Test]
    public async Task ShouldEvaluateTernaryAsync(CancellationToken cancellationToken)
    {
        var e = new AsyncExpression("1 == 1 ? 42 : 3/0");
        await Assert.That(await e.EvaluateAsync(cancellationToken)).IsEqualTo(42);
    }

    [Test]
    public async Task ShouldEvaluatePowAsync(CancellationToken cancellationToken)
    {
        var e = new AsyncExpression("2**2");
        await Assert.That(await e.EvaluateAsync(cancellationToken)).IsEqualTo(4d);
    }
}