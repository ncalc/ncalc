namespace NCalc.Parser;

/// <summary>
/// Options for configuring the LogicalExpressionParser behavior.
/// </summary>
/// <remarks>
/// This is different from ExpressionContextBase as it only contains parsing-related settings.
/// For instance the culture info is used during parsing to interpret number formats correctly, while the
/// ExpressionContextBase culture info is used during evaluation.
/// This class is immutable. Use the static methods to create instances with specific settings.
/// </remarks>
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
    public static LogicalExpressionParserOptions Default { get; } = new();

    /// <summary>
    /// Creates parser options with both culture info and argument separator.
    /// </summary>
    /// <param name="cultureInfo">The culture info to use.</param>
    /// <param name="argumentSeparator">The argument separator to use.</param>
    /// <returns>Parser options with the specified settings.</returns>
    public static LogicalExpressionParserOptions Create(CultureInfo? cultureInfo = null, ArgumentSeparator? argumentSeparator = null) => new()
    {
        CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture,
        ArgumentSeparator = argumentSeparator ?? ArgumentSeparator.Comma
    };

    /// <summary>
    /// Creates parser options from a culture info.
    /// </summary>
    public static LogicalExpressionParserOptions FromCultureInfo(CultureInfo cultureInfo) => Create(cultureInfo: cultureInfo);

    /// <summary>
    /// Creates parser options from an argument separator.
    /// </summary>
    public static LogicalExpressionParserOptions FromArgumentSeparator(ArgumentSeparator argumentSeparator) => Create(argumentSeparator: argumentSeparator);
}