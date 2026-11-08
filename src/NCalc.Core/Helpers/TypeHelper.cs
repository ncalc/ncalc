using System.Numerics;

namespace NCalc.Helpers;

public static partial class TypeHelper
{
    internal static bool IsIntegerType(Type type)
    {
        return type == typeof(BigInteger) || type == typeof(byte) || type == typeof(sbyte) ||
               type == typeof(short) || type == typeof(ushort) || type == typeof(int) ||
               type == typeof(uint) || type == typeof(long) || type == typeof(ulong) || type == typeof(char);
    }

    internal static BigInteger ToBigInteger(object value, CultureInfo cultureInfo)
    {
        return value switch
        {
            BigInteger integer => integer,
            byte integer => integer,
            sbyte integer => integer,
            short integer => integer,
            ushort integer => integer,
            int integer => integer,
            uint integer => integer,
            long integer => integer,
            ulong integer => integer,
            char character => character,
            string text => BigInteger.Parse(text, NumberStyles.Integer, cultureInfo),
            _ => throw new InvalidOperationException(
                "BigInteger operands require integral values. Convert real-valued operands explicitly before evaluation.")
        };
    }

    private static ComparisonResult CompareBigIntegers(object? a, object? b, CultureInfo cultureInfo)
    {
        if (a is null)
            return b is null ? ComparisonResult.Equal : ComparisonResult.Less;
        if (b is null)
            return ComparisonResult.Greater;

        return ToBigInteger(a, cultureInfo).CompareTo(ToBigInteger(b, cultureInfo)) switch
        {
            < 0 => ComparisonResult.Less,
            > 0 => ComparisonResult.Greater,
            _ => ComparisonResult.Equal
        };
    }

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
