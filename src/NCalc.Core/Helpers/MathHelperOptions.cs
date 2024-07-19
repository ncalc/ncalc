using System.Runtime.CompilerServices;

namespace NCalc.Helpers;

public readonly struct MathHelperOptions(CultureInfo cultureInfo, ExpressionOptions options)
{
    public CultureInfo CultureInfo { get; } = cultureInfo;

    public bool AllowBooleanCalculation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => options.HasFlag(ExpressionOptions.AllowBooleanCalculation);
    }

    public bool DecimalAsDefault
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => options.HasFlag(ExpressionOptions.DecimalAsDefault);
    }

    public bool OverflowProtection
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => options.HasFlag(ExpressionOptions.OverflowProtection);
    }

    public bool AllowCharValues
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => options.HasFlag(ExpressionOptions.AllowCharValues);
    }

    public static implicit operator MathHelperOptions(CultureInfo cultureInfo)
    {
        return new MathHelperOptions(cultureInfo, ExpressionOptions.None);
    }
}