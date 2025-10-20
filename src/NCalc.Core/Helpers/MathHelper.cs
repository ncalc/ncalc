using System.Numerics;
using ExtendedNumerics;

namespace NCalc.Helpers;

/// <summary>
/// Utilities for doing mathematical operations between different object types.
/// </summary>
public static class MathHelper
{
    // unchecked
    private static readonly Func<dynamic, dynamic, object> AddFunc = (a, b) => a + b;
    private static readonly Func<dynamic, dynamic, object> SubtractFunc = (a, b) => a - b;
    private static readonly Func<dynamic, dynamic, object> MultiplyFunc = (a, b) => a * b;
    private static readonly Func<dynamic, dynamic, object> DivideFunc = (a, b) => a / b;
    private static readonly Func<dynamic, dynamic, object> ModuloFunc = (a, b) => a % b;

    // checked
    private static readonly Func<dynamic, dynamic, object> AddFuncChecked = (a, b) =>
    {
        var res = checked(a + b);
        CheckOverflow(res);

        return res;
    };

    private static readonly Func<dynamic, dynamic, object> SubtractFuncChecked = (a, b) =>
    {
        var res = checked(a - b);
        CheckOverflow(res);

        return res;
    };

    private static readonly Func<dynamic, dynamic, object> MultiplyFuncChecked = (a, b) =>
    {
        var res = checked(a * b);
        CheckOverflow(res);

        return res;
    };

    private static readonly Func<dynamic, dynamic, object> DivideFuncChecked = (a, b) =>
    {
        var res = checked(a / b);
        CheckOverflow(res);

        return res;
    };

    public static object? Add(object? a, object? b)
    {
        return Add(a, b, CultureInfo.CurrentCulture);
    }

    public static object? Add(object? a, object? b, MathHelperOptions options)
    {
        if (a == null || b == null)
            return null;

        a = ConvertIfNeeded(a, options);
        b = ConvertIfNeeded(b, options);

        var func = options.OverflowProtection ? AddFuncChecked : AddFunc;
        return ExecuteOperation(a, b, '+', options.CultureInfo, func);
    }

    public static object? Subtract(object? a, object? b)
    {
        return Subtract(a, b, CultureInfo.CurrentCulture);
    }

    public static object? Subtract(object? a, object? b, MathHelperOptions options)
    {
        if (a == null || b == null)
            return null;

        a = ConvertIfNeeded(a, options);
        b = ConvertIfNeeded(b, options);

        var func = options.OverflowProtection ? SubtractFuncChecked : SubtractFunc;
        return ExecuteOperation(a, b, '-', options.CultureInfo, func);
    }

    public static object? Multiply(object? a, object? b)
    {
        return Multiply(a, b, CultureInfo.CurrentCulture);
    }

    public static object? Multiply(object? a, object? b, MathHelperOptions options)
    {
        if (a == null || b == null)
            return null;

        a = ConvertIfNeeded(a, options);
        b = ConvertIfNeeded(b, options);

        var func = options.OverflowProtection ? MultiplyFuncChecked : MultiplyFunc;
        return ExecuteOperation(a, b, '*', options.CultureInfo, func);
    }

    public static object? Divide(object? a, object? b)
    {
        return Divide(a, b, CultureInfo.CurrentCulture);
    }

    public static object? Divide(object? a, object? b, MathHelperOptions options)
    {
        if (a == null || b == null)
            return null;

        a = ConvertIfNeeded(a, options);
        b = ConvertIfNeeded(b, options);

        if (!(TypeHelper.IsReal(a) || TypeHelper.IsReal(b)))
            a = Convert.ToDouble(a, options.CultureInfo);

        var func = options.OverflowProtection ? DivideFuncChecked : DivideFunc;
        return ExecuteOperation(a, b, '/', options.CultureInfo, func);
    }

    public static object? Modulo(object? a, object? b)
    {
        return Modulo(a, b, CultureInfo.CurrentCulture);
    }

    public static object? Modulo(object? a, object? b, MathHelperOptions options)
    {
        if (a == null || b == null)
            return null;

        a = ConvertIfNeeded(a, options);
        b = ConvertIfNeeded(b, options);

        return ExecuteOperation(a, b, '%', options.CultureInfo, ModuloFunc);
    }

    public static object? Max(object a, object b)
    {
        return Max(a, b, CultureInfo.CurrentCulture);
    }

    public static object? Max(object? a, object? b, MathHelperOptions options)
    {
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

        a = ConvertIfNeeded(a, options);
        b = ConvertIfNeeded(b, options);

        var typeCode = ConvertToHighestPrecision(ref a, ref b, options.CultureInfo);

