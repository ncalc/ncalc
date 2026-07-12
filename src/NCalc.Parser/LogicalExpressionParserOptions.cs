namespace NCalc.Parser;

/// <summary>
/// Options for configuring the LogicalExpressionParser behavior.
/// </summary>
public sealed class LogicalExpressionParserOptions
{
    /// <summary>
    /// Parses single-quoted one-character values as <see cref="char"/>.
    /// </summary>
    public bool AllowCharValues { get; init; }

    /// <summary>
    /// Gets the default parsed floating point number type.
    /// </summary>
    public FloatingPointNumberType FloatingPointNumberType { get; init; } = FloatingPointNumberType.Double;

    /// <summary>
    /// Gets the default parsed integer number type.
    /// </summary>
    public IntegerNumberType IntegerNumberType { get; init; } = IntegerNumberType.Auto;

    /// <summary>
    /// The argument separator used to separate function arguments. Default is Comma.
    /// </summary>
    public ArgumentSeparator ArgumentSeparator { get; init; } = ArgumentSeparator.Comma;
}
