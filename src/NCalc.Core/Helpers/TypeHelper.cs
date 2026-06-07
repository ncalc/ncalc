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

    public static ComparisonResult CompareUsingMostPreciseType(object? a, object? b, ComparisonOptions options)
    {
        var mpt = GetMostPreciseType(a?.GetType(), b?.GetType());

        if (mpt == typeof(double))
        {
            var left = Convert.ToDouble(a, options.CultureInfo);
            var right = Convert.ToDouble(b, options.CultureInfo);

            if (double.IsNaN(left) || double.IsNaN(right))
                return ComparisonResult.Unordered;

            return left.CompareTo(right) switch
            {
                < 0 => ComparisonResult.Less,
                > 0 => ComparisonResult.Greater,
                _ => ComparisonResult.Equal
            };
        }

        if (mpt == typeof(float))
        {
            var left = Convert.ToSingle(a, options.CultureInfo);
            var right = Convert.ToSingle(b, options.CultureInfo);

            if (float.IsNaN(left) || float.IsNaN(right))
                return ComparisonResult.Unordered;

            return left.CompareTo(right) switch
            {
                < 0 => ComparisonResult.Less,
                > 0 => ComparisonResult.Greater,
                _ => ComparisonResult.Equal
            };
        }

        var aValue = a != null ? Convert.ChangeType(a, mpt, options.CultureInfo) : null;
        var bValue = b != null ? Convert.ChangeType(b, mpt, options.CultureInfo) : null;

        var comparer = GetStringComparer(options);

        return comparer.Compare(aValue, bValue) switch
        {
            < 0 => ComparisonResult.Less,
            > 0 => ComparisonResult.Greater,
            _ => ComparisonResult.Equal
        };
    }
}
