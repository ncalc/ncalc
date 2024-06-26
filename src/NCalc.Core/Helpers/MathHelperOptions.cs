namespace NCalc.Helpers;

public readonly record struct MathHelperOptions
{
    public required CultureInfo CultureInfo { get; init; }
    public required bool EnableBooleanCalculation { get; init; }
    public required bool UseDecimals { get; init; }
    public required bool OverflowProtection { get; init; }
    
    public static implicit operator MathHelperOptions(CultureInfo cultureInfo)
    {
        return new MathHelperOptions
        {
            CultureInfo = cultureInfo,
            OverflowProtection = false,
            UseDecimals = false,
            EnableBooleanCalculation = false
        };
    }
};