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
    : IList<LogicalExpression>
{
    private LogicalExpressionList Arguments { get; } = arguments;
    private ILogicalExpressionVisitor<object?> SyncVisitor { get; } = syncVisitor;
    private ILogicalExpressionVisitor<Task<object?>>? AsyncVisitor { get; } = asyncVisitor;

    public Guid Id { get; } = id;

    public ExpressionContext Context { get; } = context;
    public CancellationToken CancellationToken { get; } = cancellationToken;

    public LogicalExpression this[int index]
    {
        get => Arguments[index];
        set => Arguments[index] = value;
    }

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

    public void Add(LogicalExpression item) => Arguments.Add(item);

    public void Clear() => Arguments.Clear();

    public bool Contains(LogicalExpression item) => Arguments.Contains(item);

    public void CopyTo(LogicalExpression[] array, int arrayIndex) => Arguments.CopyTo(array, arrayIndex);

    public bool Remove(LogicalExpression item) => Arguments.Remove(item);

    public int Count => Arguments.Count;

    public bool IsReadOnly => Arguments.IsReadOnly;

    public int IndexOf(LogicalExpression item)
    {
        return Arguments.IndexOf(item);
    }

    public void Insert(int index, LogicalExpression item)
    {
        Arguments.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        Arguments.RemoveAt(index);
    }

    public IEnumerator<LogicalExpression> GetEnumerator()
    {
        return Arguments.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Arguments.GetEnumerator();
    }
}
