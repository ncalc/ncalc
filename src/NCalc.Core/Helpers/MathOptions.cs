namespace NCalc.Helpers;

public sealed class MathOptions
{
    public MathOptions()
    {
    }

    public MathOptions(NumberType defaultNumberType)
    {
        DefaultNumberType = defaultNumberType;
    }

    /// <summary>
    /// Gets the number type used when coercing string values and choosing math function precision.
    /// </summary>
    public NumberType DefaultNumberType { get; init; } = NumberType.Double;

    /// <summary>
    /// Allows arithmetic operations with <see cref="bool"/> values.
    /// </summary>
    public bool AllowBooleanCalculation { get; init; }

    /// <summary>
    /// Checks arithmetic binary operations for overflow.
    /// </summary>
    public bool OverflowProtection { get; init; }

    /// <summary>
    /// Uses <see cref="MidpointRounding.AwayFromZero"/> for the built-in Round function.
    /// </summary>
    public bool RoundAwayFromZero { get; init; }
}
