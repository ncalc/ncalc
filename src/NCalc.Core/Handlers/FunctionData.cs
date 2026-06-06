using NCalc.Extensions;

namespace NCalc.Handlers;

public class FunctionData(Guid id, LogicalExpressionList arguments, ExpressionContext context, CancellationToken cancellationToken) : IList<LogicalExpression>
{
    private LogicalExpressionList Arguments { get; } = arguments;

    public Guid Id { get; } = id;

    public ExpressionContext Context { get; } = context;
    public CancellationToken CancellationToken { get; } = cancellationToken;

    public LogicalExpression this[int index]
    {
        get => Arguments[index];
        set => Arguments[index] = value;
    }

    public ValueTask<object?> EvaluateAsync(int index)
    {
        return Arguments[index].EvaluateAsync(Context, CancellationToken);
    }

    public object? Evaluate(int index)
    {
        var valueTask = EvaluateAsync(index);

        if(valueTask.IsCompletedSuccessfully)
            return valueTask.Result;

        return valueTask.AsTask().GetAwaiter().GetResult();
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
