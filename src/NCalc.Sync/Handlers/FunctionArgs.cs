namespace NCalc.Handlers;

public class FunctionArgs(Guid id, Expression[] parameters) : EventArgs
{
    public Guid Id { get; } = id;

    private object? _result;

    public object? Result
    {
        get => _result;
        set
        {
            _result = value;
            HasResult = true;
        }
    }

    public Expression[] Parameters { get; } = parameters;

    public bool HasResult { get; private set; }

    public object?[] EvaluateParameters()
    {
        var values = new object?[Parameters.Length];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = Parameters[i].Evaluate();
        }

        return values;
    }
}