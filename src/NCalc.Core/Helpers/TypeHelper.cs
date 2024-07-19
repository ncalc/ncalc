using System.Collections.Frozen;
using System.Runtime.CompilerServices;

namespace NCalc.Helpers;

public static class TypeHelper
{
    private static readonly Type[] BuiltInTypes =
    [
        typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(long),
        typeof(ulong),
        typeof(int),
        typeof(uint),
        typeof(short),
        typeof(ushort),
        typeof(byte),
        typeof(sbyte),
        typeof(char),
        typeof(bool),
        typeof(string),
        typeof(object)
    ];

    private static readonly Type[] NumbersPrecedence =
    [
        typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(ulong),
        typeof(long),
        typeof(uint),
        typeof(int),
        typeof(ushort),
        typeof(short),
        typeof(byte),
        typeof(sbyte)
    ];

    public static readonly FrozenDictionary<Type, Type[]> ImplicitPrimitiveConversionTable =
        new Dictionary<Type, Type[]>()
        {
            {
                typeof(sbyte),
                [typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal)]
            },
            {
                typeof(byte),
                [
                    typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
                    typeof(float),
                    typeof(double), typeof(decimal)
                ]
            },
            { typeof(short), [typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal)] },
            {
                typeof(ushort),
                [typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)]
            },
            { typeof(int), [typeof(long), typeof(float), typeof(double), typeof(decimal)] },
            { typeof(uint), [typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)] },
            { typeof(long), [typeof(float), typeof(double), typeof(decimal)] },
            {
                typeof(char),
                [
                    typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float),
                    typeof(double),
                    typeof(decimal)
                ]
            },
            { typeof(float), [typeof(double)] },
            { typeof(ulong), [typeof(float), typeof(double), typeof(decimal)] }
        }.ToFrozenDictionary();

    /// <summary>
    /// Gets the most precise type.
    /// </summary>
    /// <param name="a">Type a.</param>
    /// <param name="b">Type b.</param>
    /// <returns></returns>
    private static Type GetMostPreciseType(Type? a, Type? b)
    {
        foreach (var t in BuiltInTypes)
        {
            if (a == t || b == t)
            {
                return t;
            }
        }

        return a ?? typeof(object);
    }

    /// <summary>
    /// Gets the the most precise number type.
    /// </summary>
    /// <param name="a">Type a.</param>
    /// <param name="b">Type b.</param>
    /// <returns></returns>
    public static Type? GetMostPreciseNumberType(Type a, Type b)
    {
        var l = Array.IndexOf(NumbersPrecedence, a);
        var r = Array.IndexOf(NumbersPrecedence, b);

        if (l >= 0 && r >= 0)
            return NumbersPrecedence[Math.Min(l, r)];

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReal(object? value) => value is decimal or double or float;

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