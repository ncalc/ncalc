namespace NCalc.Parser;

internal readonly record struct LogicalExpressionParserCacheKey(
    bool AllowCharValues,
    FloatingPointNumberType FloatingPointNumberType,
    IntegerNumberType IntegerNumberType,
    ArgumentSeparator ArgumentSeparator,
    string CultureName)
{
    public LogicalExpressionParserCacheKey(LogicalExpressionParserOptions options, CultureInfo culture)
        : this(
            options.AllowCharValues,
            options.FloatingPointNumberType,
            options.IntegerNumberType,
            options.ArgumentSeparator,
            culture.Name)
    {
    }
}
