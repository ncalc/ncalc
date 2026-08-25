namespace NCalc;

internal readonly record struct LogicalExpressionParserCacheKey(
    bool AllowCharValues,
    bool DisallowSingleEquals,
    FloatingPointNumberType FloatingPointNumberType,
    IntegerNumberType IntegerNumberType,
    ArgumentSeparator ArgumentSeparator,
    string CultureName)
{
    public LogicalExpressionParserCacheKey(LogicalExpressionParserOptions options, CultureInfo culture)
        : this(
            options.AllowCharValues,
            options.DisallowSingleEquals,
            options.FloatingPointNumberType,
            options.IntegerNumberType,
            options.ArgumentSeparator,
            culture.Name)
    {
    }
}
