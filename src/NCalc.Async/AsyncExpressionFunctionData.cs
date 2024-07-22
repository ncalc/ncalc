namespace NCalc;

public class AsyncExpressionFunctionData(Guid id, AsyncExpression[] arguments, AsyncExpressionContext context) : IEnumerable<AsyncExpression>
{
    public Guid Id { get; } = id;
    private AsyncExpression[] Arguments { get; } = arguments;
    public AsyncExpressionContext Context { get; } = context;

    public AsyncExpression this[int index]
    {
        get => Arguments[index];
        set => Arguments[index] = value;
    }

    public IEnumerator<AsyncExpression> GetEnumerator()
    {
        return ((IEnumerable<AsyncExpression>)Arguments).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Arguments.GetEnumerator();
    }
}