namespace NCalc.Helpers;

public class MathHelperOptions
{
    public required CultureInfo CultureInfo { get; init; }
    public required bool EnableBooleanCalculation { get; init; }
    public required bool UseDecimals { get; init; }
    public required bool OverflowProtection { get; init; }
    public required bool AllowCharValues { get; init; }
    
    public static implicit operator MathHelperOptions(CultureInfo cultureInfo)
    {
        return new MathHelperOptions
        {
            CultureInfo = cultureInfo,
            OverflowProtection = false,
            UseDecimals = false,
            AllowCharValues = false,
            EnableBooleanCalculation = false
        };
    }
}