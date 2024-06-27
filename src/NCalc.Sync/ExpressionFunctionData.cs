namespace NCalc;

public class ExpressionFunctionData(Guid id, Expression[] arguments, ExpressionContext context)
{
    public Guid Id { get; } = id;
    private Expression[] Arguments { get; } = arguments;
    public ExpressionContext Context { get; } = context;
    
    public Expression this[int index]
    {
        get => Arguments[index];
        set => Arguments[index] = value;
    }
}