using Parlot;
using Parlot.Fluent;

namespace NCalc.Parser;

public sealed class LogicalExpressionParserContext(string text, ExpressionOptions options)
    : ParseContext(new Scanner(text))
{
    /// <summary>
    /// Parser options containing culture info and argument separator settings.
    /// </summary>
    public LogicalExpressionParserOptions ParserOptions { get; init; } = LogicalExpressionParserOptions.Default;
    public ExpressionOptions Options { get; } = options;

    public CultureInfo CultureInfo { get; } = CultureInfo.CurrentCulture;

    public LogicalExpressionParserContext(string text, ExpressionOptions options, CultureInfo cultureInfo): this(text, options)
    {
        CultureInfo = cultureInfo;
    }
}