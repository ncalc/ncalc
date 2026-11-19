using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using NCalc.Exceptions;
using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Visitors;

namespace NCalc.Tests;

public class ExpressionContextTests
{
    [Test]
    public async Task ShouldCreateCopyFromAnotherContext()
    {
        EvaluateParameterHandler handler = (_, _) => { };
        var original = new ExpressionContext
        {
            Parameters = new Dictionary<string, object> { ["value"] = 42 },
            DynamicParameters = new Dictionary<string, ExpressionParameter>(),
            AsyncParameters = new Dictionary<string, AsyncExpressionParameter>(),
            Functions = new Dictionary<string, ExpressionFunction>(),
            AsyncFunctions = new Dictionary<string, AsyncExpressionFunction>(),
            EvaluateParameterHandler = handler
        };

        var copy = new ExpressionContext(original);

        await Assert.That(copy).IsNotSameReferenceAs(original);
        await Assert.That(copy.Parameters).IsNotSameReferenceAs(original.Parameters);
        await Assert.That(copy.DynamicParameters).IsNotSameReferenceAs(original.DynamicParameters);
        await Assert.That(copy.AsyncParameters).IsNotSameReferenceAs(original.AsyncParameters);
        await Assert.That(copy.Functions).IsNotSameReferenceAs(original.Functions);
        await Assert.That(copy.AsyncFunctions).IsNotSameReferenceAs(original.AsyncFunctions);
        await Assert.That(copy.EvaluateParameterHandler).IsSameReferenceAs(handler);

        copy.Parameters["value"] = 21;

        await Assert.That(original.Parameters["value"]).IsEqualTo(42);
    }

