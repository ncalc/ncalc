using Parlot;
using Parlot.Fluent;

namespace NCalc.Parser;

public sealed class LogicalExpressionParserContext(string text, ExpressionOptions options, CancellationToken ct = default)
    : ParseContext(new Scanner(text), false, true, ct)
{
    /// <summary>
    /// Parser options containing culture info and argument separator settings.
    /// </summary>
    public LogicalExpressionParserOptions ParserOptions { get; init; } = LogicalExpressionParserOptions.Default;

    public ExpressionOptions Options { get; } = options;

    public CultureInfo CultureInfo { get; } = CultureInfo.CurrentCulture;

    public LogicalExpressionParserContext(string text, ExpressionOptions options, CultureInfo cultureInfo, CancellationToken ct = default) : this(text, options, ct)
    {
        CultureInfo = cultureInfo;
    }
}