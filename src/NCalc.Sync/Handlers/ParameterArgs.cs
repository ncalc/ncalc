namespace NCalc.Handlers;

public class ParameterArgs(Guid id, CancellationToken ct) : EventArgs
{
    private object? _result;
    public Guid Id { get; } = id;
    public CancellationToken CancellationToken { get; } = ct;

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
}