using NCalc.Exceptions;
using NCalc.Visitors;

namespace NCalc.Handlers;

public class FunctionData(
    Guid id,
    LogicalExpressionList arguments,
    ExpressionContext context,
    ILogicalExpressionVisitor<object?> syncVisitor,
    ILogicalExpressionVisitor<Task<object?>>? asyncVisitor,
    CancellationToken cancellationToken)
    : IReadOnlyList<LogicalExpression>
{
    private LogicalExpressionList Arguments { get; } = arguments;
    private ILogicalExpressionVisitor<object?> SyncVisitor { get; } = syncVisitor;
    private ILogicalExpressionVisitor<Task<object?>>? AsyncVisitor { get; } = asyncVisitor;

    public Guid Id { get; } = id;

    public ExpressionContext Context { get; } = context;
    public CancellationToken CancellationToken { get; } = cancellationToken;

    public LogicalExpression this[int index] => Arguments[index];

    public Task<object?> EvaluateAsync(int index)
    {
        if (AsyncVisitor is null)
            throw new NCalcEvaluationException(
                "Asynchronous binary value evaluation is not available in this context.");

        return Arguments[index].Accept(AsyncVisitor);
    }

    public object? Evaluate(int index)
    {
        return Arguments[index].Accept(SyncVisitor);
    }
    public int Count => Arguments.Count;

    public IEnumerator<LogicalExpression> GetEnumerator()
    {
        return Arguments.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Arguments.GetEnumerator();
    }
}