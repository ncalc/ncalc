namespace NCalc.Handlers;

public class FunctionArgs(Guid id, Expression[] parameters, CancellationToken ct) : EventArgs
{
    public Guid Id { get; } = id;

    public Expression[] Parameters { get; } = parameters;

    public CancellationToken CancellationToken { get; } = ct;

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

    public object?[] EvaluateParameters(CancellationToken ct)
    {
        var values = new object?[Parameters.Length];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = Parameters[i].Evaluate(ct);
        }

        return values;
    }
}