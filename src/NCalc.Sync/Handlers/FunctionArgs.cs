namespace NCalc.Handlers;

public class FunctionArgs(Expression[] parameters) : EventArgs
{
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
    
    public bool HasResult { get;  private set; }

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