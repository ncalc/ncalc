using Parlot;
using Parlot.Fluent;

namespace NCalc.Parser;

public class LogicalExpressionParserContext(string text) : ParseContext(new Scanner(text))
{
    public required bool UseDecimalsAsDefault { get; init; }

    public required CultureInfo CultureInfo { get; init; }
}