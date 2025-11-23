namespace NCalc.Handlers;

public class AsyncFunctionArgs(Guid id, AsyncExpression[] parameters) : EventArgs
{
    public Guid Id { get; } = id;

    public object? Result
    {
        get;
        set
        {
            field = value;
            HasResult = true;
        }
    }

    public AsyncExpression[] Parameters { get; } = parameters;

    public bool HasResult { get; private set; }

    public async ValueTask<object?[]> EvaluateParametersAsync()
    {
        var values = new object?[Parameters.Length];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = await Parameters[i].EvaluateAsync();
        }

        return values;
    }
}