    [Test]
    public async Task ShouldRejectNullContextWhenCopying()
    {
        await Assert.That(() => new ExpressionContext(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldPreserveComparerForAllContextDictionaries()
    {
        var original = new ExpressionContext
        {
            Parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["value"] = new AbandonedMutexException()
            },
            DynamicParameters = new SortedDictionary<string, ExpressionParameter>(StringComparer.OrdinalIgnoreCase)
            {
                ["dynamic"] = _ => 69
            },
            AsyncParameters = new OrderedDictionary<string, AsyncExpressionParameter>(StringComparer.OrdinalIgnoreCase)
            {
                ["async"] = _ => Task.FromResult<object>(08)
            },
            Functions = new ConcurrentDictionary<string, ExpressionFunction>(StringComparer.OrdinalIgnoreCase)
            {
                ["function"] = _ => 08
            },
            AsyncFunctions = new Dictionary<string, AsyncExpressionFunction>
            {
                ["asyncFunction"] = _ => Task.FromResult<object>(2001)
            }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase)
        };

        var copy = new ExpressionContext(original);

        await AssertContainsKey(copy.Parameters, "VALUE");
        await AssertContainsKey(copy.DynamicParameters, "DYNAMIC");
        await AssertContainsKey(copy.AsyncParameters, "ASYNC");
        await AssertContainsKey(copy.Functions, "FUNCTION");
        await AssertContainsKey(copy.AsyncFunctions, "ASYNCFUNCTION");
    }

    [Test]
    [Arguments(false, false)]
    [Arguments(true, false)]
    [Arguments(false, true)]
    [Arguments(true, true)]
    public async Task ShouldPreserveParametersAcrossIterations(bool evaluateAsync, bool frozen)
    {
        var values = new[] { 1, 2, 3 };
        var parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["values"] = values, ["offset"] = 10
        };
        var context = new ExpressionContext(frozen ? parameters.ToFrozenDictionary(parameters.Comparer) : parameters);
        var config = new ExpressionConfiguration { Evaluation = new() { IterateParameters = true } };
        var first = new Expression("VALUES + offset", config, context);
        var second = new Expression("VALUES + offset", config, context);

        foreach (var expression in new[] { first, first, second })
        {
            var result = evaluateAsync ? await expression.EvaluateAsync() : expression.Evaluate();
            await Assert.That((IList<object>)result).IsEquivalentTo(new object[] { 11, 12, 13 });
            await Assert.That(context.Parameters["values"]).IsSameReferenceAs(values);
        }
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ShouldPreserveParametersWhenIterationIsCanceled(bool evaluateAsync)
    {
        using var cancellation = new CancellationTokenSource();
        var values = new[] { 1, 2 };
        var context = new ExpressionContext { Parameters = { ["x"] = values } };
        object Cancel(FunctionData args)
        {
            cancellation.Cancel();
            args.CancellationToken.ThrowIfCancellationRequested();
            return 0;
        }

        context.Functions["stop"] = Cancel;
        context.AsyncFunctions["stop"] = args => Task.FromResult(Cancel(args));
        var config = new ExpressionConfiguration { Evaluation = new() { IterateParameters = true } };
        var expression = new Expression("x + stop()", config, context);
        async Task Evaluate()
        {
            if (evaluateAsync)
                await expression.EvaluateAsync(cancellation.Token);
            else
                expression.Evaluate(cancellation.Token);
        }

        await Assert.That(Evaluate).Throws<OperationCanceledException>();
        await Assert.That(context.Parameters["x"]).IsSameReferenceAs(values);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ShouldKeepSharedStoredChildInPrivateIterationContext(bool evaluateAsync)
    {
        var values = new[] { 3, 4 };
        var context = new ExpressionContext { Parameters = { ["x"] = values } };
        var parameterCalls = 0;
        var functionCalls = 0;
        var binaryCalls = 0;
        var asyncParameterCalls = 0;
        var asyncFunctionCalls = 0;
        var asyncBinaryCalls = 0;
        context.EvaluateParameterHandler = (_, _) => parameterCalls++;
        context.EvaluateFunctionHandler = (_, _) => functionCalls++;
        context.EvaluateBinaryHandler = _ => binaryCalls++;
        context.EvaluateAsyncParameterHandler = (_, _) => { asyncParameterCalls++; return Task.CompletedTask; };
        context.EvaluateAsyncFunctionHandler = (_, _) => { asyncFunctionCalls++; return Task.CompletedTask; };
        context.EvaluateBinaryAsyncHandler = _ => { asyncBinaryCalls++; return Task.CompletedTask; };
        context.Functions["f"] = _ => 10;
        var child = new Expression("x + f()", context);
        context.Parameters["inner"] = child;
        var outer = new Expression("inner + inner", IterationConfiguration(), context);

        for (var i = 0; i < 2; i++)
        {
            await AssertSequence(await Evaluate(outer, evaluateAsync), 26, 28);
            await Assert.That(context.Parameters["x"]).IsSameReferenceAs(values);
            await Assert.That(child.Context).IsSameReferenceAs(context);
        }

        await Assert.That(parameterCalls).IsEqualTo(16);
        await Assert.That(functionCalls).IsEqualTo(8);
        await Assert.That(binaryCalls).IsEqualTo(12);
        await Assert.That(asyncParameterCalls).IsEqualTo(evaluateAsync ? 16 : 0);
        await Assert.That(asyncFunctionCalls).IsEqualTo(evaluateAsync ? 8 : 0);
        await Assert.That(asyncBinaryCalls).IsEqualTo(evaluateAsync ? 12 : 0);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ShouldPreserveDistinctChildConfigurationFactoryAndFunctions(bool evaluateAsync)
    {
        var values = new[] { 1, 2 };
        var parent = new ExpressionContext(new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["X"] = values, ["offset"] = 10
        });
        var childContext = new ExpressionContext(new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["x"] = -1, ["offset"] = -100
        });
        ExpressionParameter childDynamic = _ => -3;
        childContext.DynamicParameters["v"] = childDynamic;
        parent.DynamicParameters["v"] = _ => 7;
        parent.Functions["own"] = _ => 999;
        childContext.Functions["own"] = _ => 1;
        var parentCalls = 0;
        var childCalls = 0;
        var parentFunctionCalls = 0;
        var childFunctionCalls = 0;
        parent.EvaluateParameterHandler = (_, _) => parentCalls++;
        childContext.EvaluateParameterHandler = (_, _) => childCalls++;
        parent.EvaluateFunctionHandler = (_, _) => parentFunctionCalls++;
        childContext.EvaluateFunctionHandler = (_, _) => childFunctionCalls++;
        var factory = new ContextRecordingFactory();
        var child = new Expression("own() + x + offset + v + 'z'",
            new ExpressionConfiguration { Evaluation = new() { StringConcat = true } },
            childContext, CultureInfo.InvariantCulture, factory);
        parent.Parameters["inner"] = child;
        var outer = new Expression("inner", IterationConfiguration(), parent,
            CultureInfo.GetCultureInfo("fr-FR"), new ContextRecordingFactory());

        for (var i = 0; i < 2; i++)
        {
            await AssertSequence(await Evaluate(outer, evaluateAsync), "11107z", "12107z");
            await Assert.That(parent.Parameters["X"]).IsSameReferenceAs(values);
            await Assert.That(child.Parameters["x"]).IsEqualTo(-1);
            await Assert.That(child.Parameters["offset"]).IsEqualTo(-100);
            await Assert.That(child.DynamicParameters["v"]).IsSameReferenceAs(childDynamic);
            await Assert.That(child.Context).IsSameReferenceAs(childContext);
        }

        await Assert.That(parentCalls).IsEqualTo(16);
        await Assert.That(childCalls).IsEqualTo(12);
        await Assert.That(parentFunctionCalls).IsEqualTo(4);
        await Assert.That(childFunctionCalls).IsEqualTo(4);
        await Assert.That(factory.Contexts.Count > 0).IsTrue();
        await Assert.That(factory.Contexts.All(c => !ReferenceEquals(c, childContext))).IsTrue();
        await Assert.That(factory.ChildOptionsOnly).IsTrue();
    }

    [Test]
    public async Task ShouldNotReplaceStoredChildContextWhileAwaiting()
    {
        var values = new[] { 1, 2 };
        var context = new ExpressionContext { Parameters = { ["x"] = values } };
        var child = new Expression("pause()", context);
        context.Parameters["inner"] = child;
        var entered = new TaskCompletionSource<ExpressionContext>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        context.AsyncFunctions["pause"] = async args =>
        {
            calls++;
            entered.TrySetResult(args.Context);
            await release.Task;
            return args.Context.Parameters["x"];
        };
        var outer = new Expression("inner", IterationConfiguration(), context);
        var evaluation = outer.EvaluateAsync().AsTask();
        try
        {
            var row = await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await Assert.That(child.Context).IsSameReferenceAs(context);
            await Assert.That(context.Parameters["x"]).IsSameReferenceAs(values);
            await Assert.That(row).IsNotSameReferenceAs(context);
            await Assert.That(row.Parameters["x"]).IsEqualTo(1);
        }
        finally
        {
            release.TrySetResult();
        }
        await AssertSequence(await evaluation, 1, 2);
        await Assert.That(calls).IsEqualTo(2);
    }

    [Test]
    [Arguments(false, false)]
    [Arguments(true, false)]
    [Arguments(false, true)]
    [Arguments(true, true)]
    public async Task ShouldPreserveNestedInputsAfterFailure(bool evaluateAsync, bool cancel)
    {
        var values = new[] { 1, 2 };
        var context = new ExpressionContext { Parameters = { ["x"] = values } };
        var child = new Expression("x + stop()", context);
        context.Parameters["inner"] = child;
        var outer = new Expression("inner", IterationConfiguration(), context);
        for (var i = 0; i < 2; i++)
        {
            using var cancellation = new CancellationTokenSource();
            context.Functions["stop"] = args =>
            {
                if (!cancel) throw new InvalidOperationException("test child failure");
                cancellation.Cancel();
                args.CancellationToken.ThrowIfCancellationRequested();
                return 0;
            };
            async Task Call() => await Evaluate(outer, evaluateAsync, cancellation.Token);
            if (cancel)
                await Assert.That(Call).Throws<OperationCanceledException>();
            else
                await Assert.That(Call).Throws<InvalidOperationException>();
            await Assert.That(context.Parameters["x"]).IsSameReferenceAs(values);
            await Assert.That(child.Context).IsSameReferenceAs(context);
        }
        context.Functions["stop"] = _ => 0;
        await AssertSequence(await Evaluate(outer, evaluateAsync), 1, 2);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ShouldKeepConstantAndMismatchedIterationBehavior(bool evaluateAsync)
    {
        var values = new[] { 1, 2 };
        var context = new ExpressionContext { Parameters = { ["x"] = values } };
        var constant = new Expression("5", IterationConfiguration(), context);
        for (var i = 0; i < 2; i++) await AssertSequence(await Evaluate(constant, evaluateAsync), 5, 5);
        var scalar = new Expression("5", IterationConfiguration());
        await Assert.That(await Evaluate(scalar, evaluateAsync)).IsEqualTo(5);
        var empty = new Expression("5", IterationConfiguration(),
            new ExpressionContext { Parameters = { ["x"] = Array.Empty<int>() } });
        await AssertSequence(await Evaluate(empty, evaluateAsync));
        context.Parameters["other"] = new[] { 9 };
        async Task Mismatch() => await Evaluate(constant, evaluateAsync);
        await Assert.That(Mismatch).Throws<NCalcException>();
        await Assert.That(context.Parameters["x"]).IsSameReferenceAs(values);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ShouldIterateReadOnlyAndComparerAwareInputs(bool evaluateAsync)
    {
        var values = new[] { 1, 2 };
        var source = new Dictionary<string, object> { ["x"] = values };
        foreach (var parameters in new IDictionary<string, object>[]
        {
            new Dictionary<string, object>(source, StringComparer.OrdinalIgnoreCase),
            new ConcurrentDictionary<string, object>(source, StringComparer.OrdinalIgnoreCase),
            new SortedDictionary<string, object>(source, StringComparer.OrdinalIgnoreCase),
            new SortedList<string, object>(source, StringComparer.OrdinalIgnoreCase),
            new OrderedDictionary<string, object>(source, StringComparer.OrdinalIgnoreCase),
            source.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase),
            new ReadOnlyDictionary<string, object>(source),
            source.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase),
            source.ToImmutableSortedDictionary(StringComparer.OrdinalIgnoreCase),
            source.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase).ToBuilder(),
            source.ToImmutableSortedDictionary(StringComparer.OrdinalIgnoreCase).ToBuilder()
        })
        {
            var formula = parameters is ReadOnlyDictionary<string, object> ? "x + 1" : "X + 1";
            var expression = new Expression(formula, IterationConfiguration(), new ExpressionContext(parameters));
            for (var i = 0; i < 2; i++) await AssertSequence(await Evaluate(expression, evaluateAsync), 2, 3);
            await Assert.That(parameters["x"]).IsSameReferenceAs(values);
        }
    }

