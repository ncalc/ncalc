using Parlot.Fluent;

namespace NCalc.Benchmarks;

internal static partial class FluentExpressionParser
{
    internal static Parser<LogicalExpression> Create(LogicalExpressionParserOptions options, CultureInfo culture)
    {
        return CreateParserGrammar(options, culture);
    }
}
