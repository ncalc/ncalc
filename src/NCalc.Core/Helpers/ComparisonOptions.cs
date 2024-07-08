using System.Runtime.CompilerServices;

namespace NCalc.Helpers;


public readonly struct ComparisonOptions(CultureInfo cultureInfo, ExpressionOptions options)
{
    public CultureInfo CultureInfo { get; } = cultureInfo;

    public bool IsCaseInsensitive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer);
    }

    public bool IsOrdinal
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => options.HasFlag(ExpressionOptions.OrdinalStringComparer);
    }

    public static implicit operator ComparisonOptions(CultureInfo cultureInfo)
    {
        return new ComparisonOptions(cultureInfo, ExpressionOptions.None);
    }
}