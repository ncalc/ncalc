namespace NCalc;

internal readonly record struct LogicalExpressionParserCacheKey(
    bool AllowCharValues,
    bool DisallowSingleEquals,
    FloatingPointNumberType FloatingPointNumberType,
    IntegerNumberType IntegerNumberType,
    ArgumentSeparator ArgumentSeparator)
{
    public LogicalExpressionParserCacheKey(LogicalExpressionParserOptions options)
        : this(
            options.AllowCharValues,
            options.DisallowSingleEquals,
            options.FloatingPointNumberType,
            options.IntegerNumberType,
            options.ArgumentSeparator)
    {
    }
}
