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
}
