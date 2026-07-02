using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Parser;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NCalc.Tests;

[Property("Category", "Async")]
public class AsyncTests
{
    [Test]
    [MethodDataSource(typeof(EvaluationTestData), "GetEnumerator")]
    public async Task ShouldEvaluateAsync(string expression, object expected)
    {
        var e =  new Expression(expression);
        var res = await e.EvaluateAsync(CancellationToken.None);
        await Assert.That(res).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldEvaluateAsyncFunction()
    {
        var expression = new Expression("database_operation('SELECT FOO') == 'FOO'");
        expression.AsyncFunctions["database_operation"] = async (_) =>
        {
            // My heavy database work.
            await Task.Delay(1);

            return "FOO";
        };

        var result = await expression.EvaluateAsync<bool>(CancellationToken.None);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task ShouldEvaluateAsyncFunctionHandler()
    {
        var expression = new Expression("database_operation('SELECT FOO') == 'FOO'");
        expression.EvaluateAsyncFunction += async (name, args) =>
        {
            if (name == "database_operation")
            {
                //My heavy database work.
                await Task.Delay(100, args.CancellationToken);

                args.Result = "FOO";
            }
        };

        var result = (bool?)await expression.EvaluateAsync(CancellationToken.None);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task ShouldIgnoreAsyncFunctionHandlerDuringSyncEvaluation()
    {
        var expression = new Expression("Abs(-1)");
        var asyncHandlerCalled = false;
        expression.EvaluateAsyncFunction += (_, _) =>
        {
            asyncHandlerCalled = true;
            return Task.CompletedTask;
        };

        await Assert.That(expression.Evaluate(CancellationToken.None)).IsEqualTo(1d);
        await Assert.That(asyncHandlerCalled).IsFalse();
    }

    [Test]
    public async Task ShouldPreferSyncFunctionHandlerOverAsyncFunctionHandler()
    {
        var expression = new Expression("database_operation('SELECT FOO') == 'FOO'");
        var asyncHandlerCalled = false;

        expression.EvaluateFunction += (name, args) =>
        {
            if (name == "database_operation")
                args.Result = "FOO";
        };

        expression.EvaluateAsyncFunction += (_, _) =>
        {
            asyncHandlerCalled = true;
            return Task.CompletedTask;
        };

        var result = (bool?)await expression.EvaluateAsync(CancellationToken.None);

        await Assert.That(result).IsTrue();
        await Assert.That(asyncHandlerCalled).IsFalse();
    }

    [Test]
    public async Task ShouldEvaluateAsyncParameterHandler()
    {
        var expression = new Expression("(a + b) == 'Leo'")
        {
            Parameters =
            {
                ["b"] = new Expression("'eo'")
            }
        };
        expression.EvaluateParameter += (name, args) =>
        {
            if (name == "a")
            {
                args.Result = "L";
            }
        };

        var result = (bool?)await expression.EvaluateAsync(CancellationToken.None);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task ShouldEvaluateAsyncParameterDictionary()
    {
        var expression = new Expression("(a + b) == 'Leo'")
        {
            Parameters =
            {
                ["b"] = new Expression("'eo'")
            }
        };

        expression.AsyncParameters["a"] = async args =>
        {
            await Task.Delay(1, args.CancellationToken);
            return "L";
        };

        var result = (bool?)await expression.EvaluateAsync(CancellationToken.None);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task ShouldIgnoreAsyncParameterHandlerAndDictionaryDuringSyncEvaluation()
    {
        var expression = new Expression("x");
        var asyncHandlerCalled = false;
        var asyncDictionaryCalled = false;

        expression.EvaluateAsyncParameter += (_, _) =>
        {
            asyncHandlerCalled = true;
            return Task.CompletedTask;
        };

        expression.AsyncParameters["x"] = _ =>
        {
            asyncDictionaryCalled = true;
            return Task.FromResult((object)1);
        };

        await Assert.That(() => expression.Evaluate(CancellationToken.None))
            .Throws<NCalcParameterNotDefinedException>();
        await Assert.That(asyncHandlerCalled).IsFalse();
        await Assert.That(asyncDictionaryCalled).IsFalse();
    }

    [Test]
    public async Task ShouldPreferSyncParameterHandlerOverAsyncParameterHandler()
    {
        var expression = new Expression("x");
        var asyncHandlerCalled = false;

        expression.EvaluateParameter += (_, args) => args.Result = 1;
        expression.EvaluateAsyncParameter += (_, _) =>
        {
            asyncHandlerCalled = true;
            return Task.CompletedTask;
        };

        var result = await expression.EvaluateAsync(CancellationToken.None);

        await Assert.That(result).IsEqualTo(1);
        await Assert.That(asyncHandlerCalled).IsFalse();
    }

    [Test]
    public async Task ShouldPreferSyncParameterDictionaryOverAsyncParameterDictionary()
    {
        var expression = new Expression("x");
        var asyncDictionaryCalled = false;

        expression.DynamicParameters["x"] = _ => 1;
        expression.AsyncParameters["x"] = _ =>
        {
            asyncDictionaryCalled = true;
            return Task.FromResult((object)2);
        };

        var result = await expression.EvaluateAsync(CancellationToken.None);

        await Assert.That(result).IsEqualTo(1);
        await Assert.That(asyncDictionaryCalled).IsFalse();
    }

    [Test]
    public async Task ShouldShareAsyncParametersWithNestedExpressions()
    {
        var expression = new Expression("outer")
        {
            Parameters =
            {
                ["outer"] = new Expression("inner")
            },
            AsyncParameters =
            {
                ["inner"] = async args =>
                {
                    await Task.Delay(1, args.CancellationToken);
                    return 69;
                }
            }
        };

        var result = await expression.EvaluateAsync(CancellationToken.None);

        await Assert.That(result).IsEqualTo(69);
    }

    [Test]
    public async Task ShouldEvaluateAsyncBinaryHandler()
    {
        var expression = new Expression("1 + 2");

        expression.EvaluateBinaryAsync += async args =>
        {
            if (args.BinaryExpression.Type != BinaryExpressionType.Plus)
                return;

            await Task.Delay(1, args.CancellationToken);
            args.Result = (int)(await args.LeftValueAsync())! + (int)(await args.RightValueAsync())! + 10;
        };

        var result = await expression.EvaluateAsync(CancellationToken.None);
        await Assert.That(result).IsEqualTo(13);
    }

    [Test]
    public async Task ShouldIgnoreAsyncBinaryHandlerDuringSyncEvaluation()
    {
        var expression = new Expression("1 + 2");
        var asyncHandlerCalled = false;

        expression.EvaluateBinaryAsync += _ =>
        {
            asyncHandlerCalled = true;
            return Task.CompletedTask;
        };

        await Assert.That(expression.Evaluate(CancellationToken.None)).IsEqualTo(3);
        await Assert.That(asyncHandlerCalled).IsFalse();
    }

    [Test]
    public async Task ShouldPreferSyncBinaryHandlerOverAsyncBinaryHandler()
    {
        var expression = new Expression("1 + 2");
        var asyncHandlerCalled = false;

        expression.EvaluateBinary += args =>
        {
            if (args.BinaryExpression.Type == BinaryExpressionType.Plus)
                args.Result = 10;
        };

        expression.EvaluateBinaryAsync += _ =>
        {
            asyncHandlerCalled = true;
            return Task.CompletedTask;
        };

        var result = await expression.EvaluateAsync(CancellationToken.None);

        await Assert.That(result).IsEqualTo(10);
        await Assert.That(asyncHandlerCalled).IsFalse();
    }

    [Test]
    public async Task ShouldEvaluateArrayParameters()
    {
        var e = new Expression("x * x", ExpressionOptions.IterateParameters)
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
    public async Task ShouldEvaluateIfFalseBranchWhenIteratedConditionComparesNaNAsync()
    {
        var logicalExpression = LogicalExpressionParser.Parse(new LogicalExpressionParserContext(
            "if((A - B) < 1; 0; (A - B))",
            new LogicalExpressionParserOptions { ArgumentSeparator = LogicalExpressionArgumentSeparator.Semicolon }));
        var expression = new Expression(logicalExpression, ExpressionOptions.IterateParameters)
        {
            Parameters =
            {
                ["A"] = new[] { double.NaN },
                ["B"] = new[] { double.NaN }
            }
        };

        var result = await expression.EvaluateAsync<IList<object>>(CancellationToken.None);

        await Assert.That(result).IsNotNull();
        await Assert.That(result.Count).IsEqualTo(1);
        await Assert.That(double.IsNaN((double)result[0])).IsTrue();
    }

    [Test]
    [MethodDataSource(typeof(BuiltInFunctionsTestData), "GetEnumerator")]
    public async Task ShouldHandleBuiltInFunctions(string expression, object expected, double? tolerance)
    {
        var e = new Expression(expression);
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
        var expression = new Expression(input);
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
        var e = new Expression(expression, ExpressionOptions.AllowNullParameter);
        await Assert.That(await e.EvaluateAsync(CancellationToken.None)).IsEqualTo(expected);
    }

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
        var expression = new Expression(expressionString);
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
        var e = new Expression("1 == 1 ? 42 : 3/0");
        await Assert.That(await e.EvaluateAsync(CancellationToken.None)).IsEqualTo(42);
    }

    [Test]
    public async Task ShouldEvaluatePowAsync()
    {
        var e = new Expression("2**2");
        await Assert.That(await e.EvaluateAsync(CancellationToken.None)).IsEqualTo(4d);
    }

    [Test]
    public async Task Should_Use_IEEE754_Semantics_For_NaN_Comparisons_Async()
    {
        var expression = new Expression("amount != amount")
        {
            Parameters =
            {
                ["amount"] = double.NaN
            }
        };

        await Assert.That(await new Expression("amount < 0")
        {
            Parameters =
            {
                ["amount"] = double.NaN
            }
        }.EvaluateAsync<bool>(CancellationToken.None)).IsFalse();
        await Assert.That(await expression.EvaluateAsync<bool>(CancellationToken.None)).IsTrue();
        await Assert.That(await new Expression("(0.0 / 0.0) == (0.0 / 0.0)").EvaluateAsync<bool>(CancellationToken.None))
            .IsFalse();
    }
}
