namespace NCalc;

public class ExpressionFunctionData(Guid id, Expression[] arguments, ExpressionContext context) : IEnumerable<Expression>
{
    public Guid Id { get; } = id;
    private Expression[] Arguments { get; } = arguments;
    public ExpressionContext Context { get; } = context;

    public Expression this[int index]
    {
        get => Arguments[index];
        set => Arguments[index] = value;
    }

    public IEnumerator<Expression> GetEnumerator()
    {
        return ((IEnumerable<Expression>)Arguments).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Arguments.GetEnumerator();
    }
}