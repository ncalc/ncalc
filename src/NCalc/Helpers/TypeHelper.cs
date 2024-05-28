using System.Collections.Frozen;

namespace NCalc.Helpers;

public static class TypeHelper
{
    private static readonly FrozenSet<Type> BuiltInTypes =
    new[]{
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
    }.ToFrozenSet();
    
    public static readonly FrozenDictionary<Type, FrozenSet<Type>> ImplicitPrimitiveConversionTable = new Dictionary<Type, FrozenSet<Type>> {
        { typeof(sbyte), new[] { typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) }.ToFrozenSet()},
        { typeof(byte), new[] { typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) }.ToFrozenSet()},
        { typeof(short), new[] { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) }.ToFrozenSet()},
        { typeof(ushort), new[] { typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) }.ToFrozenSet()},
        { typeof(int), new[] { typeof(long), typeof(float), typeof(double), typeof(decimal) }.ToFrozenSet()},
        { typeof(uint), new[] { typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) }.ToFrozenSet()},
        { typeof(long), new[] { typeof(float), typeof(double), typeof(decimal) }.ToFrozenSet()},
        { typeof(char), new[] { typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) }.ToFrozenSet()},
        { typeof(float), new[] { typeof(double) }.ToFrozenSet()},
        { typeof(ulong), new[] { typeof(float), typeof(double), typeof(decimal) }.ToFrozenSet()},
    }.ToFrozenDictionary();

    
    /// <summary>
    /// Gets the the most precise type.
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
    
    public static bool IsReal(object? value) => value is decimal or double or float;

    public record struct ComparasionOptions(CultureInfo CultureInfo, bool IsCaseSensitive);
    public static int CompareUsingMostPreciseType(object? a, object? b, ComparasionOptions options)
    {
        var (cultureInfo, isCaseInsensitiveComparer) = options;

        var mpt = GetMostPreciseType(a?.GetType(), b?.GetType());

        var aValue = a != null ? Convert.ChangeType(a, mpt, cultureInfo) : null;
        var bValue = b != null ? Convert.ChangeType(b, mpt, cultureInfo) : null;

        if (isCaseInsensitiveComparer)
            return CaseInsensitiveComparer.Default.Compare(aValue, bValue);

        return Comparer.Default.Compare(aValue, bValue);
    }
}