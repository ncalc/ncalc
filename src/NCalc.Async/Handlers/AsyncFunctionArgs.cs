namespace NCalc.Handlers;

public class AsyncFunctionArgs(Guid id, AsyncExpression[] parameters, CancellationToken ct) : EventArgs
{
    public Guid Id { get; } = id;

    public AsyncExpression[] Parameters { get; } = parameters;

    public CancellationToken CancellationToken { get; } = ct;

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

    public bool HasResult { get; private set; }

    public async ValueTask<object?[]> EvaluateParametersAsync(CancellationToken ct = default)
    {
        var values = new object?[Parameters.Length];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = await Parameters[i].EvaluateAsync(ct);
        }

        return values;
    }
}