namespace NCalc;

public class ExpressionParameterData(Guid id, ExpressionContext context, CancellationToken ct)
{
    public Guid Id { get; } = id;
    public ExpressionContext Context { get; } = context;
    public CancellationToken CancellationToken { get; } = ct;
}