        return typeCode switch
        {
            TypeCode.Byte => Math.Max((byte)a, (byte)b),
            TypeCode.SByte => Math.Max((sbyte)a, (sbyte)b),
            TypeCode.Int16 => Math.Max((short)a, (short)b),
            TypeCode.UInt16 => Math.Max((ushort)a, (ushort)b),
            TypeCode.Int32 => Math.Max((int)a, (int)b),
            TypeCode.UInt32 => Math.Max((uint)a, (uint)b),
            TypeCode.Int64 => Math.Max((long)a, (long)b),
            TypeCode.UInt64 => Math.Max((ulong)a, (ulong)b),
            TypeCode.Single => Math.Max((float)a, (float)b),
            TypeCode.Double => Math.Max((double)a, (double)b),
            TypeCode.Decimal => Math.Max((decimal)a, (decimal)b),
            _ => null,
        };
    }

    public static object? Min(object? a, object? b)
    {
        return Min(a, b, CultureInfo.CurrentCulture);
    }

    public static object? Min(object? a, object? b, MathHelperOptions options)
    {
        var cultureInfo = options.CultureInfo;

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

        a = ConvertIfNeeded(a, options);
        b = ConvertIfNeeded(b, options);

        var typeCode = ConvertToHighestPrecision(ref a, ref b, cultureInfo);

        return typeCode switch
        {
            TypeCode.Byte => Math.Min((byte)a, (byte)b),
            TypeCode.SByte => Math.Min((sbyte)a, (sbyte)b),
            TypeCode.Int16 => Math.Min((short)a, (short)b),
            TypeCode.UInt16 => Math.Min((ushort)a, (ushort)b),
            TypeCode.Int32 => Math.Min((int)a, (int)b),
            TypeCode.UInt32 => Math.Min((uint)a, (uint)b),
            TypeCode.Int64 => Math.Min((long)a, (long)b),
            TypeCode.UInt64 => Math.Min((ulong)a, (ulong)b),
            TypeCode.Single => Math.Min((float)a, (float)b),
            TypeCode.Double => Math.Min((double)a, (double)b),
            TypeCode.Decimal => Math.Min((decimal)a, (decimal)b),
            _ => null
        };
    }

    private static TypeCode ConvertToHighestPrecision(ref object a, ref object b, CultureInfo cultureInfo)
    {
        var typeCodeA = Type.GetTypeCode(a.GetType());
        var typeCodeB = Type.GetTypeCode(b.GetType());

        if (typeCodeA == typeCodeB)
            return typeCodeA;

        if (TypeCodeBitSize(typeCodeA, out var floatingPointA) is not { } bitSizeA)
            return TypeCode.Empty;

        if (TypeCodeBitSize(typeCodeB, out var floatingPointB) is not { } bitSizeB)
            return TypeCode.Empty;

        if ((floatingPointA && !floatingPointB) || (bitSizeA > bitSizeB))
        {
            b = Convert.ChangeType(b, typeCodeA, cultureInfo);
            return typeCodeA;
        }

        if (bitSizeA == bitSizeB)
        {
            bool isUnsignedA = TypeHelper.IsUnsignedType(a);
            bool isUnsignedB = TypeHelper.IsUnsignedType(b);

            if (isUnsignedA || isUnsignedB)
            {
                var extendedType = TypeHelper.TypeCodeExpandBits(typeCodeA);
                if (extendedType != TypeCode.Empty)
                {
                    a = Convert.ChangeType(a, extendedType, cultureInfo);
                    b = Convert.ChangeType(b, extendedType, cultureInfo);
                    return extendedType;
                }

                return typeCodeA;
            }
        }

        a = Convert.ChangeType(a, typeCodeB, cultureInfo);
        return typeCodeB;
    }

    private static int? TypeCodeBitSize(TypeCode typeCode, out bool floatingPoint)
    {
        floatingPoint = false;
        switch (typeCode)
        {
            case TypeCode.SByte:
            case TypeCode.Byte:
                return 8;
            case TypeCode.Int16:
            case TypeCode.UInt16:
                return 16;
            case TypeCode.Int32:
            case TypeCode.UInt32:
                return 32;
            case TypeCode.Int64:
            case TypeCode.UInt64:
                return 64;
            case TypeCode.Single:
                floatingPoint = true;
                return 32;
            case TypeCode.Double:
                floatingPoint = true;
                return 64;
            case TypeCode.Decimal:
                floatingPoint = true;
                return 128;
            default: return null;
        }
    }

    public static object Abs(object? a, MathHelperOptions options)
    {
        if (options.DecimalAsDefault)
            return Math.Abs(ConvertToDecimal(a, options));

        return Math.Abs(ConvertToDouble(a, options));
    }

    public static object Acos(object? a, MathHelperOptions options)
    {
        return Math.Acos(ConvertToDouble(a, options));
    }

    public static object Asin(object? a, MathHelperOptions options)
    {
        return Math.Asin(ConvertToDouble(a, options));
    }

    public static object Atan(object? a, MathHelperOptions options)
    {
        return Math.Atan(ConvertToDouble(a, options));
    }

    public static object Atan2(object? a, object? b, MathHelperOptions options)
    {
        return Math.Atan2(ConvertToDouble(a, options), ConvertToDouble(b, options));
    }

    public static object Ceiling(object? a, MathHelperOptions options)
    {
        if (options.DecimalAsDefault)
            return Math.Ceiling(ConvertToDecimal(a, options));

        return Math.Ceiling(ConvertToDouble(a, options));
    }

