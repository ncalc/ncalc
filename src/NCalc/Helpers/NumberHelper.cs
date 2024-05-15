using System;
using System.Globalization;

namespace NCalc;

internal static class NumberHelper
{
    private static object? ConvertIfString(object? s, CultureInfo cultureInfo)
    {
        return s is string or char ? decimal.Parse(s.ToString()!, cultureInfo) : s;
    }

    public static object Add(object? a, object? b)
    {
        return Add(a, b, CultureInfo.CurrentCulture);
    }

    public static object Add(object? a, object? b, CultureInfo cultureInfo)
    {
        a = ConvertIfString(a, cultureInfo);
        b = ConvertIfString(b, cultureInfo);
        
        switch (a)
        {
            case bool:
                throw new InvalidOperationException(
                    $"Operator '+' can't be applied to operands of types 'bool' and {b?.GetType()}");
            case byte b1:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'byte' and 'bool'");
                    case byte b2: return b1 + b2;
                    case sbyte @sbyte: return b1 + @sbyte;
                    case short s: return b1 + s;
                    case ushort @ushort: return b1 + @ushort;
                    case int i: return b1 + i;
                    case uint u: return b1 + u;
                    case long l: return b1 + l;
                    case ulong @ulong: return b1 + @ulong;
                    case float f: return b1 + f;
                    case double d: return b1 + d;
                    case decimal @decimal: return b1 + @decimal;
                    default: throw new InvalidOperationException($"Operator '+' not implemented for 'byte' and {b?.GetType()}");
                }
            case sbyte @sbyte:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'sbyte' and 'bool'");
                    case byte b1: return @sbyte + b1;
                    case sbyte b1: return @sbyte + b1;
                    case short s: return @sbyte + s;
                    case ushort @ushort: return @sbyte + @ushort;
                    case int i: return @sbyte + i;
                    case uint u: return @sbyte + u;
                    case long l: return @sbyte + l;
                    case ulong: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'sbyte' and 'ulong'");
                    case float f: return @sbyte + f;
                    case double d: return @sbyte + d;
                    case decimal @decimal: return @sbyte + @decimal;
                    default: throw new InvalidOperationException($"Operator '+' not implemented for 'sbyte' and {b?.GetType()}");
                }
            case short s:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'short' and 'bool'");
                    case byte b1: return s + b1;
                    case sbyte @sbyte: return s + @sbyte;
                    case short s1: return s + s1;
                    case ushort @ushort: return s + @ushort;
                    case int i: return s + i;
                    case uint u: return s + u;
                    case long l: return s + l;
                    case ulong: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'short' and 'ulong'");
                    case float f: return s + f;
                    case double d: return s + d;
                    case decimal @decimal: return s + @decimal;
                    default: throw new InvalidOperationException($"Operator '+' not implemented for types 'short' and {b?.GetType()}");
                }
            case ushort @ushort:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ushort' and 'bool'");
                    case byte b1: return @ushort + b1;
                    case sbyte @sbyte: return @ushort + @sbyte;
                    case short s: return @ushort + s;
                    case ushort b1: return @ushort + b1;
                    case int i: return @ushort + i;
                    case uint u: return @ushort + u;
                    case long l: return @ushort + l;
                    case ulong @ulong: return @ushort + @ulong;
                    case float f: return @ushort + f;
                    case double d: return @ushort + d;
                    case decimal @decimal: return @ushort + @decimal;
                    default: throw new InvalidOperationException($"Operator '+' not implemented for types 'ushort' and {b?.GetType()}");
                }
            case int i:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'int' and 'bool'");
                    case byte b1: return i + b1;
                    case sbyte @sbyte: return i + @sbyte;
                    case short s: return i + s;
                    case ushort @ushort: return i + @ushort;
                    case int i1: return i + i1;
                    case uint u: return i + u;
                    case long l: return i + l;
                    case ulong: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'int' and 'ulong'");
                    case float f: return i + f;
                    case double d: return i + d;
                    case decimal @decimal: return i + @decimal;
                    default: throw new InvalidOperationException($"Operator '+' not implemented for types 'int' and {b?.GetType()}");
                }
            case uint u:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'uint' and 'bool'");
                    case byte b1: return u + b1;
                    case sbyte @sbyte: return u + @sbyte;
                    case short s: return u + s;
                    case ushort @ushort: return u + @ushort;
                    case int i: return u + i;
                    case uint u1: return u + u1;
                    case long l: return u + l;
                    case ulong @ulong: return u + @ulong;
                    case float f: return u + f;
                    case double d: return u + d;
                    case decimal @decimal: return u + @decimal;
                    default: throw new InvalidOperationException($"Operator '+' not implemented for types 'uint' and {b?.GetType()}");
                }
            case long l:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'long' and 'bool'");
                    case byte b1: return l + b1;
                    case sbyte @sbyte: return l + @sbyte;
                    case short s: return l + s;
                    case ushort @ushort: return l + @ushort;
                    case int i: return l + i;
                    case uint u: return l + u;
                    case long l1: return l + l1;
                    case ulong: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'long' and 'ulong'");
                    case float f: return l + f;
                    case double d: return l + d;
                    case decimal @decimal: return l + @decimal;
                    default: throw new InvalidOperationException($"Operator '+' not implemented for types 'long' and {b?.GetType()}");
                }
            case ulong @ulong:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'bool'");
                    case byte b1: return @ulong + b1;
                    case sbyte: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'sbyte'");
                    case short: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'short'");
                    case ushort @ushort: return @ulong + @ushort;
                    case int: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'int'");
                    case uint u: return @ulong + u;
                    case long: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'ulong'");
                    case ulong b1: return @ulong + b1;
                    case float f: return @ulong + f;
                    case double d: return @ulong + d;
                    case decimal @decimal: return @ulong + @decimal;
                    default: throw new InvalidOperationException($"Operator '+' not implemented for types 'ulong' and {b?.GetType()}");
                }
            case float f:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'float' and 'bool'");
                    case byte b1: return f + b1;
                    case sbyte @sbyte: return f + @sbyte;
                    case short s: return f + s;
                    case ushort @ushort: return f + @ushort;
                    case int i: return f + i;
                    case uint u: return f + u;
                    case long l: return f + l;
                    case ulong @ulong: return f + @ulong;
                    case float f1: return f + f1;
                    case double d: return f + d;
                    case decimal @decimal: return Convert.ToDecimal(a) + @decimal;
                    default: throw new InvalidOperationException($"Operator '+' not implemented for types 'float' and {b?.GetType()}");
                }
            case double d:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'double' and 'bool'");
                    case byte b1: return d + b1;
                    case sbyte @sbyte: return d + @sbyte;
                    case short s: return d + s;
                    case ushort @ushort: return d + @ushort;
                    case int i: return d + i;
                    case uint u: return d + u;
                    case long l: return d + l;
                    case ulong @ulong: return d + @ulong;
                    case float f: return d + f;
                    case double d1: return d + d1;
                    case decimal @decimal: return Convert.ToDecimal(a) + @decimal;
                    default: throw new InvalidOperationException($"Operator '+' not implemented for types 'double' and {b?.GetType()}");
                }

            case decimal @decimal:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'decimal' and 'bool'");
                    case byte b1: return @decimal + b1;
                    case sbyte @sbyte: return @decimal + @sbyte;
                    case short s: return @decimal + s;
                    case ushort @ushort: return @decimal + @ushort;
                    case int i: return @decimal + i;
                    case uint u: return @decimal + u;
                    case long l: return @decimal + l;
                    case ulong @ulong: return @decimal + @ulong;
                    case float: return @decimal + Convert.ToDecimal(b);
                    case double: return @decimal + Convert.ToDecimal(b);
                    case decimal b1: return @decimal + b1;
                    default: throw new InvalidOperationException($"Operator '+' not implemented for types 'decimal' and {b?.GetType()}");
                }
            default: throw new InvalidOperationException($"Operator '+' not implemented for operands of types {a} and {b?.GetType()}");
        }
    }

    public static object Subtract(object? a, object? b)
    {
        return Subtract(a, b, CultureInfo.CurrentCulture);
    }

    public static object Subtract(object? a, object? b, CultureInfo cultureInfo)
    {
        a = ConvertIfString(a, cultureInfo);
        b = ConvertIfString(b, cultureInfo);

        switch (a)
        {
            case bool: 
                throw new InvalidOperationException($"Operator '-' can't be applied to operands of types 'bool' and {b?.GetType()}");
            case byte b1:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'byte' and 'bool'");
                    case byte b2: return b1 - b2;
                    case sbyte @sbyte: return b1 - @sbyte;
                    case short s: return b1 - s;
                    case ushort @ushort: return b1 - @ushort;
                    case int i: return b1 - i;
                    case uint u: return b1 - u;
                    case long l: return b1 - l;
                    case ulong @ulong: return b1 - @ulong;
                    case float f: return b1 - f;
                    case double d: return b1 - d;
                    case decimal @decimal: return b1 - @decimal;
                    default: throw new InvalidOperationException($"Operator '-' not implemented for operands of types 'byte' and {b?.GetType()}");
                }
            case sbyte @sbyte:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'sbyte' and 'bool'");
                    case byte b1: return @sbyte - b1;
                    case sbyte b1: return @sbyte - b1;
                    case short s: return @sbyte - s;
                    case ushort @ushort: return @sbyte - @ushort;
                    case int i: return @sbyte - i;
                    case uint u: return @sbyte - u;
                    case long l: return @sbyte - l;
                    case ulong: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'sbyte' and 'ulong'");
                    case float f: return @sbyte - f;
                    case double d: return @sbyte - d;
                    case decimal @decimal: return @sbyte - @decimal;
                    default: throw new InvalidOperationException($"Operator '-' not implemented for operands of types 'byte' and {b?.GetType()}");
                }
            case short s:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'short' and 'bool'");
                    case byte b1: return s - b1;
                    case sbyte @sbyte: return s - @sbyte;
                    case short s1: return s - s1;
                    case ushort @ushort: return s - @ushort;
                    case int i: return s - i;
                    case uint u: return s - u;
                    case long l: return s - l;
                    case ulong: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'short' and 'ulong'");
                    case float f: return s - f;
                    case double d: return s - d;
                    case decimal @decimal: return s - @decimal;
                    default: throw new InvalidOperationException($"Operator '-' not implemented for operands of types 'short' and {b?.GetType()}");
                }
            case ushort @ushort:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ushort' and 'bool'");
                    case byte b1: return @ushort - b1;
                    case sbyte @sbyte: return @ushort - @sbyte;
                    case short s: return @ushort - s;
                    case ushort b1: return @ushort - b1;
                    case int i: return @ushort - i;
                    case uint u: return @ushort - u;
                    case long l: return @ushort - l;
                    case ulong @ulong: return @ushort - @ulong;
                    case float f: return @ushort - f;
                    case double d: return @ushort - d;
                    case decimal @decimal: return @ushort - @decimal;
                    default: throw new InvalidOperationException($"Operator '-' not implemented for operands of types 'ushort' and {b?.GetType()}");
                }
            case int i:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'int' and 'bool'");
                    case byte b1: return i - b1;
                    case sbyte @sbyte: return i - @sbyte;
                    case short s: return i - s;
                    case ushort @ushort: return i - @ushort;
                    case int i1: return i - i1;
                    case uint u: return i - u;
                    case long l: return i - l;
                    case ulong: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'int' and 'ulong'");
                    case float f: return i - f;
                    case double d: return i - d;
                    case decimal @decimal: return i - @decimal;
                    default: throw new InvalidOperationException($"Operator '-' not implemented for operands of types 'int' and {b?.GetType()}");
                }
            case uint u:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'uint' and 'bool'");
                    case byte b1: return u - b1;
                    case sbyte @sbyte: return u - @sbyte;
                    case short s: return u - s;
                    case ushort @ushort: return u - @ushort;
                    case int i: return u - i;
                    case uint u1: return u - u1;
                    case long l: return u - l;
                    case ulong @ulong: return u - @ulong;
                    case float f: return u - f;
                    case double d: return u - d;
                    case decimal @decimal: return u - @decimal;
                    default: throw new InvalidOperationException($"Operator '-' not implemented for operands of types 'uint' and {b?.GetType()}");
                }
            case long l:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'long' and 'bool'");
                    case byte b1: return l - b1;
                    case sbyte @sbyte: return l - @sbyte;
                    case short s: return l - s;
                    case ushort @ushort: return l - @ushort;
                    case int i: return l - i;
                    case uint u: return l - u;
                    case long l1: return l - l1;
                    case ulong: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'long' and 'ulong'");
                    case float f: return l - f;
                    case double d: return l - d;
                    case decimal @decimal: return l - @decimal;
                    default: throw new InvalidOperationException($"Operator '-' not implemented for operands of types 'long' and {b?.GetType()}");
                }
            case ulong @ulong:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'bool'");
                    case byte b1: return @ulong - b1;
                    case sbyte: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'sbyte'");
                    case short: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'short'");
                    case ushort @ushort: return @ulong - @ushort;
                    case int: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'int'");
                    case uint u: return @ulong - u;
                    case long: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'long'");
                    case ulong b1: return @ulong - b1;
                    case float f: return @ulong - f;
                    case double d: return @ulong - d;
                    case decimal @decimal: return @ulong - @decimal;
                    default: throw new InvalidOperationException($"Operator '-' not implemented for operands of types 'ulong' and {b?.GetType()}");
                }

            case float f:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'float' and 'bool'");
                    case byte b1: return f - b1;
                    case sbyte @sbyte: return f - @sbyte;
                    case short s: return f - s;
                    case ushort @ushort: return f - @ushort;
                    case int i: return f - i;
                    case uint u: return f - u;
                    case long l: return f - l;
                    case ulong @ulong: return f - @ulong;
                    case float f1: return f - f1;
                    case double d: return f - d;
                    case decimal: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'float' and 'decimal'");
                    default: throw new InvalidOperationException($"Operator '-' not implemented for operands of types 'float' and {b?.GetType()}");
                }
            case double d:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'double' and 'bool'");
                    case byte b1: return d - b1;
                    case sbyte @sbyte: return d - @sbyte;
                    case short s: return d - s;
                    case ushort @ushort: return d - @ushort;
                    case int i: return d - i;
                    case uint u: return d - u;
                    case long l: return d - l;
                    case ulong @ulong: return d - @ulong;
                    case float f: return d - f;
                    case double d1: return d - d1;
                    case decimal @decimal: return Convert.ToDecimal(a) - @decimal;
                    default: throw new InvalidOperationException($"Operator '-' not implemented for operands of types 'double' and {b?.GetType()}");
                }
            case decimal @decimal:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'decimal' and 'bool'");
                    case byte b1: return @decimal - b1;
                    case sbyte @sbyte: return @decimal - @sbyte;
                    case short s: return @decimal - s;
                    case ushort @ushort: return @decimal - @ushort;
                    case int i: return @decimal - i;
                    case uint u: return @decimal - u;
                    case long l: return @decimal - l;
                    case ulong @ulong: return @decimal - @ulong;
                    case float: return @decimal - Convert.ToDecimal(b);
                    case double: return @decimal - Convert.ToDecimal(b);
                    case decimal b1: return @decimal - b1;
                    default: throw new InvalidOperationException($"Operator '-' not implemented for operands of types 'decimal' and {b?.GetType()}");
                }
            default: throw new InvalidOperationException($"Operator '-' not implemented for operands of types {a} and {b?.GetType()}");
        }
    }

    public static object Multiply(object? a, object? b)
    {
        return Multiply(a, b, CultureInfo.CurrentCulture);
    }

    public static object Multiply(object? a, object? b, CultureInfo cultureInfo)
    {
        a = ConvertIfString(a, cultureInfo);
        b = ConvertIfString(b, cultureInfo);



        switch (a)
        {
            case bool:
                throw new InvalidOperationException(
                    $"Operator '*' can't be applied to operands of types 'bool' and {b?.GetType()}");
            case byte b1:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'byte' and 'bool'");
                    case byte b2: return b1 * b2;
                    case sbyte @sbyte: return b1 * @sbyte;
                    case short s: return b1 * s;
                    case ushort @ushort: return b1 * @ushort;
                    case int i: return b1 * i;
                    case uint u: return b1 * u;
                    case long l: return b1 * l;
                    case ulong @ulong: return b1 * @ulong;
                    case float f: return b1 * f;
                    case double d: return b1 * d;
                    case decimal @decimal: return b1 * @decimal;
                    default: throw new InvalidOperationException($"Operator '*' not implemented for operands of types 'byte' and {b?.GetType()}");
                }
            case sbyte @sbyte:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'sbyte' and 'bool'");
                    case byte b1: return @sbyte * b1;
                    case sbyte b1: return @sbyte * b1;
                    case short s: return @sbyte * s;
                    case ushort @ushort: return @sbyte * @ushort;
                    case int i: return @sbyte * i;
                    case uint u: return @sbyte * u;
                    case long l: return @sbyte * l;
                    case ulong: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'sbyte' and 'ulong'");
                    case float f: return @sbyte * f;
                    case double d: return @sbyte * d;
                    case decimal @decimal: return @sbyte * @decimal;
                    default: throw new InvalidOperationException($"Operator '*' not implemented for operands of types 'sbyte' and {b?.GetType()}");
                }
            case short s:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'short' and 'bool'");
                    case byte b1: return s * b1;
                    case sbyte @sbyte: return s * @sbyte;
                    case short s1: return s * s1;
                    case ushort @ushort: return s * @ushort;
                    case int i: return s * i;
                    case uint u: return s * u;
                    case long l: return s * l;
                    case ulong: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'short' and 'ulong'");
                    case float f: return s * f;
                    case double d: return s * d;
                    case decimal @decimal: return s * @decimal;
                    default: throw new InvalidOperationException($"Operator '*' not implemented for operands of types 'short' and {b?.GetType()}");
                }
            case ushort @ushort:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ushort' and 'bool'");
                    case byte b1: return @ushort * b1;
                    case sbyte @sbyte: return @ushort * @sbyte;
                    case short s: return @ushort * s;
                    case ushort b1: return @ushort * b1;
                    case int i: return @ushort * i;
                    case uint u: return @ushort * u;
                    case long l: return @ushort * l;
                    case ulong @ulong: return @ushort * @ulong;
                    case float f: return @ushort * f;
                    case double d: return @ushort * d;
                    case decimal @decimal: return @ushort * @decimal;
                    default: throw new InvalidOperationException($"Operator '*' not implemented for operands of types 'ushort' and {b?.GetType()}");
                }
            case int i:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'int' and 'bool'");
                    case byte b1: return i * b1;
                    case sbyte @sbyte: return i * @sbyte;
                    case short s: return i * s;
                    case ushort @ushort: return i * @ushort;
                    case int i1: return i * i1;
                    case uint u: return i * u;
                    case long l: return i * l;
                    case ulong: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'int' and 'ulong'");
                    case float f: return i * f;
                    case double d: return i * d;
                    case decimal @decimal: return i * @decimal;
                    default: throw new InvalidOperationException($"Operator '*' not implemented for operands of types 'int' and {b?.GetType()}");
                }
            case uint u:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'uint' and 'bool'");
                    case byte b1: return u * b1;
                    case sbyte @sbyte: return u * @sbyte;
                    case short s: return u * s;
                    case ushort @ushort: return u * @ushort;
                    case int i: return u * i;
                    case uint u1: return u * u1;
                    case long l: return u * l;
                    case ulong @ulong: return u * @ulong;
                    case float f: return u * f;
                    case double d: return u * d;
                    case decimal @decimal: return u * @decimal;
                    default: throw new InvalidOperationException($"Operator '*' not implemented for operands of types 'int' and {b?.GetType()}");
                }
            case long l:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'long' and 'bool'");
                    case byte b1: return l * b1;
                    case sbyte @sbyte: return l * @sbyte;
                    case short s: return l * s;
                    case ushort @ushort: return l * @ushort;
                    case int i: return l * i;
                    case uint u: return l * u;
                    case long l1: return l * l1;
                    case ulong: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'long' and 'ulong'");
                    case float f: return l * f;
                    case double d: return l * d;
                    case decimal @decimal: return l * @decimal;
                    default: throw new InvalidOperationException($"Operator '*' not implemented for operands of types 'int' and {b?.GetType()}");
                }
            case ulong @ulong:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'bool'");
                    case byte b1: return @ulong * b1;
                    case sbyte: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'sbyte'");
                    case short: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'short'");
                    case ushort @ushort: return @ulong * @ushort;
                    case int: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'int'");
                    case uint u: return @ulong * u;
                    case long: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'long'");
                    case ulong b1: return @ulong * b1;
                    case float f: return @ulong * f;
                    case double d: return @ulong * d;
                    case decimal @decimal: return @ulong * @decimal;
                    default: throw new InvalidOperationException($"Operator '*' not implemented for operands of types 'ulong' and {b?.GetType()}");
                }

            case float f:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'float' and 'bool'");
                    case byte b1: return f * b1;
                    case sbyte @sbyte: return f * @sbyte;
                    case short s: return f * s;
                    case ushort @ushort: return f * @ushort;
                    case int i: return f * i;
                    case uint u: return f * u;
                    case long l: return f * l;
                    case ulong @ulong: return f * @ulong;
                    case float f1: return f * f1;
                    case double d: return f * d;
                    case decimal @decimal: return Convert.ToDecimal(a) * @decimal;
                    default: throw new InvalidOperationException($"Operator '*' not implemented for operands of types 'float' and {b?.GetType()}");
                }
 
            case double d:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'double' and 'bool'");
                    case byte b1: return d * b1;
                    case sbyte @sbyte: return d * @sbyte;
                    case short s: return d * s;
                    case ushort @ushort: return d * @ushort;
                    case int i: return d * i;
                    case uint u: return d * u;
                    case long l: return d * l;
                    case ulong @ulong: return d * @ulong;
                    case float f: return d * f;
                    case double d1: return d * d1;
                    case decimal @decimal: return Convert.ToDecimal(a) * @decimal;
                    default: throw new InvalidOperationException($"Operator '*' not implemented for operands of types 'double' and {b?.GetType()}");
                }
            case decimal @decimal:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'decimal' and 'bool'");
                    case byte b1: return @decimal * b1;
                    case sbyte @sbyte: return @decimal * @sbyte;
                    case short s: return @decimal * s;
                    case ushort @ushort: return @decimal * @ushort;
                    case int i: return @decimal * i;
                    case uint u: return @decimal * u;
                    case long l: return @decimal * l;
                    case ulong @ulong: return @decimal * @ulong;
                    case float: return @decimal * Convert.ToDecimal(b);
                    case double: return @decimal * Convert.ToDecimal(b);
                    case decimal b1: return @decimal * b1;
                    default: throw new InvalidOperationException($"Operator '*' not implemented for operands of types 'decimal' and {b?.GetType()}");
                }
            default: throw new InvalidOperationException($"Operator '*' not implemented for operands of types {a} and {b?.GetType()}");
        }
    }
        
    public static object Divide(object? a, object? b)
    {
        return Divide(a, b, CultureInfo.CurrentCulture);
    }

    public static object Divide(object? a, object? b, CultureInfo cultureInfo)
    {
        a = ConvertIfString(a, cultureInfo);
        b = ConvertIfString(b, cultureInfo);

        switch (a)
        {
            case bool:
                throw new InvalidOperationException(
                    $"Operator '/' can't be applied to operands of types 'bool' and {b?.GetType()}");
            case byte b1:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'byte' and 'bool'");
                    case byte b2: return b1 / b2;
                    case sbyte @sbyte: return b1 / @sbyte;
                    case short s: return b1 / s;
                    case ushort @ushort: return b1 / @ushort;
                    case int i: return b1 / i;
                    case uint u: return b1 / u;
                    case long l: return b1 / l;
                    case ulong @ulong: return b1 / @ulong;
                    case float f: return b1 / f;
                    case double d: return b1 / d;
                    case decimal @decimal: return b1 / @decimal;
                    default: throw new InvalidOperationException($"Operator '/' not implemented for operands of types 'decimal' and {b?.GetType()}");
                }
            case sbyte @sbyte:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'sbyte' and 'bool'");
                    case byte b1: return @sbyte / b1;
                    case sbyte b1: return @sbyte / b1;
                    case short s: return @sbyte / s;
                    case ushort @ushort: return @sbyte / @ushort;
                    case int i: return @sbyte / i;
                    case uint u: return @sbyte / u;
                    case long l: return @sbyte / l;
                    case ulong: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'sbyte' and 'ulong'");
                    case float f: return @sbyte / f;
                    case double d: return @sbyte / d;
                    case decimal @decimal: return @sbyte / @decimal;
                    default: throw new InvalidOperationException($"Operator '/' not implemented for operands of types 'decimal' and {b?.GetType()}");
                }

            case short s:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'short' and 'bool'");
                    case byte b1: return s / b1;
                    case sbyte @sbyte: return s / @sbyte;
                    case short s1: return s / s1;
                    case ushort @ushort: return s / @ushort;
                    case int i: return s / i;
                    case uint u: return s / u;
                    case long l: return s / l;
                    case ulong: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'short' and 'ulong'");
                    case float f: return s / f;
                    case double d: return s / d;
                    case decimal @decimal: return s / @decimal;
                    default: throw new InvalidOperationException($"Operator '/' not implemented for operands of types 'decimal' and {b?.GetType()}");
                }
            case ushort @ushort:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'ushort' and 'bool'");
                    case byte b1: return @ushort / b1;
                    case sbyte @sbyte: return @ushort / @sbyte;
                    case short s: return @ushort / s;
                    case ushort b1: return @ushort / b1;
                    case int i: return @ushort / i;
                    case uint u: return @ushort / u;
                    case long l: return @ushort / l;
                    case ulong @ulong: return @ushort / @ulong;
                    case float f: return @ushort / f;
                    case double d: return @ushort / d;
                    case decimal @decimal: return @ushort / @decimal;
                    default: throw new InvalidOperationException($"Operator '/' not implemented for operands of types 'ushort' and {b?.GetType()}");
                }
            case int i:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'int' and 'bool'");
                    case byte b1: return i / b1;
                    case sbyte @sbyte: return i / @sbyte;
                    case short s: return i / s;
                    case ushort @ushort: return i / @ushort;
                    case int i1: return i / i1;
                    case uint u: return i / u;
                    case long l: return i / l;
                    case ulong: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'int' and 'ulong'");
                    case float f: return i / f;
                    case double d: return i / d;
                    case decimal @decimal: return i / @decimal;
                    default: throw new InvalidOperationException($"Operator '/' not implemented for operands of types 'int' and {b?.GetType()}");
                }
            case uint u:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'uint' and 'bool'");
                    case byte b1: return u / b1;
                    case sbyte @sbyte: return u / @sbyte;
                    case short s: return u / s;
                    case ushort @ushort: return u / @ushort;
                    case int i: return u / i;
                    case uint u1: return u / u1;
                    case long l: return u / l;
                    case ulong @ulong: return u / @ulong;
                    case float f: return u / f;
                    case double d: return u / d;
                    case decimal @decimal: return u / @decimal;
                    default: throw new InvalidOperationException($"Operator '/' not implemented for operands of types 'uint' and {b?.GetType()}");
                }
            case long l:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'long' and 'bool'");
                    case byte b1: return l / b1;
                    case sbyte @sbyte: return l / @sbyte;
                    case short s: return l / s;
                    case ushort @ushort: return l / @ushort;
                    case int i: return l / i;
                    case uint u: return l / u;
                    case long l1: return l / l1;
                    case ulong: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'long' and 'ulong'");
                    case float f: return l / f;
                    case double d: return l / d;
                    case decimal @decimal: return l / @decimal;
                    default: throw new InvalidOperationException($"Operator '/' not implemented for operands of types 'long' and {b?.GetType()}");
                }
            case ulong @ulong:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'bool'");
                    case byte: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'ulong' and 'byte'");
                    case sbyte: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'ulong' and 'sbyte'");
                    case short: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'ulong' and 'short'");
                    case ushort @ushort: return @ulong / @ushort;
                    case int: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'ulong' and 'int'");
                    case uint u: return @ulong / u;
                    case long: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'ulong' and 'long'");
                    case ulong b1: return @ulong / b1;
                    case float f: return @ulong / f;
                    case double d: return @ulong / d;
                    case decimal @decimal: return @ulong / @decimal;
                    default: throw new InvalidOperationException($"Operator '/' not implemented for operands of types 'ulong' and {b?.GetType()}");
                }
            case float f:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'float' and 'bool'");
                    case byte b1: return f / b1;
                    case sbyte @sbyte: return f / @sbyte;
                    case short s: return f / s;
                    case ushort @ushort: return f / @ushort;
                    case int i: return f / i;
                    case uint u: return f / u;
                    case long l: return f / l;
                    case ulong @ulong: return f / @ulong;
                    case float f1: return f / f1;
                    case double d: return f / d;
                    case decimal: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'float' and 'decimal'");
                    default: throw new InvalidOperationException($"Operator '/' not implemented for operands of types 'ulong' and {b?.GetType()}");
                }
            case double d:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'double' and 'bool'");
                    case byte b1: return d / b1;
                    case sbyte @sbyte: return d / @sbyte;
                    case short s: return d / s;
                    case ushort @ushort: return d / @ushort;
                    case int i: return d / i;
                    case uint u: return d / u;
                    case long l: return d / l;
                    case ulong @ulong: return d / @ulong;
                    case float f: return d / f;
                    case double d1: return d / d1;
                    case decimal @decimal: return Convert.ToDecimal(a) / @decimal;
                    default: throw new InvalidOperationException($"Operator '/' not implemented for operands of types 'double' and {b?.GetType()}");
                }
            case decimal @decimal:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'decimal' and 'bool'");
                    case byte: return @decimal / (sbyte)b;
                    case sbyte @sbyte: return @decimal / @sbyte;
                    case short s: return @decimal / s;
                    case ushort @ushort: return @decimal / @ushort;
                    case int i: return @decimal / i;
                    case uint u: return @decimal / u;
                    case long l: return @decimal / l;
                    case ulong @ulong: return @decimal / @ulong;
                    case float: return @decimal / Convert.ToDecimal(b);
                    case double: return @decimal / Convert.ToDecimal(b);
                    case decimal b1: return @decimal / b1;
                    default: throw new InvalidOperationException($"Operator '/' not implemented for operands of types 'decimal' and {b?.GetType()}");
                }
            default: throw new InvalidOperationException($"Operator '/' not implemented for operands of types {a} and {b?.GetType()}");
        }
    }

    public static object Modulo(object? a, object? b)
    {
        return Modulo(a, b, CultureInfo.CurrentCulture);
    }

    public static object Modulo(object? a, object? b, CultureInfo cultureInfo)
    {
        a = ConvertIfString(a, cultureInfo);
        b = ConvertIfString(b, cultureInfo);



        switch (a)
        {
            case bool:
                throw new InvalidOperationException(
                    $"Operator '%' can't be applied to operands of types 'bool' and {b?.GetType()}");
            case byte b1:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'byte' and 'bool'");
                    case byte b2: return b1 % b2;
                    case sbyte @sbyte: return b1 % @sbyte;
                    case short s: return b1 % s;
                    case ushort @ushort: return b1 % @ushort;
                    case int i: return b1 % i;
                    case uint u: return b1 % u;
                    case long l: return b1 % l;
                    case ulong @ulong: return b1 % @ulong;
                    case float f: return b1 % f;
                    case double d: return b1 % d;
                    case decimal @decimal: return b1 % @decimal;
                    default: throw new InvalidOperationException($"Operator '/' not implemented for operands of types 'byte' and {b?.GetType()}");
                }
            case sbyte @sbyte:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'sbyte' and 'bool'");
                    case byte b1: return @sbyte % b1;
                    case sbyte b1: return @sbyte % b1;
                    case short s: return @sbyte % s;
                    case ushort @ushort: return @sbyte % @ushort;
                    case int i: return @sbyte % i;
                    case uint u: return @sbyte % u;
                    case long l: return @sbyte % l;
                    case ulong: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'sbyte' and 'ulong'");
                    case float f: return @sbyte % f;
                    case double d: return @sbyte % d;
                    case decimal @decimal: return @sbyte % @decimal;
                    default: throw new InvalidOperationException($"Operator '/' not implemented for operands of types 'sbyte' and {b?.GetType()}");
                }
            case short s:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'short' and 'bool'");
                    case byte b1: return s % b1;
                    case sbyte @sbyte: return s % @sbyte;
                    case short s1: return s % s1;
                    case ushort @ushort: return s % @ushort;
                    case int i: return s % i;
                    case uint u: return s % u;
                    case long l: return s % l;
                    case ulong: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'short' and 'ulong'");
                    case float f: return s % f;
                    case double d: return s % d;
                    case decimal @decimal: return s % @decimal;
                    default: throw new InvalidOperationException($"Operator '%' not implemented for operands of types 'short' and {b?.GetType()}");
                }
            case ushort @ushort:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ushort' and 'bool'");
                    case byte b1: return @ushort % b1;
                    case sbyte @sbyte: return @ushort % @sbyte;
                    case short s: return @ushort % s;
                    case ushort b1: return @ushort % b1;
                    case int i: return @ushort % i;
                    case uint u: return @ushort % u;
                    case long l: return @ushort % l;
                    case ulong @ulong: return @ushort % @ulong;
                    case float f: return @ushort % f;
                    case double d: return @ushort % d;
                    case decimal @decimal: return @ushort % @decimal;
                    default: throw new InvalidOperationException($"Operator '%' not implemented for operands of types 'ushort' and {b?.GetType()}");
                }
            case int i:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'int' and 'bool'");
                    case byte b1: return i % b1;
                    case sbyte @sbyte: return i % @sbyte;
                    case short s: return i % s;
                    case ushort @ushort: return i % @ushort;
                    case int i1: return i % i1;
                    case uint u: return i % u;
                    case long l: return i % l;
                    case ulong: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'int' and 'ulong'");
                    case float f: return i % f;
                    case double d: return i % d;
                    case decimal @decimal: return i % @decimal;
                    default: throw new InvalidOperationException($"Operator '%' not implemented for operands of types 'int' and {b?.GetType()}");
                }
            case uint u:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'uint' and 'bool'");
                    case byte b1: return u % b1;
                    case sbyte @sbyte: return u % @sbyte;
                    case short s: return u % s;
                    case ushort @ushort: return u % @ushort;
                    case int i: return u % i;
                    case uint u1: return u % u1;
                    case long l: return u % l;
                    case ulong @ulong: return u % @ulong;
                    case float f: return u % f;
                    case double d: return u % d;
                    case decimal @decimal: return u % @decimal;
                    default: throw new InvalidOperationException($"Operator '%' not implemented for operands of types 'uint' and {b?.GetType()}");
                }
            case long l:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'long' and 'bool'");
                    case byte b1: return l % b1;
                    case sbyte @sbyte: return l % @sbyte;
                    case short s: return l % s;
                    case ushort @ushort: return l % @ushort;
                    case int i: return l % i;
                    case uint u: return l % u;
                    case long l1: return l % l1;
                    case ulong: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'long' and 'ulong'");
                    case float f: return l % f;
                    case double d: return l % d;
                    case decimal @decimal: return l % @decimal;
                    default: throw new InvalidOperationException($"Operator '%' not implemented for operands of types 'long' and {b?.GetType()}");
                }
            case ulong @ulong:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ulong' and 'bool'");
                    case byte b1: return @ulong % b1;
                    case sbyte: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ulong' and 'sbyte'");
                    case short: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ulong' and 'short'");
                    case ushort @ushort: return @ulong % @ushort;
                    case int: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ulong' and 'int'");
                    case uint u: return @ulong % u;
                    case long: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ulong' and 'long'");
                    case ulong b1: return @ulong % b1;
                    case float f: return @ulong % f;
                    case double d: return @ulong % d;
                    case decimal @decimal: return @ulong % @decimal;
                    default: throw new InvalidOperationException($"Operator '%' not implemented for operands of types 'ulong' and {b?.GetType()}");
                }
            case float f:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'float' and 'bool'");
                    case byte b1: return f % b1;
                    case sbyte @sbyte: return f % @sbyte;
                    case short s: return f % s;
                    case ushort @ushort: return f % @ushort;
                    case int i: return f % i;
                    case uint u: return f % u;
                    case long l: return f % l;
                    case ulong @ulong: return f % @ulong;
                    case float f1: return f % f1;
                    case double d: return f % d;
                    case decimal: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'float' and 'decimal'");
                    default: throw new InvalidOperationException($"Operator '%' not implemented for operands of types 'long' and {b?.GetType()}");
                }
            case double d:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'double' and 'bool'");
                    case byte b1: return d % b1;
                    case sbyte @sbyte: return d % @sbyte;
                    case short s: return d % s;
                    case ushort @ushort: return d % @ushort;
                    case int i: return d % i;
                    case uint u: return d % u;
                    case long l: return d % l;
                    case ulong @ulong: return d % @ulong;
                    case float f: return d % f;
                    case double d1: return d % d1;
                    case decimal: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'double' and 'decimal'");
                    default: throw new InvalidOperationException($"Operator '%' not implemented for operands of types 'double' and {b?.GetType()}");
                }
            case decimal @decimal:
                switch (b)
                {
                    case bool: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'decimal' and 'bool'");
                    case byte b1: return @decimal % b1;
                    case sbyte @sbyte: return @decimal % @sbyte;
                    case short s: return @decimal % s;
                    case ushort @ushort: return @decimal % @ushort;
                    case int i: return @decimal % i;
                    case uint u: return @decimal % u;
                    case long l: return @decimal % l;
                    case ulong @ulong: return @decimal % @ulong;
                    case float: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'decimal' and 'float'");
                    case double: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'decimal' and 'decimal'");
                    case decimal b1: return @decimal % b1;
                    default: throw new InvalidOperationException($"Operator '%' not implemented for operands of types 'decimal' and {b?.GetType()}");
                }
            default: throw new InvalidOperationException($"Operator '+' not implemented for operands of types {a} and {b?.GetType()}");
        }
    }

    public static object? Max(object? a, object? b)
    {
        return Max(a, b, CultureInfo.CurrentCulture);
    }

    public static object? Max(object? a, object? b, CultureInfo cultureInfo)
    {
        a = ConvertIfString(a, cultureInfo);
        b = ConvertIfString(b, cultureInfo);

        if (a == null && b == null)
        {
            return null;
        }

        if (a == null)
        {
            return b;
        }

        if (b == null)
        {
            return a;
        }
        
        switch (a)
        {
            case byte b1:
                return Math.Max(b1, Convert.ToByte(b));
            case sbyte @sbyte:
                return Math.Max(@sbyte, Convert.ToSByte(b));
            case short s:
                return Math.Max(s, Convert.ToInt16(b));
            case ushort @ushort:
                return Math.Max(@ushort, Convert.ToUInt16(b));
            case int i:
                return Math.Max(i, Convert.ToInt32(b));
            case uint u:
                return Math.Max(u, Convert.ToUInt32(b));
            case long l:
                return Math.Max(l, Convert.ToInt64(b));
            case ulong @ulong:
                return Math.Max(@ulong, Convert.ToUInt64(b));
            case float f:
                return Math.Max(f, Convert.ToSingle(b));
            case double d:
                return Math.Max(d, Convert.ToDouble(b));
            case decimal @decimal:
                return Math.Max(@decimal, Convert.ToDecimal(b));
            default: throw new InvalidOperationException($"Max not implemented for parameters of {a} and {b?.GetType()}");
        }
    }

    public static object? Min(object? a, object? b)
    {
        return Min(a, b, CultureInfo.CurrentCulture);
    }

    public static object? Min(object? a, object? b, CultureInfo cultureInfo)
    {
        a = ConvertIfString(a, cultureInfo);
        b = ConvertIfString(b, cultureInfo);

        if (a == null && b == null)
        {
            return null;
        }

        if (a == null)
        {
            return b;
        }

        if (b == null)
        {
            return a;
        }

        switch (a)
        {
            case byte b1:
                return Math.Min(b1, Convert.ToByte(b));
            case sbyte @sbyte:
                return Math.Min(@sbyte, Convert.ToSByte(b));
            case short s:
                return Math.Min(s, Convert.ToInt16(b));
            case ushort @ushort:
                return Math.Min(@ushort, Convert.ToUInt16(b));
            case int i:
                return Math.Min(i, Convert.ToInt32(b));
            case uint u:
                return Math.Min(u, Convert.ToUInt32(b));
            case long l:
                return Math.Min(l, Convert.ToInt64(b));
            case ulong @ulong:
                return Math.Min(@ulong, Convert.ToUInt64(b));
            case float f:
                return Math.Min(f, Convert.ToSingle(b));
            case double d:
                return Math.Min(d, Convert.ToDouble(b));
            case decimal @decimal:
                return Math.Min(@decimal, Convert.ToDecimal(b));
            default: throw new InvalidOperationException($"Max not implemented for parameters of {a} and {b?.GetType()}");

        }
    }
}