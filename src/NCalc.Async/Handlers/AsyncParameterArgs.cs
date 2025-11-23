namespace NCalc.Handlers;

public class AsyncParameterArgs(Guid id) : EventArgs
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

    public bool HasResult { get; private set; }
}