#nullable enable
using System.Runtime.CompilerServices;
using NCalc.Factories;
using NCalc.Helpers;
using NCalc.LambdaCompilation;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NCalc.Tests;

public abstract class YamlEvaluationTests<TSuite>
{
    public static IEnumerable<ExpressionTestCase> Cases() => YamlTestData.GetCases(typeof(TSuite).Name);

    [Test]
    [MethodDataSource(nameof(Cases))]
    public async Task ShouldMatchOutput(ExpressionTestCase testCase)
    {
        if (testCase.Operation is not ("evaluate" or "roundTrip"))
        {
            await AssertCase(testCase, () => EvaluateOperation(testCase));
            return;
        }

        if (testCase.CompileToLambda)
        {
            if (!RuntimeFeature.IsDynamicCodeCompiled)
                Skip.Test("Lambda compilation is not supported under NativeAOT.");
            await AssertCase(testCase, () => Evaluate(testCase, EvaluationKind.Lambda));
            return;
        }

        await Task.WhenAll(
            AssertCase(testCase, () => Evaluate(testCase, EvaluationKind.Sync)),
            AssertCase(testCase, () => Evaluate(testCase, EvaluationKind.Async)));
    }

    private static ValueTask<object?> EvaluateOperation(ExpressionTestCase testCase)
    {
        var expression = testCase.CreateExpression();
        object result = testCase.Operation switch
        {
            "parameterCount" => expression.GetParameterNames(CancellationToken.None).Count,
            "functionCount" => expression.GetFunctionNames(CancellationToken.None).Count,
            "functionNames" => expression.GetFunctionNames(CancellationToken.None).Order(StringComparer.Ordinal).ToArray(),
            "serialize" => expression.ToExpressionString(cancellationToken: CancellationToken.None),
            "serializeWithParameters" => expression.ToExpressionString(evaluateParameters: true,
                cancellationToken: CancellationToken.None),
            "escapeLike" => LikeOperatorHelper.EscapeLike(testCase.Input!),
            "hasErrors" => expression.HasErrors(CancellationToken.None),
            "parse" => LogicalExpressionParser.Parse(new LogicalExpressionParseContext(testCase.Input!,
                testCase.Configuration.Parsing), testCase.Culture),
            "createLogicalExpression" => LogicalExpressionFactory.Create(testCase.Input!,
                testCase.Configuration.Parsing, testCase.Culture, CancellationToken.None),
            _ => throw new InvalidDataException($"Unsupported operation in {testCase.Name}.")
        };
        return ValueTask.FromResult<object?>(result);
    }

    private static ValueTask<object?> Evaluate(ExpressionTestCase testCase, EvaluationKind kind)
    {
        var expression = testCase.CreateExpression();
        if (testCase.Operation == "roundTrip")
        {
            var json = JsonSerializer.Serialize(expression.GetLogicalExpression(CancellationToken.None),
                NCalcTestJsonContext.Default.LogicalExpression);
            expression.LogicalExpression = JsonSerializer.Deserialize(json, NCalcTestJsonContext.Default.LogicalExpression);
        }

        return testCase.ResultType switch
        {
            "object" => Evaluate<object>(expression, kind),
            "int" => Evaluate<int>(expression, kind),
            "bool" => Evaluate<bool>(expression, kind),
            "double" => Evaluate<double>(expression, kind),
            "float" => Evaluate<float>(expression, kind),
            "decimal" => Evaluate<decimal>(expression, kind),
            "string" => Evaluate<string>(expression, kind),
            _ => throw new InvalidDataException($"Unsupported result type in {testCase.Name}.")
        };
    }

    private static async ValueTask<object?> Evaluate<T>(Expression expression, EvaluationKind kind) => kind switch
    {
        EvaluationKind.Sync => expression.Evaluate<T>(CancellationToken.None),
        EvaluationKind.Async => await expression.EvaluateAsync<T>(CancellationToken.None),
        EvaluationKind.Lambda => expression.ToLambda<T>(CancellationToken.None)(),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    private static async Task AssertCase(ExpressionTestCase testCase, Func<ValueTask<object?>> evaluate)
    {
        if (testCase.Throws is { } exceptionType)
        {
            await Assert.ThrowsAsync(exceptionType, async () =>
            {
                await evaluate();
            });
            return;
        }

        await AssertResult(await evaluate(), testCase.Output);
    }

    private static async Task AssertResult(object? result, object? expected)
    {
        if (expected is object?[] expectedItems)
        {
            await Assert.That(result).IsNotNull();
            var items = ((IEnumerable)result!).Cast<object?>().ToArray();
            await Assert.That(items.Length).IsEqualTo(expectedItems.Length);
            for (var i = 0; i < expectedItems.Length; i++)
                await AssertResult(items[i], expectedItems[i]);
            return;
        }

        await Assert.That(result).IsEqualTo(expected)
            .Because($"Expected type {expected?.GetType().Name ?? "null"}; received {result?.GetType().Name ?? "null"}.");
    }

    private enum EvaluationKind { Sync, Async, Lambda }
}
