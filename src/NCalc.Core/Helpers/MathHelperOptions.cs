using System.Runtime.CompilerServices;

namespace NCalc.Helpers;

public readonly struct MathHelperOptions(CultureInfo cultureInfo, ExpressionOptions options)
{
#if NET8_0_OR_GREATER
    public readonly bool IsDynamicCodeSupported { get; } = System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported;
#else
    public readonly bool IsDynamicCodeSupported { get; } = true;
#endif

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

    public bool LongAsDefault
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => options.HasFlag(ExpressionOptions.LongAsDefault);
    }

    public static implicit operator MathHelperOptions(CultureInfo cultureInfo)
    {
        return new MathHelperOptions(cultureInfo, ExpressionOptions.None);
    }
}