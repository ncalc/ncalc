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
    /// <remarks>
    /// Without this option the binary operators <c>+</c>, <c>-</c>, <c>*</c> and <c>/</c> follow C# semantics and
    /// wrap around on overflow. The factorial operator is not affected by this option: a factorial that leaves the
    /// range of its type always throws an <see cref="OverflowException"/>, because a wrapped factorial is never a
    /// meaningful value.
    /// </remarks>
    public bool OverflowProtection { get; init; }

    /// <summary>
    /// Gets the midpoint rounding strategy.
    /// </summary>
    public MidpointRounding MidpointRounding { get; init; } = MidpointRounding.ToEven;
}
