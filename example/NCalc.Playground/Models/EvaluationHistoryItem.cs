using System.Collections.Generic;
using NCalc;

namespace NCalc.Playground.Models;

public sealed record EvaluationHistoryItem(
    string Expression,
    string OriginalExpressionString,
    string Result,
    string ReturnType,
    bool HasError,
    IReadOnlyList<VariableInput> Parameters,
    ExpressionOptions Options);
