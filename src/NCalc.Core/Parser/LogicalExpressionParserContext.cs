using Parlot;
using Parlot.Fluent;

namespace NCalc.Parser;

public class LogicalExpressionParserContext(string text, ExpressionOptions options)
    : ParseContext(new Scanner(text))
{
    public ExpressionOptions Options { get; } = options;
}