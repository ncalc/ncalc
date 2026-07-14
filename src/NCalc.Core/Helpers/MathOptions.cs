namespace NCalc.Helpers;

public sealed class MathOptions
{
    /// <summary>
    /// Gets the default parsed floating point number type.
    /// </summary>
    public FloatingPointNumberType FloatingPointNumberType { get; init; } = FloatingPointNumberType.Double;

    /// <summary>
    /// Gets the default parsed integer number type.
    /// </summary>
    public IntegerNumberType IntegerNumberType { get; init; } = IntegerNumberType.Int32;

    /// <summary>
    /// Allows arithmetic operations with <see cref="bool"/> values.
    /// </summary>
    public bool AllowBooleanCalculation { get; init; }

    /// <summary>
    /// Checks arithmetic binary operations for overflow.
    /// </summary>
    public bool OverflowProtection { get; init; }

    /// <summary>
    /// Gets the midpoint rounding strategy.
    /// </summary>
    public MidpointRounding MidpointRounding { get; init; } = MidpointRounding.ToEven;
}
