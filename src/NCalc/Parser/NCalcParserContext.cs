using Parlot;
using Parlot.Fluent;

namespace NCalc.Parser;

public class NCalcParserContext(string text) : ParseContext(new Scanner(text))
{
    public required bool UseDecimalsAsDefault { get; init; }
}