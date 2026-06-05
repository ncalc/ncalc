namespace NCalc.Parser;

/// <summary>
/// Options for configuring the LogicalExpressionParser behavior.
/// </summary>
public sealed record LogicalExpressionParserOptions
{
    public bool AllowCharValues { get; init; }

    public bool DecimalAsDefault { get; init; }

    public bool LongAsDefault { get; init; }

    /// <summary>
    /// The culture info used for parsing expressions.
    /// </summary>
    public CultureInfo CultureInfo { get; init; } = CultureInfo.CurrentCulture;

    /// <summary>
    /// The argument separator used to separate function arguments. Default is Comma.
    /// </summary>
    public LogicalExpressionArgumentSeparator ArgumentSeparator { get; init; } =
        LogicalExpressionArgumentSeparator.Comma;

    public LogicalExpressionParserOptions()
    {
    }

    public LogicalExpressionParserOptions(CultureInfo cultureInfo, LogicalExpressionArgumentSeparator separator)
    {
        CultureInfo = cultureInfo;
        ArgumentSeparator = separator;
    }
}