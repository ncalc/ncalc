namespace NCalc.Handlers;

public class ParameterArgs(Guid id, CancellationToken ct) : EventArgs
{
    public Guid Id { get; } = id;
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
}