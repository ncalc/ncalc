namespace NCalc.Helpers;

public sealed class ComparisonOptions(CultureInfo cultureInfo, ExpressionOptions options)
{
    public CultureInfo CultureInfo { get; } = cultureInfo;

    public bool IsCaseInsensitive { get; } = options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer);

    public bool IsOrdinal { get; } = options.HasFlag(ExpressionOptions.OrdinalStringComparer);
}