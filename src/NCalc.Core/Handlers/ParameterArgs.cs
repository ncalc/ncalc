namespace NCalc.Handlers;

public class ParameterArgs(Guid id) : EventArgs
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

    public bool HasResult { get; private set; }
}