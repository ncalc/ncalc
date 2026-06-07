namespace NCalc.Helpers;

public static partial class TypeHelper
{
    private static bool IsNullOrWhiteSpace(object? x)
    {
        switch (x)
        {
            case null:
            case string s when string.IsNullOrWhiteSpace(s):
            case char c when char.IsWhiteSpace(c):
                return true;
            default:
                return false;
        }
    }

    public static bool HasNullOrTypeConflict(object? a, object? b, ExpressionOptions options)
    {
        var na = IsNullOrWhiteSpace(a);
        var nb = IsNullOrWhiteSpace(b);

        if (options.HasFlag(ExpressionOptions.StrictTypeMatching) && !na && !nb && a!.GetType() != b!.GetType())
            return true;

        return na != nb;
    }

    public static StringComparer GetStringComparer(ComparisonOptions options)
    {
        return options.IsOrdinal switch
        {
            true when options.IsCaseInsensitive => StringComparer.OrdinalIgnoreCase,
            true => StringComparer.Ordinal,
            false when options.IsCaseInsensitive => StringComparer.CurrentCultureIgnoreCase,
            _ => StringComparer.CurrentCulture
        };
    }

    public static int CompareUsingMostPreciseType(object? a, object? b, ComparisonOptions options)
    {
        var mpt = GetMostPreciseType(a?.GetType(), b?.GetType());

        var aValue = a != null ? Convert.ChangeType(a, mpt, options.CultureInfo) : null;
        var bValue = b != null ? Convert.ChangeType(b, mpt, options.CultureInfo) : null;

        var comparer = GetStringComparer(options);

        return comparer.Compare(aValue, bValue);
    }
}
