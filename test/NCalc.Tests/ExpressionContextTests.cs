using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using NCalc.Handlers;

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

    private static async Task AssertContainsKey<TKey, TValue>(
        IDictionary<TKey, TValue> dictionary,
        TKey key)
        where TKey : notnull
    {
        await Assert.That(dictionary.ContainsKey(key)).IsTrue();
    }
}