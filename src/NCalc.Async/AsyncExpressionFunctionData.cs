namespace NCalc;

public class AsyncExpressionFunctionData(Guid id, AsyncExpression[] arguments, AsyncExpressionContext context)
{
    public Guid Id { get; } = id;
    private AsyncExpression[] Arguments { get; } = arguments;
    public AsyncExpressionContext Context { get; } = context;
    
    public AsyncExpression this[int index]
    {
        get => Arguments[index];
        set => Arguments[index] = value;
    }
}