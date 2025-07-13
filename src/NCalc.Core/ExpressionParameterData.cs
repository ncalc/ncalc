namespace NCalc;

public class ExpressionParameterData(Guid id, ExpressionContext context)
{
    public Guid Id { get; } = id;
    public ExpressionContext Context { get; } = context;
}