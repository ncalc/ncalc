namespace NCalc.Handlers;

public class ParameterData(Guid id, ExpressionContext context, CancellationToken cancellationToken)
{
    public Guid Id { get; } = id;
    public ExpressionContext Context { get; } = context;
    public CancellationToken CancellationToken { get; } = cancellationToken;
}