    public static object Cos(object? a, MathHelperOptions options)
    {
        return Math.Cos(ConvertToDouble(a, options));
    }

    public static object Exp(object? a, MathHelperOptions options)
    {
        return Math.Exp(ConvertToDouble(a, options));
    }

    public static object Floor(object? a, MathHelperOptions options)
    {
        if (options.DecimalAsDefault)
            return Math.Floor(ConvertToDecimal(a, options));

        return Math.Floor(ConvertToDouble(a, options));
    }

    // ReSharper disable once InconsistentNaming
    public static object IEEERemainder(object? a, object? b, MathHelperOptions options)
    {
        return Math.IEEERemainder(ConvertToDouble(a, options), ConvertToDouble(b, options));
    }

    public static object Ln(object? a, MathHelperOptions options)
    {
        return Math.Log(ConvertToDouble(a, options));
    }

    public static object Log(object? a, object? b, MathHelperOptions options)
    {
        return Math.Log(ConvertToDouble(a, options), ConvertToDouble(b, options));
    }

    public static object Log10(object? a, MathHelperOptions options)
    {
        return Math.Log10(ConvertToDouble(a, options));
    }

    public static object Pow(object? a, object? b, MathHelperOptions options)
    {
        if (options.DecimalAsDefault)
        {
            var @base = new BigDecimal(ConvertToDecimal(a, options));
            var exponent = new BigInteger(ConvertToDecimal(b, options));

            return (decimal)BigDecimal.Pow(@base, exponent);
        }

        return Math.Pow(ConvertToDouble(a, options), ConvertToDouble(b, options));
    }

    public static object Round(object? a, object? b, MidpointRounding rounding, MathHelperOptions options)
    {
        if (options.DecimalAsDefault)
            return Math.Round(ConvertToDecimal(a, options), ConvertToInt(b, options), rounding);

        return Math.Round(ConvertToDouble(a, options), ConvertToInt(b, options), rounding);
    }

    public static object Sign(object? a, MathHelperOptions options)
    {
        if (options.DecimalAsDefault)
            return Math.Sign(ConvertToDecimal(a, options));

        return Math.Sign(ConvertToDouble(a, options));
    }

    public static object Sin(object? a, MathHelperOptions options)
    {
        return Math.Sin(ConvertToDouble(a, options));
    }

    public static object Sqrt(object? a, MathHelperOptions options)
    {
        return Math.Sqrt(ConvertToDouble(a, options));
    }

    public static object Tan(object? a, MathHelperOptions options)
    {
        return Math.Tan(ConvertToDouble(a, options));
    }

    public static object Truncate(object? a, MathHelperOptions options)
    {
        if (options.DecimalAsDefault)
            return Math.Truncate(ConvertToDecimal(a, options));

        return Math.Truncate(ConvertToDouble(a, options));
    }

    private static object ConvertIfNeeded(object value, MathHelperOptions options)
    {
        return value switch
        {
            char ch when options is { DecimalAsDefault: true, AllowCharValues: false } => decimal.Parse(ch.ToString(), options.CultureInfo),
            string s when options is { DecimalAsDefault: true } => decimal.Parse(s, options.CultureInfo),
            char ch => (int)ch,
            string s => double.Parse(s, options.CultureInfo),
            bool boolean when options.AllowBooleanCalculation => boolean ? 1 : 0,
            _ => value
        };
    }

    private static double ConvertToDouble(object? value, MathHelperOptions options)
    {
        return value switch
        {
            double @double => @double,
            char ch => Convert.ToDouble(ch.ToString(), options.CultureInfo),
            _ => Convert.ToDouble(value, options.CultureInfo)
        };
    }

    private static decimal ConvertToDecimal(object? value, MathHelperOptions options)
    {
        return value switch
        {
            decimal @decimal => @decimal,
            char ch => Convert.ToDecimal(ch.ToString(), options.CultureInfo),
            _ => Convert.ToDecimal(value, options.CultureInfo)
        };
    }

    private static int ConvertToInt(object? value, MathHelperOptions options)
    {
        return value switch
        {
            int i => i,
            char ch => Convert.ToInt32(ch.ToString(), options.CultureInfo),
            _ => Convert.ToInt32(value, options.CultureInfo)
        };
    }

    private static object ExecuteOperation(object a, object b, char operatorName, CultureInfo culture, Func<object, object, object> func)
    {
        var typeCode = ConvertToHighestPrecision(ref a, ref b, culture);

        if (typeCode == TypeCode.Empty || typeCode == TypeCode.Boolean)
        {
            throw new InvalidOperationException(
                $"Operator '{operatorName}' not implemented for operands of types {a?.GetType().ToString() ?? "null"} and {b?.GetType().ToString() ?? "null"}");
        }

        return func(a, b);
    }

    private static void CheckOverflow(dynamic value)
    {
        switch (value)
        {
            case double doubleVal when double.IsInfinity(doubleVal):
            case float floatValue when float.IsInfinity(floatValue):
                throw new OverflowException("Arithmetic operation resulted in an overflow");
        }
    }
}