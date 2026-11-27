using NCalc.Exceptions;
using NCalc.LambdaCompilation;

namespace NCalc.Tests;

/// <summary>
/// Deep expression trees are evaluated recursively. Without a guard, a long operator chain overflows the stack
/// and kills the process, which cannot be caught. The visitors must instead fail with a catchable exception, which
/// the evaluation and lambda compilation entry points wrap in <see cref="NCalcEvaluationException"/>.
/// </summary>
public class StackDepthTests
{
    private const int ChainLength = 50_000;
    private const int SmallStackSize = 512 * 1024;

    private static readonly string BinaryChain = string.Join("+", Enumerable.Repeat("1", ChainLength));
    private static readonly string UnaryChain = new string('-', ChainLength) + "1";

    [Test]
    public async Task Should_Throw_Instead_Of_Overflowing_The_Stack_When_Evaluating_A_Deep_Binary_Chain()
    {
        var expression = Parse(BinaryChain);

        var exception = RunOnSmallStack(() => expression.Evaluate(CancellationToken.None));

        await Assert.That(exception).IsTypeOf<NCalcEvaluationException>();
        await Assert.That(exception.InnerException).IsTypeOf<InsufficientExecutionStackException>();
    }

    [Test]
    public async Task Should_Throw_Instead_Of_Overflowing_The_Stack_When_Evaluating_A_Deep_Unary_Chain()
    {
        var expression = Parse(UnaryChain);

        var exception = RunOnSmallStack(() => expression.Evaluate(CancellationToken.None));

        await Assert.That(exception).IsTypeOf<NCalcEvaluationException>();
        await Assert.That(exception.InnerException).IsTypeOf<InsufficientExecutionStackException>();
    }

    [Test]
    public async Task Should_Throw_Instead_Of_Overflowing_The_Stack_When_Evaluating_Asynchronously()
    {
        var expression = Parse(BinaryChain);

        var exception = RunOnSmallStack(() => expression.EvaluateAsync(CancellationToken.None).AsTask().GetAwaiter().GetResult());

        await Assert.That(exception).IsTypeOf<NCalcEvaluationException>();
        await Assert.That(exception.InnerException).IsTypeOf<InsufficientExecutionStackException>();
    }

    [Test]
    public async Task Should_Throw_Instead_Of_Overflowing_The_Stack_When_Compiling_A_Lambda()
    {
        var expression = Parse(BinaryChain);

        var exception = RunOnSmallStack(() => expression.ToLambda<int>());

        await Assert.That(exception).IsTypeOf<NCalcEvaluationException>();
        await Assert.That(exception.InnerException).IsTypeOf<InsufficientExecutionStackException>();
    }

    [Test]
    public async Task Should_Throw_Instead_Of_Overflowing_The_Stack_When_Extracting_Parameters()
    {
        var expression = Parse(BinaryChain);

        var exception = RunOnSmallStack(() => expression.GetParameterNames());

        await Assert.That(exception).IsTypeOf<InsufficientExecutionStackException>();
    }

    [Test]
    public async Task Should_Throw_Instead_Of_Overflowing_The_Stack_When_Serializing()
    {
        var expression = Parse(BinaryChain);

        var exception = RunOnSmallStack(() => expression.LogicalExpression.ToExpressionString());

        await Assert.That(exception).IsTypeOf<InsufficientExecutionStackException>();
    }

    [Test]
    public async Task Should_Still_Evaluate_A_Moderate_Chain_On_A_Small_Stack()
    {
        var expression = Parse(string.Join("+", Enumerable.Repeat("1", 100)));
        object result = null;

        var exception = RunOnSmallStack(() => result = expression.Evaluate(CancellationToken.None));

        await Assert.That(exception).IsNull();
        await Assert.That(result).IsEqualTo(100);
    }

    // The expression is parsed on the test thread: the parser handles long chains iteratively, and the point of
    // these tests is the recursion in the visitors.
    private static Expression Parse(string text)
    {
        return new Expression(text) { LogicalExpression = LogicalExpressionParser.Parse(text) };
    }

    // A thread with a small stack makes the guard trip at a predictable depth, so the chain can stay short enough
    // to parse quickly while still being far deeper than the stack can hold.
    private static Exception RunOnSmallStack(Action action)
    {
        Exception captured = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                captured = exception;
            }
        }, SmallStackSize);

        thread.Start();
        thread.Join();

        return captured;
    }
}
