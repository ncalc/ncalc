namespace NCalc.Handlers;

public class UpdateParameterArgs(string name, Guid id, object? value) : EventArgs
{
    public Guid Id { get; } = id;

    public string Name { get; } = name;

    public object? Value => value;

    public bool UpdateParameterLists { get; set; } = true;
}