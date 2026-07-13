using System.Numerics;
using ExtendedNumerics;
using NCalc.Parser;

namespace NCalc.Helpers;

/// <summary>
/// Utilities for doing mathematical operations between different object types.
/// </summary>
public static class MathHelper
{
    private const int MaxFactorialInput = 170;

    public static object? Add(object? a, object? b, MathOptions options, CultureInfo cultureInfo)
    {
        if (a == null || b == null)
            return null;

        a = ConvertIfNeeded(a, options, cultureInfo);
        b = ConvertIfNeeded(b, options, cultureInfo);

        ConvertToHighestPrecision(ref a, ref b, cultureInfo);
        return BinaryOperators.Add(a, b, options.OverflowProtection);
    }

    public static object? Subtract(object? a, object? b, MathOptions options, CultureInfo cultureInfo)
    {
        if (a == null || b == null)
            return null;

        a = ConvertIfNeeded(a, options, cultureInfo);
        b = ConvertIfNeeded(b, options, cultureInfo);

        ConvertToHighestPrecision(ref a, ref b, cultureInfo);
        return BinaryOperators.Subtract(a, b, options.OverflowProtection);
    }

    public static object? Multiply(object? a, object? b, MathOptions options, CultureInfo cultureInfo)
    {
        if (a == null || b == null)
            return null;

        a = ConvertIfNeeded(a, options, cultureInfo);
        b = ConvertIfNeeded(b, options, cultureInfo);

        ConvertToHighestPrecision(ref a, ref b, cultureInfo);
        return BinaryOperators.Multiply(a, b, options.OverflowProtection);
    }

    public static object? Divide(object? a, object? b, MathOptions options, CultureInfo cultureInfo)
    {
        if (a == null || b == null)
            return null;

        a = ConvertIfNeeded(a, options, cultureInfo);
        b = ConvertIfNeeded(b, options, cultureInfo);

        if (!(TypeHelper.IsReal(a) || TypeHelper.IsReal(b)))
            a = Convert.ToDouble(a, cultureInfo);

        ConvertToHighestPrecision(ref a, ref b, cultureInfo);
        return BinaryOperators.Divide(a, b, options.OverflowProtection);
    }

    public static object? Modulo(object? a, object? b, MathOptions options, CultureInfo cultureInfo)
    {
        if (a == null || b == null)
            return null;

        a = ConvertIfNeeded(a, options, cultureInfo);
        b = ConvertIfNeeded(b, options, cultureInfo);

        ConvertToHighestPrecision(ref a, ref b, cultureInfo);
        return BinaryOperators.Modulo(a, b);
    }

    public static object? Max(object? a, object? b, MathOptions options, CultureInfo cultureInfo)
    {
        switch (a)
        {
            case null when b == null:
                return null;
            case null:
                return b;
        }

        if (b == null)
        {
            return a;
        }

        a = ConvertIfNeeded(a, options, cultureInfo);
        b = ConvertIfNeeded(b, options, cultureInfo);

        var typeCode = ConvertToHighestPrecision(ref a, ref b, cultureInfo);

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

    public static object? Min(object? a, object? b, MathOptions options, CultureInfo cultureInfo)
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

        a = ConvertIfNeeded(a, options, cultureInfo);
        b = ConvertIfNeeded(b, options, cultureInfo);

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

        if (TypeCodeBitSize(typeCodeA, out var floatingPointA) is not { } bitSizeA || TypeCodeBitSize(typeCodeB, out var floatingPointB) is not { } bitSizeB)
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

    public static object Abs(object? a, MathOptions options, CultureInfo cultureInfo)
    {
        if (a is decimal aDecimal)
        {
            return Math.Abs(aDecimal);
        }

        if (options.FloatingPointNumberType is FloatingPointNumberType.Decimal)
        {
            return Math.Abs(ConvertToDecimal(a, cultureInfo));
        }

        return Math.Abs(ConvertToDouble(a, cultureInfo));
    }

    public static object Acos(object? a, CultureInfo cultureInfo)
    {
        return Math.Acos(ConvertToDouble(a, cultureInfo));
    }

    public static object Asin(object? a, CultureInfo cultureInfo)
    {
        return Math.Asin(ConvertToDouble(a, cultureInfo));
    }

    public static object Atan(object? a, CultureInfo cultureInfo)
    {
        return Math.Atan(ConvertToDouble(a, cultureInfo));
    }

    public static object Atan2(object? a, object? b, CultureInfo cultureInfo)
    {
        return Math.Atan2(ConvertToDouble(a, cultureInfo), ConvertToDouble(b, cultureInfo));
    }

    public static object Ceiling(object? a, MathOptions options, CultureInfo cultureInfo)
    {
        if (a is decimal aDecimal)
        {
            return Math.Ceiling(aDecimal);
        }

        if (options.FloatingPointNumberType is FloatingPointNumberType.Decimal)
        {
            return Math.Ceiling(ConvertToDecimal(a, cultureInfo));
        }

        return Math.Ceiling(ConvertToDouble(a, cultureInfo));
    }

