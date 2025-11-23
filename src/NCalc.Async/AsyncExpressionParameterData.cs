namespace NCalc;

public class AsyncExpressionParameterData(Guid id, AsyncExpressionContext context, CancellationToken ct)
{
    public Guid Id { get; } = id;
    public AsyncExpressionContext Context { get; } = context;
    public CancellationToken CancellationToken { get; } = ct;
}