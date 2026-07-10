using Parlot;
using Parlot.Fluent;

namespace NCalc.Parser;

public sealed class LogicalExpressionParseContext(
    string text,
    LogicalExpressionParserOptions? options = null,
    CancellationToken cancellationToken = default)
    : ParseContext(new Scanner(text), useNewLines: false, disableLoopDetection: true, cancellationToken)
{
    public LogicalExpressionParserOptions Options { get; } = options ?? new();
}