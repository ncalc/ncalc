namespace NCalc.Handlers;

public class AsyncUpdateParameterArgs(string name, Guid id, object? value) : EventArgs
{
    public Guid Id { get; } = id;

    public object? Value => value;
    public string Name => name;

    public bool UpdateParameterLists { get; set; } = true;
}