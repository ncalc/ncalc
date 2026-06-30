namespace NCalc.Helpers;

public sealed class MathHelperOptions(CultureInfo cultureInfo, ExpressionOptions options)
{
    public CultureInfo CultureInfo { get; } = cultureInfo;

    public bool AllowBooleanCalculation { get; } = options.HasFlag(ExpressionOptions.AllowBooleanCalculation);

    public bool DecimalAsDefault { get; } = options.HasFlag(ExpressionOptions.DecimalAsDefault);

    public bool OverflowProtection { get; } = options.HasFlag(ExpressionOptions.OverflowProtection);

    public bool AllowCharValues { get; } = options.HasFlag(ExpressionOptions.AllowCharValues);

    public bool LongAsDefault { get; } = options.HasFlag(ExpressionOptions.LongAsDefault);

    public static implicit operator MathHelperOptions(CultureInfo cultureInfo)
    {
        return new MathHelperOptions(cultureInfo, ExpressionOptions.None);
    }
}
