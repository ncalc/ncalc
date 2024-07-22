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
        return ExecuteOperation(a, b, '+', func);
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
        return ExecuteOperation(a, b, '-', func);
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
        return ExecuteOperation(a, b, '*', func);
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

        var func = options.OverflowProtection ? DivideFuncChecked : DivideFunc;
        return ExecuteOperation(a, b, '/', func);
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

        return ExecuteOperation(a, b, '%', ModuloFunc);
    }

    public static object? Max(object a, object b)
    {
        return Max(a, b, CultureInfo.CurrentCulture);
    }

    public static object? Max(object? a, object? b, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);
        b = ConvertIfNeeded(b, options);

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

        TypeCode typeCode = ConvertToHighestPrecision(ref a, ref b, options.CultureInfo);

        return typeCode switch
        {
            TypeCode.Byte => Math.Max((byte)a!, (byte)b!),
            TypeCode.SByte => Math.Max((sbyte)a!, (sbyte)b!),
            TypeCode.Int16 => Math.Max((short)a!, (short)b!),
            TypeCode.UInt16 => Math.Max((ushort)a!, (ushort)b!),
            TypeCode.Int32 => Math.Max((int)a!, (int)b!),
            TypeCode.UInt32 => Math.Max((uint)a!, (uint)b!),
            TypeCode.Int64 => Math.Max((long)a!, (long)b!),
            TypeCode.UInt64 => Math.Max((ulong)a!, (ulong)b!),
            TypeCode.Single => Math.Max((float)a!, (float)b!),
            TypeCode.Double => Math.Max((double)a!, (double)b!),
            TypeCode.Decimal => Math.Max((decimal)a!, (decimal)b!),
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

        a = ConvertIfNeeded(a, options);
        b = ConvertIfNeeded(b, options);

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

        var typeCode = ConvertToHighestPrecision(ref a, ref b, cultureInfo);

        return typeCode switch
        {
            TypeCode.Byte => Math.Min((byte)a!, (byte)b!),
            TypeCode.SByte => Math.Min((sbyte)a!, (sbyte)b!),
            TypeCode.Int16 => Math.Min((short)a!, (short)b!),
            TypeCode.UInt16 => Math.Min((ushort)a!, (ushort)b!),
            TypeCode.Int32 => Math.Min((int)a!, (int)b!),
            TypeCode.UInt32 => Math.Min((uint)a!, (uint)b!),
            TypeCode.Int64 => Math.Min((long)a!, (long)b!),
            TypeCode.UInt64 => Math.Min((ulong)a!, (ulong)b!),
            TypeCode.Single => Math.Min((float)a!, (float)b!),
            TypeCode.Double => Math.Min((double)a!, (double)b!),
            TypeCode.Decimal => Math.Min((decimal)a!, (decimal)b!),
            _ => null
        };
    }

    private static TypeCode ConvertToHighestPrecision(ref object? a, ref object? b, CultureInfo cultureInfo)
    {
        if (a == null || b == null)
            return TypeCode.Empty;

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
        a = ConvertIfNeeded(a, options);

        if (options.DecimalAsDefault)
            return Math.Abs(Convert.ToDecimal(a));

        return Math.Abs(Convert.ToDouble(a));
    }

    public static object Acos(object? a, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);
        return Math.Acos(Convert.ToDouble(a));
    }

    public static object Asin(object? a, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);
        return Math.Asin(Convert.ToDouble(a));
    }

    public static object Atan(object? a, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);

        return Math.Atan(Convert.ToDouble(a));
    }

    public static object Atan2(object? a, object? b, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);
        b = ConvertIfNeeded(b, options);
        return Math.Atan2(Convert.ToDouble(a), Convert.ToDouble(b));
    }

    public static object Ceiling(object? a, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);

        if (options.DecimalAsDefault)
            return Math.Ceiling(Convert.ToDecimal(a));

        return Math.Ceiling(Convert.ToDouble(a));
    }

    public static object Cos(object? a, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);
        return Math.Cos(Convert.ToDouble(a));
    }

    public static object Exp(object? a, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);
        return Math.Exp(Convert.ToDouble(a));
    }

    public static object Floor(object? a, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);

        if (options.DecimalAsDefault)
            return Math.Floor(Convert.ToDecimal(a));

        return Math.Floor(Convert.ToDouble(a));
    }

    // ReSharper disable once InconsistentNaming
    public static object IEEERemainder(object? a, object? b, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);
        b = ConvertIfNeeded(b, options);
        return Math.IEEERemainder(Convert.ToDouble(a), Convert.ToDouble(b));
    }

    public static object Ln(object? a, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);
        return Math.Log(Convert.ToDouble(a));
    }

    public static object Log(object? a, object? b, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);
        b = ConvertIfNeeded(b, options);
        return Math.Log(Convert.ToDouble(a), Convert.ToDouble(b));
    }

    public static object Log10(object? a, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);
        return Math.Log10(Convert.ToDouble(a));
    }

    public static object Pow(object? a, object? b, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);
        b = ConvertIfNeeded(b, options);

        if (options.DecimalAsDefault)
        {
            BigDecimal @base = new BigDecimal(Convert.ToDecimal(a));
            BigInteger exponent = new BigInteger(Convert.ToDecimal(b));

            return (decimal)BigDecimal.Pow(@base, exponent);
        }

        return Math.Pow(Convert.ToDouble(a), Convert.ToDouble(b));
    }

    public static object Round(object? a, object? b, MidpointRounding rounding, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);
        b = ConvertIfNeeded(b, options);

        if (options.DecimalAsDefault)
            return Math.Round(Convert.ToDecimal(a), Convert.ToInt16(b), rounding);

        return Math.Round(Convert.ToDouble(a), Convert.ToInt16(b), rounding);
    }

    public static object Sign(object? a, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);

        if (options.DecimalAsDefault)
            return Math.Sign(Convert.ToDecimal(a));

        return Math.Sign(Convert.ToDouble(a));
    }

    public static object Sin(object? a, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);
        return Math.Sin(Convert.ToDouble(a));
    }

    public static object Sqrt(object? a, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);
        return Math.Sqrt(Convert.ToDouble(a));
    }

    public static object Tan(object? a, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);

        return Math.Tan(Convert.ToDouble(a));
    }

    public static object Truncate(object? a, MathHelperOptions options)
    {
        a = ConvertIfNeeded(a, options);

        if (options.DecimalAsDefault)
            return Math.Truncate(Convert.ToDecimal(a));

        return Math.Truncate(Convert.ToDouble(a));
    }

    private static object? ConvertIfNeeded(object? value, MathHelperOptions options)
    {
        return value switch
        {
            char when options is { DecimalAsDefault: true, AllowCharValues: false } => decimal.Parse(value.ToString()!, options.CultureInfo),
            string when options is { DecimalAsDefault: true } => decimal.Parse(value.ToString()!, options.CultureInfo),
            char when options is { AllowCharValues: false } => double.Parse(value.ToString()!, options.CultureInfo),
            string => double.Parse(value.ToString()!, options.CultureInfo),
            bool boolean when options.AllowBooleanCalculation => boolean ? 1 : 0,
            _ => value
        };
    }

    private static object ExecuteOperation(object? a, object? b, char operatorName, Func<object, object, object> func)
    {
        return a switch
        {
            bool => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'bool' and {b?.GetType().ToString() ?? "null"}"),
            byte b1 => ExecuteByteOperation(b1, b, operatorName, func),
            char @char => ExecuteCharOperation(@char, b, operatorName, func),
            sbyte @sbyte => ExecuteSByteOperation(@sbyte, b, operatorName, func),
            short s => ExecuteShortOperation(s, b, operatorName, func),
            ushort @ushort => ExecuteUShortOperation(@ushort, b, operatorName, func),
            int i => ExecuteIntOperation(i, b, operatorName, func),
            uint u => ExecuteUIntOperation(u, b, operatorName, func),
            long l => ExecuteLongOperation(l, b, operatorName, func),
            ulong @ulong => ExecuteULongOperation(@ulong, b, operatorName, func),
            float f => ExecuteFloatOperation(f, b, operatorName, func),
            double d => ExecuteDoubleOperation(d, b, operatorName, func),
            decimal @decimal => ExecuteDecimalOperation(@decimal, b, operatorName, func),
            _ => throw new InvalidOperationException(
                                $"Operator '{operatorName}' not implemented for operands of types {a?.GetType().ToString() ?? "null"} and {b?.GetType().ToString() ?? "null"}"),
        };
    }

    private static object ExecuteByteOperation(byte left, object? right, char operatorName, Func<object, object, object> func)
    {
        return right switch
        {
            bool => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'byte' and 'bool'"),
            byte or char or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal => func(left, right),
            _ => throw new InvalidOperationException(
                                $"Operator '{operatorName}' not implemented for 'byte' and {right?.GetType().ToString() ?? "null"}"),
        };
    }

    private static object ExecuteCharOperation(char left, object? right, char operatorName, Func<object, object, object> func)
    {
        return right switch
        {
            bool => throw new InvalidOperationException(
                $"Operator '{operatorName}' can't be applied to operands of types 'char' and 'bool'"),
            byte or char or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal => func(left, right),
            _ => throw new InvalidOperationException(
                $"Operator '{operatorName}' not implemented for 'char' and {right?.GetType().ToString() ?? "null"}"),
        };
    }

    private static object ExecuteSByteOperation(sbyte left, object? right, char operatorName, Func<object, object, object> func)
    {
        return right switch
        {
            bool => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'sbyte' and 'bool'"),
            byte or char or sbyte or short or ushort or int or uint or long or float or double or decimal => func(left, right),
            ulong => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'sbyte' and 'ulong'"),
            _ => throw new InvalidOperationException(
                                $"Operator '{operatorName}' not implemented for 'sbyte' and {right?.GetType().ToString() ?? "null"}"),
        };
    }

    private static object ExecuteShortOperation(short left, object? right, char operatorName, Func<object, object, object> func)
    {
        return right switch
        {
            bool => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'short' and 'bool'"),
            byte or char or sbyte or short or ushort or int or uint or long or float or double or decimal => func(left, right),
            ulong => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'short' and 'ulong'"),
            _ => throw new InvalidOperationException(
                                $"Operator '{operatorName}' not implemented for types 'short' and {right?.GetType().ToString() ?? "null"}"),
        };
    }

    private static object ExecuteUShortOperation(ushort left, object? right, char operatorName, Func<object, object, object> func)
    {
        return right switch
        {
            bool => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'ushort' and 'bool'"),
            byte or char or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal => func(left, right),
            _ => throw new InvalidOperationException(
                                $"Operator '{operatorName}' not implemented for types 'ushort' and {right?.GetType().ToString() ?? "null"}"),
        };
    }

    private static object ExecuteIntOperation(int left, object? right, char operatorName, Func<object, object, object> func)
    {
        return right switch
        {
            bool => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'int' and 'bool'"),
            byte or char or sbyte or short or ushort or int or uint or long or float or double or decimal => func(left, right),
            ulong => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'int' and 'ulong'"),
            _ => throw new InvalidOperationException(
                                $"Operator '{operatorName}' not implemented for types 'int' and {right?.GetType().ToString() ?? "null"}"),
        };
    }

    private static object ExecuteUIntOperation(uint left, object? right, char operatorName, Func<object, object, object> func)
    {
        return right switch
        {
            bool => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'uint' and 'bool'"),
            byte or char or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal => func(left, right),
            _ => throw new InvalidOperationException(
                                $"Operator '{operatorName}' not implemented for types 'uint' and {right?.GetType().ToString() ?? "null"}"),
        };
    }

    private static object ExecuteLongOperation(long left, object? right, char operatorName, Func<object, object, object> func)
    {
        return right switch
        {
            bool => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'long' and 'bool'"),
            byte or char or sbyte or short or ushort or int or uint or long or float or double or decimal => func(left, right),
            ulong => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'long' and 'ulong'"),
            _ => throw new InvalidOperationException(
                                $"Operator '{operatorName}' not implemented for types 'long' and {right?.GetType().ToString() ?? "null"}"),
        };
    }

    private static object ExecuteULongOperation(ulong left, object? right, char operatorName, Func<object, object, object> func)
    {
        return right switch
        {
            bool => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'ulong' and 'bool'"),
            sbyte => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'ulong' and 'sbyte'"),
            short => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'ulong' and 'short'"),
            int => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'ulong' and 'int'"),
            long => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'ulong' and 'long'"),
            byte or char or ushort or uint or ulong or float or double or decimal => func(left, right),
            _ => throw new InvalidOperationException(
                                $"Operator '{operatorName}' not implemented for types 'ulong' and {right?.GetType().ToString() ?? "null"}"),
        };
    }

    private static object ExecuteFloatOperation(float left, object? right, char operatorName, Func<object, object, object> func)
    {
        return right switch
        {
            bool => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'float' and 'bool'"),
            byte or char or sbyte or short or ushort or int or uint or long or ulong or float or double => func(left, right),
            decimal => func(Convert.ToDecimal(left), right),
            _ => throw new InvalidOperationException(
                                $"Operator '{operatorName}' not implemented for types 'float' and {right?.GetType().ToString() ?? "null"}"),
        };
    }

    private static object ExecuteDoubleOperation(double left, object? right, char operatorName, Func<object, object, object> func)
    {
        return right switch
        {
            bool => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'double' and 'bool'"),
            byte or char or sbyte or short or ushort or int or uint or long or ulong or float or double => func(left, right),
            decimal => func(Convert.ToDecimal(left), right),
            _ => throw new InvalidOperationException(
                                $"Operator '{operatorName}' not implemented for types 'double' and {right?.GetType().ToString() ?? "null"}"),
        };
    }

    private static object ExecuteDecimalOperation(decimal left, object? right, char operatorName, Func<object, object, object> func)
    {
        return right switch
        {
            bool => throw new InvalidOperationException(
                                $"Operator '{operatorName}' can't be applied to operands of types 'decimal' and 'bool'"),
            byte or char or sbyte or short or ushort or int or uint or long or ulong or decimal => func(left, right),
            float or double => func(left, Convert.ToDecimal(right)),
            _ => throw new InvalidOperationException(
                                $"Operator '{operatorName}' not implemented for types 'decimal' and {right?.GetType().ToString() ?? "null"}"),
        };
    }

    private static void CheckOverflow(dynamic value)
    {
        switch (value)
        {
            case double doubleVal when double.IsInfinity(doubleVal):
                throw new OverflowException("Arithmetic operation resulted in an overflow");
            case float floatValue when float.IsInfinity(floatValue):
                throw new OverflowException("Arithmetic operation resulted in an overflow");
        }
    }
}