    public static object Cos(object? a, CultureInfo cultureInfo)
    {
        return Math.Cos(ConvertToDouble(a, cultureInfo));
    }

    public static object Exp(object? a, CultureInfo cultureInfo)
    {
        return Math.Exp(ConvertToDouble(a, cultureInfo));
    }

    public static object Floor(object? a, MathOptions options, CultureInfo cultureInfo)
    {
        if (a is decimal aDecimal)
        {
            return Math.Floor(aDecimal);
        }

        if (options.FloatingPointNumberType is FloatingPointNumberType.Decimal)
        {
            return Math.Floor(ConvertToDecimal(a, cultureInfo));
        }

        return Math.Floor(ConvertToDouble(a, cultureInfo));
    }

    // ReSharper disable once InconsistentNaming
    public static object IEEERemainder(object? a, object? b, CultureInfo cultureInfo)
    {
        return Math.IEEERemainder(ConvertToDouble(a, cultureInfo), ConvertToDouble(b, cultureInfo));
    }

    public static object Ln(object? a, CultureInfo cultureInfo)
    {
        return Math.Log(ConvertToDouble(a, cultureInfo));
    }

    public static object Log(object? a, object? b, CultureInfo cultureInfo)
    {
        return Math.Log(ConvertToDouble(a, cultureInfo), ConvertToDouble(b, cultureInfo));
    }

    public static object Log10(object? a, CultureInfo cultureInfo)
    {
        return Math.Log10(ConvertToDouble(a, cultureInfo));
    }

    public static object Pow(object? a, object? b, MathOptions options, CultureInfo cultureInfo)
    {
        if (a is decimal aDecimal)
        {
            var @base = new BigDecimal(aDecimal);
            var exponent = new BigInteger(ConvertToDecimal(b, cultureInfo));

            return (decimal)BigDecimal.Pow(@base, exponent);
        }

        if (options.FloatingPointNumberType is FloatingPointNumberType.Decimal)
        {
            var @base = new BigDecimal(ConvertToDecimal(a, cultureInfo));
            var exponent = new BigInteger(ConvertToDecimal(b, cultureInfo));

            return (decimal)BigDecimal.Pow(@base, exponent);
        }

        return Math.Pow(ConvertToDouble(a, cultureInfo), ConvertToDouble(b, cultureInfo));
    }

    public static object Round(object? a, object? b, MidpointRounding rounding, MathOptions options, CultureInfo cultureInfo)
    {
        var digits = ConvertToInt(b, cultureInfo);

        if (a is decimal aDecimal)
        {
            return Math.Round(aDecimal, digits, rounding);
        }

        if (options.FloatingPointNumberType is FloatingPointNumberType.Decimal)
        {
            return Math.Round(ConvertToDecimal(a, cultureInfo), digits, rounding);
        }

        return Math.Round(ConvertToDouble(a, cultureInfo), digits, rounding);
    }

    public static object Sign(object? a, MathOptions options, CultureInfo cultureInfo)
    {
        if (a is decimal aDecimal)
        {
            return Math.Sign(aDecimal);
        }

        if (options.FloatingPointNumberType is FloatingPointNumberType.Decimal)
        {
            return Math.Sign(ConvertToDecimal(a, cultureInfo));
        }

        return Math.Sign(ConvertToDouble(a, cultureInfo));
    }

    public static object Sin(object? a, CultureInfo cultureInfo)
    {
        return Math.Sin(ConvertToDouble(a, cultureInfo));
    }

    public static object Sqrt(object? a, CultureInfo cultureInfo)
    {
        return Math.Sqrt(ConvertToDouble(a, cultureInfo));
    }

    public static object Tan(object? a, CultureInfo cultureInfo)
    {
        return Math.Tan(ConvertToDouble(a, cultureInfo));
    }

    public static object Truncate(object? a, MathOptions options, CultureInfo cultureInfo)
    {
        if (a is decimal aDecimal)
        {
            return Math.Truncate(aDecimal);
        }

        if (options.FloatingPointNumberType is FloatingPointNumberType.Decimal)
        {
            return Math.Truncate(ConvertToDecimal(a, cultureInfo));
        }

        return Math.Truncate(ConvertToDouble(a, cultureInfo));
    }

    private static object ConvertIfNeeded(object value, MathOptions options, CultureInfo cultureInfo)
    {
        return value switch
        {
            char ch => (int)ch,
            string s => ParseNumber(s, options, cultureInfo),
            bool boolean when options.AllowBooleanCalculation => boolean ? 1 : 0,
            _ => value
        };
    }

    private static object ParseNumber(string value, MathOptions options, CultureInfo cultureInfo)
    {
        if (options.FloatingPointNumberType is FloatingPointNumberType.Decimal)
            return decimal.Parse(value, cultureInfo);

        if (options.IntegerNumberType is IntegerNumberType.Int64)
            return long.Parse(value, cultureInfo);

        return double.Parse(value, cultureInfo);
    }

    private static double ConvertToDouble(object? value, CultureInfo cultureInfo)
    {
        return value switch
        {
            double @double => @double,
            char ch => Convert.ToDouble(ch.ToString(), cultureInfo),
            _ => Convert.ToDouble(value, cultureInfo)
        };
    }

