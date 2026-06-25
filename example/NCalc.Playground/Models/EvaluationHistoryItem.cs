namespace NCalc.Playground.Models;

public class EvaluationHistoryItem
{
    public required string Expression { get; init; }
    public required string OriginalExpressionString { get; init; }
    public required object? Result { get; init; }
    public required Type ReturnType { get; init; }
    public required bool HasError { get; init; }
    public required IReadOnlyList<VariableInput> Parameters { get; init; }
    public required ExpressionOptions Options { get; init; }
}
