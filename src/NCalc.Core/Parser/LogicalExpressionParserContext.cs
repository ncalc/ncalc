using Parlot;
using Parlot.Fluent;

namespace NCalc.Parser;

public sealed class LogicalExpressionParserContext(string text, ExpressionOptions options)
    : ParseContext(new Scanner(text))
{
    public ExpressionOptions Options { get; } = options;

    public AdvancedExpressionOptions? AdvancedOptions { get; set; }

    public CultureInfo CultureInfo { get; } = CultureInfo.CurrentCulture;

    public LogicalExpressionParserContext(string text, ExpressionOptions options, CultureInfo cultureInfo): this(text, options)
    {
        CultureInfo = cultureInfo;
    }

    public LogicalExpressionParserContext(string text, ExpressionOptions options, CultureInfo cultureInfo, AdvancedExpressionOptions? advancedOptions): this(text, options)
    {
        CultureInfo = cultureInfo;
		AdvancedOptions = advancedOptions;
    }

    public LogicalExpressionParserContext(string text, ExpressionOptions options, AdvancedExpressionOptions? advancedOptions): this(text, options)
    {
		AdvancedOptions = advancedOptions;
    }
}