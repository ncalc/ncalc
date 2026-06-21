using System.Collections.Generic;

namespace NCalc.Playground.Models;

public sealed record EvaluationHistoryItem(
    string Expression,
    string OriginalExpressionString,
    string Result,
    string ReturnType,
    bool HasError,
    IReadOnlyList<VariableInput> Parameters)
{
    public string Status => HasError ? "Error" : "Evaluated";
}
