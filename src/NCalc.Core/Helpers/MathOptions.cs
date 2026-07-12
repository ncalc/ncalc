using NCalc.Parser;

namespace NCalc.Helpers;

public sealed class MathOptions
{
    public MathOptions()
    {
    }

    public MathOptions(DefaultNumberType defaultNumberType)
    {
        DefaultNumberType = defaultNumberType;
    }

    /// <summary>
    /// Gets the default parsed and coerced number type.
    /// </summary>
    public DefaultNumberType DefaultNumberType { get; init; } = DefaultNumberType.Double;

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
