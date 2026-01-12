using Parlot;
using Parlot.Fluent;

namespace NCalc.Parser;

public sealed class LogicalExpressionParserContext(string text, ExpressionOptions? options = null, LogicalExpressionParserOptions? parserOptions = null, CancellationToken ct = default)
    : ParseContext(new Scanner(text), disableLoopDetection: true, cancellationToken: ct)
{
    /// <summary>
    /// Parser options containing culture info and argument separator settings.
    /// </summary>
    public readonly LogicalExpressionParserOptions ParserOptions = parserOptions ?? LogicalExpressionParserOptions.Default;

    /// <summary>
    /// Expression options for parsing, like decimal handling.
    /// </summary>
    public readonly ExpressionOptions Options = options ?? ExpressionOptions.None;
}