    private static ExpressionConfiguration IterationConfiguration() =>
        new() { Evaluation = new() { IterateParameters = true } };

    [Test]
    [Arguments(false, false)]
    [Arguments(true, false)]
    [Arguments(false, true)]
    [Arguments(true, true)]
    public async Task ShouldPreserveImmutableBuilderComparersAcrossIterations(bool evaluateAsync, bool sorted)
    {
        var values = new[] { 1, 2 };
        var parameters = CreateImmutableBuilder<object>(sorted);
        parameters["x"] = values;
        var expression = new Expression("X + 1", IterationConfiguration(), new ExpressionContext(parameters));
        for (var i = 0; i < 2; i++)
        {
            await AssertSequence(await Evaluate(expression, evaluateAsync), 2, 3);
            await Assert.That(parameters["x"]).IsSameReferenceAs(values);
        }
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ShouldCopyImmutableBuilderComparersForAllDictionaries(bool sorted)
    {
        var original = new ExpressionContext
        {
            Parameters = CreateImmutableBuilder<object>(sorted),
            DynamicParameters = CreateImmutableBuilder<ExpressionParameter>(sorted),
            AsyncParameters = CreateImmutableBuilder<AsyncExpressionParameter>(sorted),
            Functions = CreateImmutableBuilder<ExpressionFunction>(sorted),
            AsyncFunctions = CreateImmutableBuilder<AsyncExpressionFunction>(sorted)
        };
        var value = new object();
        original.Parameters["value"] = value;
        original.DynamicParameters["dynamic"] = _ => 1;
        original.AsyncParameters["async"] = _ => Task.FromResult<object>(1);
        original.Functions["function"] = _ => 1;
        original.AsyncFunctions["asyncFunction"] = _ => Task.FromResult<object>(1);
        if (sorted)
            ((ImmutableSortedDictionary<string, object>.Builder)original.Parameters).ValueComparer = ReferenceEqualityComparer.Instance;
        else
            ((ImmutableDictionary<string, object>.Builder)original.Parameters).ValueComparer = ReferenceEqualityComparer.Instance;

        var copy = new ExpressionContext(original);
        await AssertContainsKey(copy.Parameters, "VALUE");
        await AssertContainsKey(copy.DynamicParameters, "DYNAMIC");
        await AssertContainsKey(copy.AsyncParameters, "ASYNC");
        await AssertContainsKey(copy.Functions, "FUNCTION");
        await AssertContainsKey(copy.AsyncFunctions, "ASYNCFUNCTION");
        object keyComparer = sorted
            ? ((ImmutableSortedDictionary<string, object>.Builder)copy.Parameters).KeyComparer
            : ((ImmutableDictionary<string, object>.Builder)copy.Parameters).KeyComparer;
        var valueComparer = sorted
            ? ((ImmutableSortedDictionary<string, object>.Builder)copy.Parameters).ValueComparer
            : ((ImmutableDictionary<string, object>.Builder)copy.Parameters).ValueComparer;
        await Assert.That(keyComparer).IsSameReferenceAs(StringComparer.OrdinalIgnoreCase);
        await Assert.That(valueComparer).IsSameReferenceAs(ReferenceEqualityComparer.Instance);
        copy.Parameters["VALUE"] = new object();
        await Assert.That(original.Parameters["value"]).IsSameReferenceAs(value);
    }

    private static IDictionary<string, TValue> CreateImmutableBuilder<TValue>(bool sorted) => sorted
        ? ImmutableSortedDictionary.CreateBuilder<string, TValue>(StringComparer.OrdinalIgnoreCase)
        : ImmutableDictionary.CreateBuilder<string, TValue>(StringComparer.OrdinalIgnoreCase);

    private static async Task<object> Evaluate(Expression expression, bool asyncMode, CancellationToken token = default) =>
        asyncMode ? await expression.EvaluateAsync(token) : expression.Evaluate(token);

    private static async Task AssertSequence(object result, params object[] expected)
    {
        var actual = (IList<object>)result;
        await Assert.That(actual.Count).IsEqualTo(expected.Length);
        for (var i = 0; i < expected.Length; i++) await Assert.That(actual[i]).IsEqualTo(expected[i]);
    }

    private sealed class ContextRecordingFactory : IEvaluationVisitorFactory
    {
        public List<ExpressionContext> Contexts { get; } = [];
        public bool ChildOptionsOnly { get; private set; } = true;
        public EvaluationVisitor CreateEvaluationVisitor(ExpressionContext context, ExpressionEvaluationOptions options,
            CultureInfo cultureInfo, CancellationToken cancellationToken = default)
        {
            Contexts.Add(context);
            ChildOptionsOnly &= options.StringConcat && cultureInfo.Equals(CultureInfo.InvariantCulture);
            return new EvaluationVisitor(context, options, cultureInfo, this, cancellationToken);
        }
        public AsyncEvaluationVisitor CreateAsyncEvaluationVisitor(ExpressionContext context, ExpressionEvaluationOptions options,
            CultureInfo cultureInfo, CancellationToken cancellationToken = default)
        {
            Contexts.Add(context);
            ChildOptionsOnly &= options.StringConcat && cultureInfo.Equals(CultureInfo.InvariantCulture);
            return new AsyncEvaluationVisitor(context, options, cultureInfo, this, cancellationToken);
        }
    }

    private static async Task AssertContainsKey<TKey, TValue>(
        IDictionary<TKey, TValue> dictionary,
        TKey key)
        where TKey : notnull
    {
        await Assert.That(dictionary.ContainsKey(key)).IsTrue();
    }
}
