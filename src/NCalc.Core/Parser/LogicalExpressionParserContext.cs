using Parlot;
using Parlot.Fluent;

namespace NCalc.Parser;

public sealed class LogicalExpressionParserContext(string text, ExpressionOptions options)
    : ParseContext(new Scanner(text))
{
    public ExpressionOptions Options { get; } = options;

    public CultureInfo CultureInfo { get; } = CultureInfo.CurrentCulture;

    public LogicalExpressionParserContext(string text, ExpressionOptions options, CultureInfo cultureInfo): this(text, options)
    {
        CultureInfo = cultureInfo;
    }
}