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

    public static bool HasNullOrTypeConflict(object? a, object? b, bool strictTypeMatching)
    {
        var na = IsNullOrWhiteSpace(a);
        var nb = IsNullOrWhiteSpace(b);

        if (strictTypeMatching && !na && !nb && a!.GetType() != b!.GetType())
            return true;

        return na != nb;
    }

    public static bool IsOrdinal(StringComparer comparer)
    {
        return ReferenceEquals(comparer, StringComparer.Ordinal) ||
               ReferenceEquals(comparer, StringComparer.OrdinalIgnoreCase);
    }

    public static ComparisonResult CompareUsingMostPreciseType(object? a, object? b, StringComparer stringComparer, CultureInfo cultureInfo)
    {
        var mpt = GetMostPreciseType(a?.GetType(), b?.GetType());

        if (mpt == typeof(double))
        {
            var left = Convert.ToDouble(a, cultureInfo);
            var right = Convert.ToDouble(b, cultureInfo);

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
            var left = Convert.ToSingle(a, cultureInfo);
            var right = Convert.ToSingle(b, cultureInfo);

            if (float.IsNaN(left) || float.IsNaN(right))
                return ComparisonResult.Unordered;

            return left.CompareTo(right) switch
            {
                < 0 => ComparisonResult.Less,
                > 0 => ComparisonResult.Greater,
                _ => ComparisonResult.Equal
            };
        }

        var aValue = a != null ? Convert.ChangeType(a, mpt, cultureInfo) : null;
        var bValue = b != null ? Convert.ChangeType(b, mpt, cultureInfo) : null;

        return stringComparer.Compare(aValue, bValue) switch
        {
            < 0 => ComparisonResult.Less,
            > 0 => ComparisonResult.Greater,
            _ => ComparisonResult.Equal
        };
    }
}
