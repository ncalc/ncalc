using NCalc.Exceptions;
using NCalc.Factories;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NCalc.Tests;

[Property("Category", "Async")]
public class AsyncTests
{
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
    public async Task ShouldEvaluateBinaryOperandsConcurrentlyWhenEnabled()
    {
        var operandsStarted = CreateCompletionSources();
        var releaseOperands = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var expression = CreateConcurrentExpression("first() + second()");
        expression.AsyncFunctions["first"] = async _ =>
        {
            operandsStarted[0].SetResult();
            await releaseOperands.Task;
            return 1;
        };
        expression.AsyncFunctions["second"] = async _ =>
        {
            operandsStarted[1].SetResult();
            await releaseOperands.Task;
            return 2;
        };

        var evaluationTask = expression.EvaluateAsync(CancellationToken.None);

        await WaitForAll(operandsStarted);
        releaseOperands.SetResult();

        await Assert.That(await evaluationTask).IsEqualTo(3);
    }

    [Test]
    public async Task ShouldEvaluateBuiltInFunctionArgumentsConcurrentlyWhenEnabled()
    {
        var argumentsStarted = CreateCompletionSources(2);
        var releaseArguments = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var expression = CreateConcurrentExpression("Max(first(), second())");
        expression.AsyncFunctions["first"] = async _ =>
        {
            argumentsStarted[0].SetResult();
            await releaseArguments.Task;
            return 1;
        };
        expression.AsyncFunctions["second"] = async _ =>
        {
            argumentsStarted[1].SetResult();
            await releaseArguments.Task;
            return 2;
        };

        var evaluationTask = expression.EvaluateAsync(CancellationToken.None);

        await WaitForAll(argumentsStarted);
        releaseArguments.SetResult();

        await Assert.That(await evaluationTask).IsEqualTo(2);
    }

    [Test]
    public async Task ShouldEvaluateListItemsConcurrentlyWhenEnabled()
    {
        var itemsStarted = CreateCompletionSources(3);
        var releaseItems = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var expression = CreateConcurrentExpression("(first(), second(), third())");

        for (var index = 0; index < itemsStarted.Length; index++)
        {
            var itemIndex = index;
            expression.AsyncFunctions[index switch
            {
                0 => "first",
                1 => "second",
                _ => "third"
            }] = async _ =>
            {
                itemsStarted[itemIndex].SetResult();
                await releaseItems.Task;
                return itemIndex + 1;
            };
        }

        var evaluationTask = expression.EvaluateAsync(CancellationToken.None);

        await WaitForAll(itemsStarted);
        releaseItems.SetResult();

        await Assert.That(await evaluationTask).IsEquivalentTo(new object[] { 1, 2, 3 });
    }

    [Test]
    public async Task ShouldKeepShortCircuitingBinaryExpressionsLazyDuringConcurrentEvaluation()
    {
        foreach (var (expressionText, expected) in new (string, object)[]
                 {
                     ("false && skipped()", false),
                     ("true || skipped()", true),
                     ("'value' ?? skipped()", "value")
                 })
        {
            var expression = CreateConcurrentExpression(expressionText);
            expression.AsyncFunctions["skipped"] = _ =>
                Task.FromException<object>(new InvalidOperationException("The right operand should not be evaluated."));

            await Assert.That(await expression.EvaluateAsync(CancellationToken.None)).IsEqualTo(expected);
        }
    }

    [Test]
    public async Task ShouldKeepConditionalBranchesLazyDuringConcurrentEvaluation()
    {
        foreach (var expressionText in new[] { "true ? selected() : skipped()", "if(true, selected(), skipped())" })
        {
            var expression = CreateConcurrentExpression(expressionText);
            expression.AsyncFunctions["selected"] = _ => Task.FromResult<object>(42);
            expression.AsyncFunctions["skipped"] = _ =>
                Task.FromException<object>(new InvalidOperationException("The unselected branch should not be evaluated."));

            await Assert.That(await expression.EvaluateAsync(CancellationToken.None)).IsEqualTo(42);
        }
    }

    [Test]
    public async Task ShouldUseBinaryHandlerResultBeforeConcurrentOperandEvaluation()
    {
        var expression = CreateConcurrentExpression("left() + right()");
        expression.AsyncFunctions["left"] = _ =>
            Task.FromException<object>(new InvalidOperationException("The operands should not be evaluated."));
        expression.AsyncFunctions["right"] = expression.AsyncFunctions["left"];
        expression.EvaluateBinaryAsync += args =>
        {
            args.Result = 42;
            return Task.CompletedTask;
        };

        await Assert.That(await expression.EvaluateAsync(CancellationToken.None)).IsEqualTo(42);
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
    public async Task ShouldEvaluateAndShortCircuitCoalesceAsync()
    {
        var expression = new Expression("[foo] ?? fallback()")
        {
            Parameters = { ["foo"] = "value" },
            AsyncFunctions =
            {
                ["fallback"] = _ => Task.FromException<object>(new InvalidOperationException())
            }
        };

        await Assert.That(await expression.EvaluateAsync(CancellationToken.None)).IsEqualTo("value");

        expression.Parameters["foo"] = null;
        expression.AsyncFunctions["fallback"] = _ => Task.FromResult<object>("fallback");

        await Assert.That(await expression.EvaluateAsync(CancellationToken.None)).IsEqualTo("fallback");
    }

    private static Expression CreateConcurrentExpression(string expression)
    {
        return new Expression(expression, new ExpressionConfiguration
        {
            Evaluation = new ExpressionEvaluationOptions
            {
                ConcurrentAsyncEvaluation = true
            }
        });
    }

    private static TaskCompletionSource[] CreateCompletionSources(int count = 2)
    {
        return Enumerable.Range(0, count)
            .Select(_ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously))
            .ToArray();
    }

    private static Task WaitForAll(TaskCompletionSource[] operationsStarted)
    {
        return Task.WhenAll(operationsStarted.Select(started => started.Task)).WaitAsync(TimeSpan.FromSeconds(5));
    }
}