    private static decimal ConvertToDecimal(object? value, CultureInfo cultureInfo)
    {
        return value switch
        {
            char ch => Convert.ToDecimal(ch.ToString(), cultureInfo),
            decimal @decimal => @decimal,
            _ => Convert.ToDecimal(value, cultureInfo)
        };
    }

    private static int ConvertToInt(object? value, CultureInfo cultureInfo)
    {
        return value switch
        {
            int i => i,
            char ch => Convert.ToInt32(ch.ToString(), cultureInfo),
            _ => Convert.ToInt32(value, cultureInfo)
        };
    }

    public static object Factorial(object? result)
    {
        return result switch
        {
#if NET
            int v => CalculateFactorial(ValidateFactorialInput(v)),
            long v => CalculateFactorial(ValidateFactorialInput(v)),
            float v => CalculateFactorial(ValidateFactorialInput(v)),
            double v => CalculateFactorial(ValidateFactorialInput(v)),
            decimal v => CalculateFactorial(ValidateFactorialInput(v)),
            BigInteger v => CalculateFactorial(ValidateFactorialInput(v)),
#else
            int v => CalculateFactorial(ValidateFactorialInput(v)),
            long v => CalculateFactorial(ValidateFactorialInput(v)),
            float v => CalculateFactorial(ValidateFactorialInput(v)),
            double v => CalculateFactorial(ValidateFactorialInput(v)),
            decimal v => CalculateFactorial(ValidateFactorialInput(v)),
            BigInteger v => CalculateFactorial(ValidateFactorialInput(v)),
#endif
            _ => throw new ArgumentException("Unsupported numeric type.", nameof(result)),
        };
    }

#if NET
    private static T ValidateFactorialInput<T>(T n) where T : INumber<T>
    {
        if (n < T.Zero)
            throw new ArgumentOutOfRangeException(nameof(n));

        if (n > T.CreateChecked(MaxFactorialInput))
        {
            throw new ArgumentOutOfRangeException(nameof(n),
                $"Factorial input must be less than or equal to {MaxFactorialInput}.");
        }

        return n;
    }

    private static float ValidateFactorialInput(float n)
    {
        if (float.IsNaN(n) || float.IsInfinity(n))
            throw new ArgumentOutOfRangeException(nameof(n));

        return ValidateFactorialInput<float>(n);
    }

    private static double ValidateFactorialInput(double n)
    {
        if (double.IsNaN(n) || double.IsInfinity(n))
            throw new ArgumentOutOfRangeException(nameof(n));

        return ValidateFactorialInput<double>(n);
    }
#else
    private static int ValidateFactorialInput(int n)
    {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n));

        if (n > MaxFactorialInput)
            throw new ArgumentOutOfRangeException(nameof(n),
                $"Factorial input must be less than or equal to {MaxFactorialInput}.");

        return n;
    }

    private static long ValidateFactorialInput(long n)
    {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n));

        if (n > MaxFactorialInput)
            throw new ArgumentOutOfRangeException(nameof(n),
                $"Factorial input must be less than or equal to {MaxFactorialInput}.");

        return n;
    }

    private static float ValidateFactorialInput(float n)
    {
        if (float.IsNaN(n) || float.IsInfinity(n) || n < 0)
            throw new ArgumentOutOfRangeException(nameof(n));

        if (n > MaxFactorialInput)
            throw new ArgumentOutOfRangeException(nameof(n),
                $"Factorial input must be less than or equal to {MaxFactorialInput}.");

        return n;
    }

    private static double ValidateFactorialInput(double n)
    {
        if (double.IsNaN(n) || double.IsInfinity(n) || n < 0)
            throw new ArgumentOutOfRangeException(nameof(n));

        if (n > MaxFactorialInput)
            throw new ArgumentOutOfRangeException(nameof(n),
                $"Factorial input must be less than or equal to {MaxFactorialInput}.");

        return n;
    }

    private static decimal ValidateFactorialInput(decimal n)
    {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n));

        if (n > MaxFactorialInput)
            throw new ArgumentOutOfRangeException(nameof(n),
                $"Factorial input must be less than or equal to {MaxFactorialInput}.");

        return n;
    }

    private static BigInteger ValidateFactorialInput(BigInteger n)
    {
        if (n < BigInteger.Zero)
            throw new ArgumentOutOfRangeException(nameof(n));

        if (n > MaxFactorialInput)
            throw new ArgumentOutOfRangeException(nameof(n),
                $"Factorial input must be less than or equal to {MaxFactorialInput}.");

        return n;
    }
#endif

#if NET
    private static T CalculateFactorial<T>(T n) where T : INumber<T>
    {
        var one = T.One;
        var r = one;

        for (var i = one + one; i <= n; i++)
            r *= i;

        return r;
    }
#else
    private static dynamic CalculateFactorial(dynamic n)
    {
        var r = 1;

        for (var i = 1 + 1; i <= n; i++)
            r *= i;

        return r;
    }
#endif
}
