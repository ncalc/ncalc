namespace NCalc.Playground.Models;

public sealed class VariableRow(string name, string valueText)
{
    public string Name { get; set; } = name;

    public string ValueText { get; set; } = valueText;
}
