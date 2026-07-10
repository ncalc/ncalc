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

    public DefaultNumberType DefaultNumberType { get; init; } = DefaultNumberType.Double;
    public bool AllowBooleanCalculation { get; init; }
    public bool OverflowProtection { get; init; }
    public bool RoundAwayFromZero { get; init; }
}
