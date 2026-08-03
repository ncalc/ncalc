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
}
