namespace NCalc.Handlers;

public class AsyncFunctionArgs : EventArgs
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

    public bool HasResult { get; set; }

    public AsyncExpression[] Parameters { get; init; } = [];

    public async Task<object?[]> EvaluateParametersAsync()
    {
        var values = new object?[Parameters.Length];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = await Parameters[i].EvaluateAsync();
        }

        return values;
    }
}