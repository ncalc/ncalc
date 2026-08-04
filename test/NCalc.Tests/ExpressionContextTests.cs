using System.Collections.Frozen;
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
            Parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase),
            DynamicParameters = new SortedDictionary<string, ExpressionParameter>(StringComparer.OrdinalIgnoreCase),
            AsyncParameters = new OrderedDictionary<string, AsyncExpressionParameter>(StringComparer.OrdinalIgnoreCase),
            Functions = new ConcurrentDictionary<string, ExpressionFunction>(StringComparer.OrdinalIgnoreCase),
            AsyncFunctions = new Dictionary<string, AsyncExpressionFunction>().ToFrozenDictionary(StringComparer.OrdinalIgnoreCase)
        };

        var copy = new ExpressionContext(original)
        {
            Parameters =
            {
                ["value"] = 42
            },
            DynamicParameters =
            {
                ["dynamic"] = _ => 42
            },
            AsyncParameters =
            {
                ["async"] = _ => Task.FromResult<object>(42)
            },
            Functions =
            {
                ["function"] = _ => 42
            },
            AsyncFunctions =
            {
                ["asyncFunction"] = _ => Task.FromResult<object>(42)
            }
        };

        await Assert.That(copy.Parameters.ContainsKey("VALUE")).IsTrue();
        await Assert.That(copy.DynamicParameters.ContainsKey("DYNAMIC")).IsTrue();
        await Assert.That(copy.AsyncParameters.ContainsKey("ASYNC")).IsTrue();
        await Assert.That(copy.Functions.ContainsKey("FUNCTION")).IsTrue();
        await Assert.That(copy.AsyncFunctions.ContainsKey("ASYNCFUNCTION")).IsTrue();
    }
}
