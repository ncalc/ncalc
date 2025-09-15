namespace NCalc.Parser;

/// <summary>
/// Options for configuring the LogicalExpressionParser behavior.
/// </summary>
public sealed record LogicalExpressionParserOptions
{
    /// <summary>
    /// The culture info used for parsing expressions.
    /// </summary>
    public CultureInfo CultureInfo { get; private init; } = CultureInfo.CurrentCulture;

    /// <summary>
    /// The argument separator used to separate function arguments. Default is Comma.
    /// </summary>
    public ArgumentSeparator ArgumentSeparator { get; private init; } = ArgumentSeparator.Comma;

    /// <summary>
    /// Gets the default parser options.
    /// </summary>
    public static LogicalExpressionParserOptions Default => new();

    /// <summary>
    /// Creates parser options with the specified culture info.
    /// </summary>
    /// <param name="cultureInfo">The culture info to use.</param>
    /// <returns>Parser options with the specified culture info.</returns>
    public static LogicalExpressionParserOptions WithCultureInfo(CultureInfo cultureInfo) => new()
    {
        CultureInfo = cultureInfo
    };

    /// <summary>
    /// Creates parser options with the specified argument separator.
    /// </summary>
    /// <param name="argumentSeparator">The argument separator to use.</param>
    /// <returns>Parser options with the specified argument separator.</returns>
    public static LogicalExpressionParserOptions WithArgumentSeparator(ArgumentSeparator argumentSeparator) => new()
    {
        ArgumentSeparator = argumentSeparator
    };

    /// <summary>
    /// Creates parser options with both culture info and argument separator.
    /// </summary>
    /// <param name="cultureInfo">The culture info to use.</param>
    /// <param name="argumentSeparator">The argument separator to use.</param>
    /// <returns>Parser options with the specified settings.</returns>
    public static LogicalExpressionParserOptions Create(CultureInfo cultureInfo, ArgumentSeparator argumentSeparator) => new()
    {
        CultureInfo = cultureInfo,
        ArgumentSeparator = argumentSeparator
    };

    /// <summary>
    /// Implicitly creates parser options from a CultureInfo.
    /// </summary>
    /// <param name="cultureInfo">The culture info to use.</param>
    public static implicit operator LogicalExpressionParserOptions(CultureInfo cultureInfo) => new()
    {
        CultureInfo = cultureInfo
    };
}