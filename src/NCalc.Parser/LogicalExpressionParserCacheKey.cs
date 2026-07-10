namespace NCalc.Parser;

internal readonly record struct LogicalExpressionParserCacheKey(
    bool AllowCharValues,
    DefaultNumberType DefaultNumberType,
    ArgumentSeparator ArgumentSeparator,
    string CultureName)
{
    public LogicalExpressionParserCacheKey(LogicalExpressionParserOptions options, CultureInfo culture)
        : this(
            options.AllowCharValues,
            options.DefaultNumberType,
            options.ArgumentSeparator,
            culture.Name)
    {
    }
}