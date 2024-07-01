namespace NCalc;

public class AsyncExpressionParameterData(Guid id, AsyncExpressionContext context)
{
    public Guid Id { get; } = id;
    public AsyncExpressionContext Context { get; } = context;
}