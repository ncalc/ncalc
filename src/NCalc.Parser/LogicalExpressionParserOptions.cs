namespace NCalc.Parser;

/// <summary>
/// Options for configuring the LogicalExpressionParser behavior.
/// </summary>
public sealed class LogicalExpressionParserOptions
{
    public bool AllowCharValues { get; init; }

    public DefaultNumberType DefaultNumberType { get; init; } = DefaultNumberType.Double;

    /// <summary>
    /// The argument separator used to separate function arguments. Default is Comma.
    /// </summary>
    public ArgumentSeparator ArgumentSeparator { get; init; } = ArgumentSeparator.Comma;
}