namespace NCalc.Handlers;

public class FunctionEventArgs(FunctionData parameters) : EventArgs
{
    public FunctionData Parameters { get; } = parameters;

    public CancellationToken CancellationToken => Parameters.CancellationToken;

    public Guid Id => Parameters.Id;

    public ExpressionContext Context => Parameters.Context;

    public object? Result
    {
        get;
        set
        {
            field = value;
            HasResult = true;
        }
    }

    public bool HasResult { get; private set; }
}
