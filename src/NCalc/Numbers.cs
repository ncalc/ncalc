using System;
using System.Globalization;

namespace NCalc;

public static class Numbers
{
    private static object ConvertIfString(object s, CultureInfo cultureInfo)
    {
        return s is string or char ? decimal.Parse(s.ToString(), cultureInfo) : s;
    }

    public static object Add(object a, object b)
    {
        return Add(a, b, CultureInfo.CurrentCulture);
    }

    public static object Add(object a, object b, CultureInfo cultureInfo)
    {
        a = ConvertIfString(a, cultureInfo);
        b = ConvertIfString(b, cultureInfo);

        var typeCodeA = Type.GetTypeCode(a.GetType());
        var typeCodeB = Type.GetTypeCode(b.GetType());

        return typeCodeA switch
        {
            TypeCode.Boolean => throw new InvalidOperationException(
                $"Operator '+' can't be applied to operands of types 'bool' and {typeCodeB}"),
            TypeCode.Byte => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'byte' and 'bool'"),
                TypeCode.Byte => (byte)a + (byte)b,
                TypeCode.SByte => (byte)a + (sbyte)b,
                TypeCode.Int16 => (byte)a + (short)b,
                TypeCode.UInt16 => (byte)a + (ushort)b,
                TypeCode.Int32 => (byte)a + (int)b,
                TypeCode.UInt32 => (byte)a + (uint)b,
                TypeCode.Int64 => (byte)a + (long)b,
                TypeCode.UInt64 => (byte)a + (ulong)b,
                TypeCode.Single => (byte)a + (float)b,
                TypeCode.Double => (byte)a + (double)b,
                TypeCode.Decimal => (byte)a + (decimal)b,
                _ => throw new InvalidOperationException($"Operator '+' not implemented for 'byte' and {typeCodeB}")
            }),
            TypeCode.SByte => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'sbyte' and 'bool'"),
                TypeCode.Byte => (sbyte)a + (byte)b,
                TypeCode.SByte => (sbyte)a + (sbyte)b,
                TypeCode.Int16 => (sbyte)a + (short)b,
                TypeCode.UInt16 => (sbyte)a + (ushort)b,
                TypeCode.Int32 => (sbyte)a + (int)b,
                TypeCode.UInt32 => (sbyte)a + (uint)b,
                TypeCode.Int64 => (sbyte)a + (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'sbyte' and 'ulong'"),
                TypeCode.Single => (sbyte)a + (float)b,
                TypeCode.Double => (sbyte)a + (double)b,
                TypeCode.Decimal => (sbyte)a + (decimal)b,
                _ => throw new InvalidOperationException($"Operator '+' not implemented for 'sbyte' and {typeCodeB}")
            }),
            TypeCode.Int16 => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'short' and 'bool'"),
                TypeCode.Byte => (short)a + (byte)b,
                TypeCode.SByte => (short)a + (sbyte)b,
                TypeCode.Int16 => (short)a + (short)b,
                TypeCode.UInt16 => (short)a + (ushort)b,
                TypeCode.Int32 => (short)a + (int)b,
                TypeCode.UInt32 => (short)a + (uint)b,
                TypeCode.Int64 => (short)a + (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'short' and 'ulong'"),
                TypeCode.Single => (short)a + (float)b,
                TypeCode.Double => (short)a + (double)b,
                TypeCode.Decimal => (short)a + (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '+' not implemented for types 'short' and {typeCodeB}")
            }),
            TypeCode.UInt16 => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'ushort' and 'bool'"),
                TypeCode.Byte => (ushort)a + (byte)b,
                TypeCode.SByte => (ushort)a + (sbyte)b,
                TypeCode.Int16 => (ushort)a + (short)b,
                TypeCode.UInt16 => (ushort)a + (ushort)b,
                TypeCode.Int32 => (ushort)a + (int)b,
                TypeCode.UInt32 => (ushort)a + (uint)b,
                TypeCode.Int64 => (ushort)a + (long)b,
                TypeCode.UInt64 => (ushort)a + (ulong)b,
                TypeCode.Single => (ushort)a + (float)b,
                TypeCode.Double => (ushort)a + (double)b,
                TypeCode.Decimal => (ushort)a + (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '+' not implemented for types 'ushort' and {typeCodeB}")
            }),
            TypeCode.Int32 => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'int' and 'bool'"),
                TypeCode.Byte => (int)a + (byte)b,
                TypeCode.SByte => (int)a + (sbyte)b,
                TypeCode.Int16 => (int)a + (short)b,
                TypeCode.UInt16 => (int)a + (ushort)b,
                TypeCode.Int32 => (int)a + (int)b,
                TypeCode.UInt32 => (int)a + (uint)b,
                TypeCode.Int64 => (int)a + (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'int' and 'ulong'"),
                TypeCode.Single => (int)a + (float)b,
                TypeCode.Double => (int)a + (double)b,
                TypeCode.Decimal => (int)a + (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '+' not implemented for types 'int' and {typeCodeB}")
            }),
            TypeCode.UInt32 => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'uint' and 'bool'"),
                TypeCode.Byte => (uint)a + (byte)b,
                TypeCode.SByte => (uint)a + (sbyte)b,
                TypeCode.Int16 => (uint)a + (short)b,
                TypeCode.UInt16 => (uint)a + (ushort)b,
                TypeCode.Int32 => (uint)a + (int)b,
                TypeCode.UInt32 => (uint)a + (uint)b,
                TypeCode.Int64 => (uint)a + (long)b,
                TypeCode.UInt64 => (uint)a + (ulong)b,
                TypeCode.Single => (uint)a + (float)b,
                TypeCode.Double => (uint)a + (double)b,
                TypeCode.Decimal => (uint)a + (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '+' not implemented for types 'uint' and {typeCodeB}")
            }),
            TypeCode.Int64 => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'long' and 'bool'"),
                TypeCode.Byte => (long)a + (byte)b,
                TypeCode.SByte => (long)a + (sbyte)b,
                TypeCode.Int16 => (long)a + (short)b,
                TypeCode.UInt16 => (long)a + (ushort)b,
                TypeCode.Int32 => (long)a + (int)b,
                TypeCode.UInt32 => (long)a + (uint)b,
                TypeCode.Int64 => (long)a + (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'long' and 'ulong'"),
                TypeCode.Single => (long)a + (float)b,
                TypeCode.Double => (long)a + (double)b,
                TypeCode.Decimal => (long)a + (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '+' not implemented for types 'long' and {typeCodeB}")
            }),
            TypeCode.UInt64 => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'ulong' and 'bool'"),
                TypeCode.Byte => (ulong)a + (byte)b,
                TypeCode.SByte => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'ulong' and 'sbyte'"),
                TypeCode.Int16 => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'ulong' and 'short'"),
                TypeCode.UInt16 => (ulong)a + (ushort)b,
                TypeCode.Int32 => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'ulong' and 'int'"),
                TypeCode.UInt32 => (ulong)a + (uint)b,
                TypeCode.Int64 => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'ulong' and 'ulong'"),
                TypeCode.UInt64 => (ulong)a + (ulong)b,
                TypeCode.Single => (ulong)a + (float)b,
                TypeCode.Double => (ulong)a + (double)b,
                TypeCode.Decimal => (ulong)a + (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '+' not implemented for types 'ulong' and {typeCodeB}")
            }),
            TypeCode.Single => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'float' and 'bool'"),
                TypeCode.Byte => (float)a + (byte)b,
                TypeCode.SByte => (float)a + (sbyte)b,
                TypeCode.Int16 => (float)a + (short)b,
                TypeCode.UInt16 => (float)a + (ushort)b,
                TypeCode.Int32 => (float)a + (int)b,
                TypeCode.UInt32 => (float)a + (uint)b,
                TypeCode.Int64 => (float)a + (long)b,
                TypeCode.UInt64 => (float)a + (ulong)b,
                TypeCode.Single => (float)a + (float)b,
                TypeCode.Double => (float)a + (double)b,
                TypeCode.Decimal => Convert.ToDecimal(a) + (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '+' not implemented for types 'float' and {typeCodeB}")
            }),
            TypeCode.Double => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'double' and 'bool'"),
                TypeCode.Byte => (double)a + (byte)b,
                TypeCode.SByte => (double)a + (sbyte)b,
                TypeCode.Int16 => (double)a + (short)b,
                TypeCode.UInt16 => (double)a + (ushort)b,
                TypeCode.Int32 => (double)a + (int)b,
                TypeCode.UInt32 => (double)a + (uint)b,
                TypeCode.Int64 => (double)a + (long)b,
                TypeCode.UInt64 => (double)a + (ulong)b,
                TypeCode.Single => (double)a + (float)b,
                TypeCode.Double => (double)a + (double)b,
                TypeCode.Decimal => Convert.ToDecimal(a) + (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '+' not implemented for types 'double' and {typeCodeB}")
            }),
            TypeCode.Decimal => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '+' can't be applied to operands of types 'decimal' and 'bool'"),
                TypeCode.Byte => (decimal)a + (byte)b,
                TypeCode.SByte => (decimal)a + (sbyte)b,
                TypeCode.Int16 => (decimal)a + (short)b,
                TypeCode.UInt16 => (decimal)a + (ushort)b,
                TypeCode.Int32 => (decimal)a + (int)b,
                TypeCode.UInt32 => (decimal)a + (uint)b,
                TypeCode.Int64 => (decimal)a + (long)b,
                TypeCode.UInt64 => (decimal)a + (ulong)b,
                TypeCode.Single => (decimal)a + Convert.ToDecimal(b),
                TypeCode.Double => (decimal)a + Convert.ToDecimal(b),
                TypeCode.Decimal => (decimal)a + (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '+' not implemented for types 'decimal' and {typeCodeB}")
            },
            _ => throw new InvalidOperationException(
                $"Operator '+' not implemented for operands of types {typeCodeA} and {typeCodeB}")
        };
    }

    public static object Subtract(object a, object b)
    {
        return Subtract(a, b, CultureInfo.CurrentCulture);
    }

    public static object Subtract(object a, object b, CultureInfo cultureInfo)
    {
        a = ConvertIfString(a, cultureInfo);
        b = ConvertIfString(b, cultureInfo);

        var typeCodeA = Type.GetTypeCode(a.GetType());
        var typeCodeB = Type.GetTypeCode(b.GetType());

        return typeCodeA switch
        {
            TypeCode.Boolean => throw new InvalidOperationException(
                $"Operator '-' can't be applied to operands of types 'bool' and {typeCodeB}"),
            TypeCode.Byte => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'byte' and 'bool'"),
                TypeCode.Byte => (byte)a - (byte)b,
                TypeCode.SByte => (byte)a - (sbyte)b,
                TypeCode.Int16 => (byte)a - (short)b,
                TypeCode.UInt16 => (byte)a - (ushort)b,
                TypeCode.Int32 => (byte)a - (int)b,
                TypeCode.UInt32 => (byte)a - (uint)b,
                TypeCode.Int64 => (byte)a - (long)b,
                TypeCode.UInt64 => (byte)a - (ulong)b,
                TypeCode.Single => (byte)a - (float)b,
                TypeCode.Double => (byte)a - (double)b,
                TypeCode.Decimal => (byte)a - (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '-' not implemented for operands of types 'byte' and {typeCodeB}")
            },
            TypeCode.SByte => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'sbyte' and 'bool'"),
                TypeCode.Byte => (sbyte)a - (byte)b,
                TypeCode.SByte => (sbyte)a - (sbyte)b,
                TypeCode.Int16 => (sbyte)a - (short)b,
                TypeCode.UInt16 => (sbyte)a - (ushort)b,
                TypeCode.Int32 => (sbyte)a - (int)b,
                TypeCode.UInt32 => (sbyte)a - (uint)b,
                TypeCode.Int64 => (sbyte)a - (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'sbyte' and 'ulong'"),
                TypeCode.Single => (sbyte)a - (float)b,
                TypeCode.Double => (sbyte)a - (double)b,
                TypeCode.Decimal => (sbyte)a - (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '-' not implemented for operands of types 'byte' and {typeCodeB}")
            },
            TypeCode.Int16 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'short' and 'bool'"),
                TypeCode.Byte => (short)a - (byte)b,
                TypeCode.SByte => (short)a - (sbyte)b,
                TypeCode.Int16 => (short)a - (short)b,
                TypeCode.UInt16 => (short)a - (ushort)b,
                TypeCode.Int32 => (short)a - (int)b,
                TypeCode.UInt32 => (short)a - (uint)b,
                TypeCode.Int64 => (short)a - (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'short' and 'ulong'"),
                TypeCode.Single => (short)a - (float)b,
                TypeCode.Double => (short)a - (double)b,
                TypeCode.Decimal => (short)a - (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '-' not implemented for operands of types 'short' and {typeCodeB}")
            },
            TypeCode.UInt16 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'ushort' and 'bool'"),
                TypeCode.Byte => (ushort)a - (byte)b,
                TypeCode.SByte => (ushort)a - (sbyte)b,
                TypeCode.Int16 => (ushort)a - (short)b,
                TypeCode.UInt16 => (ushort)a - (ushort)b,
                TypeCode.Int32 => (ushort)a - (int)b,
                TypeCode.UInt32 => (ushort)a - (uint)b,
                TypeCode.Int64 => (ushort)a - (long)b,
                TypeCode.UInt64 => (ushort)a - (ulong)b,
                TypeCode.Single => (ushort)a - (float)b,
                TypeCode.Double => (ushort)a - (double)b,
                TypeCode.Decimal => (ushort)a - (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '-' not implemented for operands of types 'ushort' and {typeCodeB}")
            },
            TypeCode.Int32 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'int' and 'bool'"),
                TypeCode.Byte => (int)a - (byte)b,
                TypeCode.SByte => (int)a - (sbyte)b,
                TypeCode.Int16 => (int)a - (short)b,
                TypeCode.UInt16 => (int)a - (ushort)b,
                TypeCode.Int32 => (int)a - (int)b,
                TypeCode.UInt32 => (int)a - (uint)b,
                TypeCode.Int64 => (int)a - (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'int' and 'ulong'"),
                TypeCode.Single => (int)a - (float)b,
                TypeCode.Double => (int)a - (double)b,
                TypeCode.Decimal => (int)a - (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '-' not implemented for operands of types 'int' and {typeCodeB}")
            },
            TypeCode.UInt32 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'uint' and 'bool'"),
                TypeCode.Byte => (uint)a - (byte)b,
                TypeCode.SByte => (uint)a - (sbyte)b,
                TypeCode.Int16 => (uint)a - (short)b,
                TypeCode.UInt16 => (uint)a - (ushort)b,
                TypeCode.Int32 => (uint)a - (int)b,
                TypeCode.UInt32 => (uint)a - (uint)b,
                TypeCode.Int64 => (uint)a - (long)b,
                TypeCode.UInt64 => (uint)a - (ulong)b,
                TypeCode.Single => (uint)a - (float)b,
                TypeCode.Double => (uint)a - (double)b,
                TypeCode.Decimal => (uint)a - (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '-' not implemented for operands of types 'uint' and {typeCodeB}")
            },
            TypeCode.Int64 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'long' and 'bool'"),
                TypeCode.Byte => (long)a - (byte)b,
                TypeCode.SByte => (long)a - (sbyte)b,
                TypeCode.Int16 => (long)a - (short)b,
                TypeCode.UInt16 => (long)a - (ushort)b,
                TypeCode.Int32 => (long)a - (int)b,
                TypeCode.UInt32 => (long)a - (uint)b,
                TypeCode.Int64 => (long)a - (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'long' and 'ulong'"),
                TypeCode.Single => (long)a - (float)b,
                TypeCode.Double => (long)a - (double)b,
                TypeCode.Decimal => (long)a - (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '-' not implemented for operands of types 'long' and {typeCodeB}")
            },
            TypeCode.UInt64 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'ulong' and 'bool'"),
                TypeCode.Byte => (ulong)a - (byte)b,
                TypeCode.SByte => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'ulong' and 'sbyte'"),
                TypeCode.Int16 => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'ulong' and 'short'"),
                TypeCode.UInt16 => (ulong)a - (ushort)b,
                TypeCode.Int32 => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'ulong' and 'int'"),
                TypeCode.UInt32 => (ulong)a - (uint)b,
                TypeCode.Int64 => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'ulong' and 'long'"),
                TypeCode.UInt64 => (ulong)a - (ulong)b,
                TypeCode.Single => (ulong)a - (float)b,
                TypeCode.Double => (ulong)a - (double)b,
                TypeCode.Decimal => (ulong)a - (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '-' not implemented for operands of types 'ulong' and {typeCodeB}")
            },
            TypeCode.Single => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'float' and 'bool'"),
                TypeCode.Byte => (float)a - (byte)b,
                TypeCode.SByte => (float)a - (sbyte)b,
                TypeCode.Int16 => (float)a - (short)b,
                TypeCode.UInt16 => (float)a - (ushort)b,
                TypeCode.Int32 => (float)a - (int)b,
                TypeCode.UInt32 => (float)a - (uint)b,
                TypeCode.Int64 => (float)a - (long)b,
                TypeCode.UInt64 => (float)a - (ulong)b,
                TypeCode.Single => (float)a - (float)b,
                TypeCode.Double => (float)a - (double)b,
                TypeCode.Decimal => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'float' and 'decimal'"),
                _ => throw new InvalidOperationException(
                    $"Operator '-' not implemented for operands of types 'float' and {typeCodeB}")
            },
            TypeCode.Double => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'double' and 'bool'"),
                TypeCode.Byte => (double)a - (byte)b,
                TypeCode.SByte => (double)a - (sbyte)b,
                TypeCode.Int16 => (double)a - (short)b,
                TypeCode.UInt16 => (double)a - (ushort)b,
                TypeCode.Int32 => (double)a - (int)b,
                TypeCode.UInt32 => (double)a - (uint)b,
                TypeCode.Int64 => (double)a - (long)b,
                TypeCode.UInt64 => (double)a - (ulong)b,
                TypeCode.Single => (double)a - (float)b,
                TypeCode.Double => (double)a - (double)b,
                TypeCode.Decimal => Convert.ToDecimal(a) - (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '-' not implemented for operands of types 'double' and {typeCodeB}")
            },
            TypeCode.Decimal => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'decimal' and 'bool'"),
                TypeCode.Byte => (decimal)a - (byte)b,
                TypeCode.SByte => (decimal)a - (sbyte)b,
                TypeCode.Int16 => (decimal)a - (short)b,
                TypeCode.UInt16 => (decimal)a - (ushort)b,
                TypeCode.Int32 => (decimal)a - (int)b,
                TypeCode.UInt32 => (decimal)a - (uint)b,
                TypeCode.Int64 => (decimal)a - (long)b,
                TypeCode.UInt64 => (decimal)a - (ulong)b,
                TypeCode.Single => (decimal)a - Convert.ToDecimal(b),
                TypeCode.Double => (decimal)a - Convert.ToDecimal(b),
                TypeCode.Decimal => (decimal)a - (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '-' not implemented for operands of types 'decimal' and {typeCodeB}")
            },
            _ => throw new InvalidOperationException(
                $"Operator '-' not implemented for operands of types {typeCodeA} and {typeCodeB}")
        };
    }

    public static object Multiply(object a, object b)
    {
        return Multiply(a, b, CultureInfo.CurrentCulture);
    }

    public static object Multiply(object a, object b, CultureInfo cultureInfo)
    {
        a = ConvertIfString(a, cultureInfo);
        b = ConvertIfString(b, cultureInfo);

        var typeCodeA = Type.GetTypeCode(a.GetType());
        var typeCodeB = Type.GetTypeCode(b.GetType());

        return typeCodeA switch
        {
            TypeCode.Boolean => throw new InvalidOperationException(
                $"Operator '*' can't be applied to operands of types 'bool' and {typeCodeB}"),
            TypeCode.Byte => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'byte' and 'bool'"),
                TypeCode.Byte => (byte)a * (byte)b,
                TypeCode.SByte => (byte)a * (sbyte)b,
                TypeCode.Int16 => (byte)a * (short)b,
                TypeCode.UInt16 => (byte)a * (ushort)b,
                TypeCode.Int32 => (byte)a * (int)b,
                TypeCode.UInt32 => (byte)a * (uint)b,
                TypeCode.Int64 => (byte)a * (long)b,
                TypeCode.UInt64 => (byte)a * (ulong)b,
                TypeCode.Single => (byte)a * (float)b,
                TypeCode.Double => (byte)a * (double)b,
                TypeCode.Decimal => (byte)a * (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '*' not implemented for operands of types 'byte' and {typeCodeB}")
            }),
            TypeCode.SByte => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'sbyte' and 'bool'"),
                TypeCode.Byte => (sbyte)a * (byte)b,
                TypeCode.SByte => (sbyte)a * (sbyte)b,
                TypeCode.Int16 => (sbyte)a * (short)b,
                TypeCode.UInt16 => (sbyte)a * (ushort)b,
                TypeCode.Int32 => (sbyte)a * (int)b,
                TypeCode.UInt32 => (sbyte)a * (uint)b,
                TypeCode.Int64 => (sbyte)a * (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'sbyte' and 'ulong'"),
                TypeCode.Single => (sbyte)a * (float)b,
                TypeCode.Double => (sbyte)a * (double)b,
                TypeCode.Decimal => (sbyte)a * (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '*' not implemented for operands of types 'sbyte' and {typeCodeB}")
            }),
            TypeCode.Int16 => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'short' and 'bool'"),
                TypeCode.Byte => (short)a * (byte)b,
                TypeCode.SByte => (short)a * (sbyte)b,
                TypeCode.Int16 => (short)a * (short)b,
                TypeCode.UInt16 => (short)a * (ushort)b,
                TypeCode.Int32 => (short)a * (int)b,
                TypeCode.UInt32 => (short)a * (uint)b,
                TypeCode.Int64 => (short)a * (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'short' and 'ulong'"),
                TypeCode.Single => (short)a * (float)b,
                TypeCode.Double => (short)a * (double)b,
                TypeCode.Decimal => (short)a * (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '*' not implemented for operands of types 'short' and {typeCodeB}")
            }),
            TypeCode.UInt16 => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'ushort' and 'bool'"),
                TypeCode.Byte => (ushort)a * (byte)b,
                TypeCode.SByte => (ushort)a * (sbyte)b,
                TypeCode.Int16 => (ushort)a * (short)b,
                TypeCode.UInt16 => (ushort)a * (ushort)b,
                TypeCode.Int32 => (ushort)a * (int)b,
                TypeCode.UInt32 => (ushort)a * (uint)b,
                TypeCode.Int64 => (ushort)a * (long)b,
                TypeCode.UInt64 => (ushort)a * (ulong)b,
                TypeCode.Single => (ushort)a * (float)b,
                TypeCode.Double => (ushort)a * (double)b,
                TypeCode.Decimal => (ushort)a * (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '*' not implemented for operands of types 'ushort' and {typeCodeB}")
            }),
            TypeCode.Int32 => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'int' and 'bool'"),
                TypeCode.Byte => (int)a * (byte)b,
                TypeCode.SByte => (int)a * (sbyte)b,
                TypeCode.Int16 => (int)a * (short)b,
                TypeCode.UInt16 => (int)a * (ushort)b,
                TypeCode.Int32 => (int)a * (int)b,
                TypeCode.UInt32 => (int)a * (uint)b,
                TypeCode.Int64 => (int)a * (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'int' and 'ulong'"),
                TypeCode.Single => (int)a * (float)b,
                TypeCode.Double => (int)a * (double)b,
                TypeCode.Decimal => (int)a * (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '*' not implemented for operands of types 'int' and {typeCodeB}")
            }),
            TypeCode.UInt32 => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'uint' and 'bool'"),
                TypeCode.Byte => (uint)a * (byte)b,
                TypeCode.SByte => (uint)a * (sbyte)b,
                TypeCode.Int16 => (uint)a * (short)b,
                TypeCode.UInt16 => (uint)a * (ushort)b,
                TypeCode.Int32 => (uint)a * (int)b,
                TypeCode.UInt32 => (uint)a * (uint)b,
                TypeCode.Int64 => (uint)a * (long)b,
                TypeCode.UInt64 => (uint)a * (ulong)b,
                TypeCode.Single => (uint)a * (float)b,
                TypeCode.Double => (uint)a * (double)b,
                TypeCode.Decimal => (uint)a * (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '*' not implemented for operands of types 'int' and {typeCodeB}")
            }),
            TypeCode.Int64 => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'long' and 'bool'"),
                TypeCode.Byte => (long)a * (byte)b,
                TypeCode.SByte => (long)a * (sbyte)b,
                TypeCode.Int16 => (long)a * (short)b,
                TypeCode.UInt16 => (long)a * (ushort)b,
                TypeCode.Int32 => (long)a * (int)b,
                TypeCode.UInt32 => (long)a * (uint)b,
                TypeCode.Int64 => (long)a * (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'long' and 'ulong'"),
                TypeCode.Single => (long)a * (float)b,
                TypeCode.Double => (long)a * (double)b,
                TypeCode.Decimal => (long)a * (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '*' not implemented for operands of types 'int' and {typeCodeB}")
            }),
            TypeCode.UInt64 => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'ulong' and 'bool'"),
                TypeCode.Byte => (ulong)a * (byte)b,
                TypeCode.SByte => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'ulong' and 'sbyte'"),
                TypeCode.Int16 => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'ulong' and 'short'"),
                TypeCode.UInt16 => (ulong)a * (ushort)b,
                TypeCode.Int32 => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'ulong' and 'int'"),
                TypeCode.UInt32 => (ulong)a * (uint)b,
                TypeCode.Int64 => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'ulong' and 'long'"),
                TypeCode.UInt64 => (ulong)a * (ulong)b,
                TypeCode.Single => (ulong)a * (float)b,
                TypeCode.Double => (ulong)a * (double)b,
                TypeCode.Decimal => (ulong)a * (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '*' not implemented for operands of types 'ulong' and {typeCodeB}")
            }),
            TypeCode.Single => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'float' and 'bool'"),
                TypeCode.Byte => (float)a * (byte)b,
                TypeCode.SByte => (float)a * (sbyte)b,
                TypeCode.Int16 => (float)a * (short)b,
                TypeCode.UInt16 => (float)a * (ushort)b,
                TypeCode.Int32 => (float)a * (int)b,
                TypeCode.UInt32 => (float)a * (uint)b,
                TypeCode.Int64 => (float)a * (long)b,
                TypeCode.UInt64 => (float)a * (ulong)b,
                TypeCode.Single => (float)a * (float)b,
                TypeCode.Double => (float)a * (double)b,
                TypeCode.Decimal => Convert.ToDecimal(a) * (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '*' not implemented for operands of types 'float' and {typeCodeB}")
            }),
            TypeCode.Double => (typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'double' and 'bool'"),
                TypeCode.Byte => (double)a * (byte)b,
                TypeCode.SByte => (double)a * (sbyte)b,
                TypeCode.Int16 => (double)a * (short)b,
                TypeCode.UInt16 => (double)a * (ushort)b,
                TypeCode.Int32 => (double)a * (int)b,
                TypeCode.UInt32 => (double)a * (uint)b,
                TypeCode.Int64 => (double)a * (long)b,
                TypeCode.UInt64 => (double)a * (ulong)b,
                TypeCode.Single => (double)a * (float)b,
                TypeCode.Double => (double)a * (double)b,
                TypeCode.Decimal => Convert.ToDecimal(a) * (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '*' not implemented for operands of types 'double' and {typeCodeB}")
            }),
            TypeCode.Decimal => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '*' can't be applied to operands of types 'decimal' and 'bool'"),
                TypeCode.Byte => (decimal)a * (byte)b,
                TypeCode.SByte => (decimal)a * (sbyte)b,
                TypeCode.Int16 => (decimal)a * (short)b,
                TypeCode.UInt16 => (decimal)a * (ushort)b,
                TypeCode.Int32 => (decimal)a * (int)b,
                TypeCode.UInt32 => (decimal)a * (uint)b,
                TypeCode.Int64 => (decimal)a * (long)b,
                TypeCode.UInt64 => (decimal)a * (ulong)b,
                TypeCode.Single => (decimal)a * Convert.ToDecimal(b),
                TypeCode.Double => (decimal)a * Convert.ToDecimal(b),
                TypeCode.Decimal => (decimal)a * (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '*' not implemented for operands of types 'decimal' and {typeCodeB}")
            },
            _ => throw new InvalidOperationException(
                $"Operator '*' not implemented for operands of types {typeCodeA} and {typeCodeB}")
        };
    }
        
    public static object Divide(object a, object b)
    {
        return Divide(a, b, CultureInfo.CurrentCulture);
    }

    public static object Divide(object a, object b, CultureInfo cultureInfo)
    {
        a = ConvertIfString(a, cultureInfo);
        b = ConvertIfString(b, cultureInfo);

        var typeCodeA = Type.GetTypeCode(a.GetType());
        var typeCodeB = Type.GetTypeCode(b.GetType());

        return typeCodeA switch
        {
            TypeCode.Boolean => throw new InvalidOperationException(
                $"Operator '/' can't be applied to operands of types 'bool' and {typeCodeB}"),
            TypeCode.Byte => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'byte' and 'bool'"),
                TypeCode.Byte => (byte)a / (byte)b,
                TypeCode.SByte => (byte)a / (sbyte)b,
                TypeCode.Int16 => (byte)a / (short)b,
                TypeCode.UInt16 => (byte)a / (ushort)b,
                TypeCode.Int32 => (byte)a / (int)b,
                TypeCode.UInt32 => (byte)a / (uint)b,
                TypeCode.Int64 => (byte)a / (long)b,
                TypeCode.UInt64 => (byte)a / (ulong)b,
                TypeCode.Single => (byte)a / (float)b,
                TypeCode.Double => (byte)a / (double)b,
                TypeCode.Decimal => (byte)a / (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '/' not implemented for operands of types 'decimal' and {typeCodeB}")
            },
            TypeCode.SByte => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'sbyte' and 'bool'"),
                TypeCode.Byte => (sbyte)a / (byte)b,
                TypeCode.SByte => (sbyte)a / (sbyte)b,
                TypeCode.Int16 => (sbyte)a / (short)b,
                TypeCode.UInt16 => (sbyte)a / (ushort)b,
                TypeCode.Int32 => (sbyte)a / (int)b,
                TypeCode.UInt32 => (sbyte)a / (uint)b,
                TypeCode.Int64 => (sbyte)a / (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'sbyte' and 'ulong'"),
                TypeCode.Single => (sbyte)a / (float)b,
                TypeCode.Double => (sbyte)a / (double)b,
                TypeCode.Decimal => (sbyte)a / (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '/' not implemented for operands of types 'decimal' and {typeCodeB}")
            },
            TypeCode.Int16 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'short' and 'bool'"),
                TypeCode.Byte => (short)a / (byte)b,
                TypeCode.SByte => (short)a / (sbyte)b,
                TypeCode.Int16 => (short)a / (short)b,
                TypeCode.UInt16 => (short)a / (ushort)b,
                TypeCode.Int32 => (short)a / (int)b,
                TypeCode.UInt32 => (short)a / (uint)b,
                TypeCode.Int64 => (short)a / (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'short' and 'ulong'"),
                TypeCode.Single => (short)a / (float)b,
                TypeCode.Double => (short)a / (double)b,
                TypeCode.Decimal => (short)a / (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '/' not implemented for operands of types 'decimal' and {typeCodeB}")
            },
            TypeCode.UInt16 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'ushort' and 'bool'"),
                TypeCode.Byte => (ushort)a / (byte)b,
                TypeCode.SByte => (ushort)a / (sbyte)b,
                TypeCode.Int16 => (ushort)a / (short)b,
                TypeCode.UInt16 => (ushort)a / (ushort)b,
                TypeCode.Int32 => (ushort)a / (int)b,
                TypeCode.UInt32 => (ushort)a / (uint)b,
                TypeCode.Int64 => (ushort)a / (long)b,
                TypeCode.UInt64 => (ushort)a / (ulong)b,
                TypeCode.Single => (ushort)a / (float)b,
                TypeCode.Double => (ushort)a / (double)b,
                TypeCode.Decimal => (ushort)a / (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '/' not implemented for operands of types 'ushort' and {typeCodeB}")
            },
            TypeCode.Int32 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'int' and 'bool'"),
                TypeCode.Byte => (int)a / (byte)b,
                TypeCode.SByte => (int)a / (sbyte)b,
                TypeCode.Int16 => (int)a / (short)b,
                TypeCode.UInt16 => (int)a / (ushort)b,
                TypeCode.Int32 => (int)a / (int)b,
                TypeCode.UInt32 => (int)a / (uint)b,
                TypeCode.Int64 => (int)a / (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'int' and 'ulong'"),
                TypeCode.Single => (int)a / (float)b,
                TypeCode.Double => (int)a / (double)b,
                TypeCode.Decimal => (int)a / (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '/' not implemented for operands of types 'int' and {typeCodeB}")
            },
            TypeCode.UInt32 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'uint' and 'bool'"),
                TypeCode.Byte => (uint)a / (byte)b,
                TypeCode.SByte => (uint)a / (sbyte)b,
                TypeCode.Int16 => (uint)a / (short)b,
                TypeCode.UInt16 => (uint)a / (ushort)b,
                TypeCode.Int32 => (uint)a / (int)b,
                TypeCode.UInt32 => (uint)a / (uint)b,
                TypeCode.Int64 => (uint)a / (long)b,
                TypeCode.UInt64 => (uint)a / (ulong)b,
                TypeCode.Single => (uint)a / (float)b,
                TypeCode.Double => (uint)a / (double)b,
                TypeCode.Decimal => (uint)a / (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '/' not implemented for operands of types 'uint' and {typeCodeB}")
            },
            TypeCode.Int64 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'long' and 'bool'"),
                TypeCode.Byte => (long)a / (byte)b,
                TypeCode.SByte => (long)a / (sbyte)b,
                TypeCode.Int16 => (long)a / (short)b,
                TypeCode.UInt16 => (long)a / (ushort)b,
                TypeCode.Int32 => (long)a / (int)b,
                TypeCode.UInt32 => (long)a / (uint)b,
                TypeCode.Int64 => (long)a / (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'long' and 'ulong'"),
                TypeCode.Single => (long)a / (float)b,
                TypeCode.Double => (long)a / (double)b,
                TypeCode.Decimal => (long)a / (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '/' not implemented for operands of types 'long' and {typeCodeB}")
            },
            TypeCode.UInt64 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '-' can't be applied to operands of types 'ulong' and 'bool'"),
                TypeCode.Byte => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'ulong' and 'byte'"),
                TypeCode.SByte => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'ulong' and 'sbyte'"),
                TypeCode.Int16 => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'ulong' and 'short'"),
                TypeCode.UInt16 => (ulong)a / (ushort)b,
                TypeCode.Int32 => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'ulong' and 'int'"),
                TypeCode.UInt32 => (ulong)a / (uint)b,
                TypeCode.Int64 => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'ulong' and 'long'"),
                TypeCode.UInt64 => (ulong)a / (ulong)b,
                TypeCode.Single => (ulong)a / (float)b,
                TypeCode.Double => (ulong)a / (double)b,
                TypeCode.Decimal => (ulong)a / (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '/' not implemented for operands of types 'ulong' and {typeCodeB}")
            },
            TypeCode.Single => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'float' and 'bool'"),
                TypeCode.Byte => (float)a / (byte)b,
                TypeCode.SByte => (float)a / (sbyte)b,
                TypeCode.Int16 => (float)a / (short)b,
                TypeCode.UInt16 => (float)a / (ushort)b,
                TypeCode.Int32 => (float)a / (int)b,
                TypeCode.UInt32 => (float)a / (uint)b,
                TypeCode.Int64 => (float)a / (long)b,
                TypeCode.UInt64 => (float)a / (ulong)b,
                TypeCode.Single => (float)a / (float)b,
                TypeCode.Double => (float)a / (double)b,
                TypeCode.Decimal => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'float' and 'decimal'"),
                _ => throw new InvalidOperationException(
                    $"Operator '/' not implemented for operands of types 'ulong' and {typeCodeB}")
            },
            TypeCode.Double => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'double' and 'bool'"),
                TypeCode.Byte => (double)a / (byte)b,
                TypeCode.SByte => (double)a / (sbyte)b,
                TypeCode.Int16 => (double)a / (short)b,
                TypeCode.UInt16 => (double)a / (ushort)b,
                TypeCode.Int32 => (double)a / (int)b,
                TypeCode.UInt32 => (double)a / (uint)b,
                TypeCode.Int64 => (double)a / (long)b,
                TypeCode.UInt64 => (double)a / (ulong)b,
                TypeCode.Single => (double)a / (float)b,
                TypeCode.Double => (double)a / (double)b,
                TypeCode.Decimal => Convert.ToDecimal(a) / (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '/' not implemented for operands of types 'double' and {typeCodeB}")
            },
            TypeCode.Decimal => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '/' can't be applied to operands of types 'decimal' and 'bool'"),
                TypeCode.Byte => (decimal)a / (sbyte)b,
                TypeCode.SByte => (decimal)a / (sbyte)b,
                TypeCode.Int16 => (decimal)a / (short)b,
                TypeCode.UInt16 => (decimal)a / (ushort)b,
                TypeCode.Int32 => (decimal)a / (int)b,
                TypeCode.UInt32 => (decimal)a / (uint)b,
                TypeCode.Int64 => (decimal)a / (long)b,
                TypeCode.UInt64 => (decimal)a / (ulong)b,
                TypeCode.Single => (decimal)a / Convert.ToDecimal(b),
                TypeCode.Double => (decimal)a / Convert.ToDecimal(b),
                TypeCode.Decimal => (decimal)a / (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '/' not implemented for operands of types 'decimal' and {typeCodeB}")
            },
            _ => throw new InvalidOperationException(
                $"Operator '/' not implemented for operands of types {typeCodeA} and {typeCodeB}")
        };
    }

    public static object Modulo(object a, object b)
    {
        return Modulo(a, b, CultureInfo.CurrentCulture);
    }

    public static object Modulo(object a, object b, CultureInfo cultureInfo)
    {
        a = ConvertIfString(a, cultureInfo);
        b = ConvertIfString(b, cultureInfo);

        var typeCodeA = Type.GetTypeCode(a.GetType());
        var typeCodeB = Type.GetTypeCode(b.GetType());

        return typeCodeA switch
        {
            TypeCode.Boolean => throw new InvalidOperationException(
                $"Operator '%' can't be applied to operands of types 'bool' and {typeCodeB}"),
            TypeCode.Byte => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'byte' and 'bool'"),
                TypeCode.Byte => (byte)a % (byte)b,
                TypeCode.SByte => (byte)a % (sbyte)b,
                TypeCode.Int16 => (byte)a % (short)b,
                TypeCode.UInt16 => (byte)a % (ushort)b,
                TypeCode.Int32 => (byte)a % (int)b,
                TypeCode.UInt32 => (byte)a % (uint)b,
                TypeCode.Int64 => (byte)a % (long)b,
                TypeCode.UInt64 => (byte)a % (ulong)b,
                TypeCode.Single => (byte)a % (float)b,
                TypeCode.Double => (byte)a % (double)b,
                TypeCode.Decimal => (byte)a % (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '/' not implemented for operands of types 'byte' and {typeCodeB}")
            },
            TypeCode.SByte => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'sbyte' and 'bool'"),
                TypeCode.Byte => (sbyte)a % (byte)b,
                TypeCode.SByte => (sbyte)a % (sbyte)b,
                TypeCode.Int16 => (sbyte)a % (short)b,
                TypeCode.UInt16 => (sbyte)a % (ushort)b,
                TypeCode.Int32 => (sbyte)a % (int)b,
                TypeCode.UInt32 => (sbyte)a % (uint)b,
                TypeCode.Int64 => (sbyte)a % (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'sbyte' and 'ulong'"),
                TypeCode.Single => (sbyte)a % (float)b,
                TypeCode.Double => (sbyte)a % (double)b,
                TypeCode.Decimal => (sbyte)a % (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '/' not implemented for operands of types 'sbyte' and {typeCodeB}")
            },
            TypeCode.Int16 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'short' and 'bool'"),
                TypeCode.Byte => (short)a % (byte)b,
                TypeCode.SByte => (short)a % (sbyte)b,
                TypeCode.Int16 => (short)a % (short)b,
                TypeCode.UInt16 => (short)a % (ushort)b,
                TypeCode.Int32 => (short)a % (int)b,
                TypeCode.UInt32 => (short)a % (uint)b,
                TypeCode.Int64 => (short)a % (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'short' and 'ulong'"),
                TypeCode.Single => (short)a % (float)b,
                TypeCode.Double => (short)a % (double)b,
                TypeCode.Decimal => (short)a % (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '%' not implemented for operands of types 'short' and {typeCodeB}")
            },
            TypeCode.UInt16 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'ushort' and 'bool'"),
                TypeCode.Byte => (ushort)a % (byte)b,
                TypeCode.SByte => (ushort)a % (sbyte)b,
                TypeCode.Int16 => (ushort)a % (short)b,
                TypeCode.UInt16 => (ushort)a % (ushort)b,
                TypeCode.Int32 => (ushort)a % (int)b,
                TypeCode.UInt32 => (ushort)a % (uint)b,
                TypeCode.Int64 => (ushort)a % (long)b,
                TypeCode.UInt64 => (ushort)a % (ulong)b,
                TypeCode.Single => (ushort)a % (float)b,
                TypeCode.Double => (ushort)a % (double)b,
                TypeCode.Decimal => (ushort)a % (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '%' not implemented for operands of types 'ushort' and {typeCodeB}")
            },
            TypeCode.Int32 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'int' and 'bool'"),
                TypeCode.Byte => (int)a % (byte)b,
                TypeCode.SByte => (int)a % (sbyte)b,
                TypeCode.Int16 => (int)a % (short)b,
                TypeCode.UInt16 => (int)a % (ushort)b,
                TypeCode.Int32 => (int)a % (int)b,
                TypeCode.UInt32 => (int)a % (uint)b,
                TypeCode.Int64 => (int)a % (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'int' and 'ulong'"),
                TypeCode.Single => (int)a % (float)b,
                TypeCode.Double => (int)a % (double)b,
                TypeCode.Decimal => (int)a % (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '%' not implemented for operands of types 'int' and {typeCodeB}")
            },
            TypeCode.UInt32 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'uint' and 'bool'"),
                TypeCode.Byte => (uint)a % (byte)b,
                TypeCode.SByte => (uint)a % (sbyte)b,
                TypeCode.Int16 => (uint)a % (short)b,
                TypeCode.UInt16 => (uint)a % (ushort)b,
                TypeCode.Int32 => (uint)a % (int)b,
                TypeCode.UInt32 => (uint)a % (uint)b,
                TypeCode.Int64 => (uint)a % (long)b,
                TypeCode.UInt64 => (uint)a % (ulong)b,
                TypeCode.Single => (uint)a % (float)b,
                TypeCode.Double => (uint)a % (double)b,
                TypeCode.Decimal => (uint)a % (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '%' not implemented for operands of types 'uint' and {typeCodeB}")
            },
            TypeCode.Int64 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'long' and 'bool'"),
                TypeCode.Byte => (long)a % (byte)b,
                TypeCode.SByte => (long)a % (sbyte)b,
                TypeCode.Int16 => (long)a % (short)b,
                TypeCode.UInt16 => (long)a % (ushort)b,
                TypeCode.Int32 => (long)a % (int)b,
                TypeCode.UInt32 => (long)a % (uint)b,
                TypeCode.Int64 => (long)a % (long)b,
                TypeCode.UInt64 => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'long' and 'ulong'"),
                TypeCode.Single => (long)a % (float)b,
                TypeCode.Double => (long)a % (double)b,
                TypeCode.Decimal => (long)a % (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '%' not implemented for operands of types 'long' and {typeCodeB}")
            },
            TypeCode.UInt64 => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'ulong' and 'bool'"),
                TypeCode.Byte => (ulong)a % (byte)b,
                TypeCode.SByte => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'ulong' and 'sbyte'"),
                TypeCode.Int16 => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'ulong' and 'short'"),
                TypeCode.UInt16 => (ulong)a % (ushort)b,
                TypeCode.Int32 => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'ulong' and 'int'"),
                TypeCode.UInt32 => (ulong)a % (uint)b,
                TypeCode.Int64 => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'ulong' and 'long'"),
                TypeCode.UInt64 => (ulong)a % (ulong)b,
                TypeCode.Single => (ulong)a % (float)b,
                TypeCode.Double => (ulong)a % (double)b,
                TypeCode.Decimal => (ulong)a % (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '%' not implemented for operands of types 'ulong' and {typeCodeB}")
            },
            TypeCode.Single => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'float' and 'bool'"),
                TypeCode.Byte => (float)a % (byte)b,
                TypeCode.SByte => (float)a % (sbyte)b,
                TypeCode.Int16 => (float)a % (short)b,
                TypeCode.UInt16 => (float)a % (ushort)b,
                TypeCode.Int32 => (float)a % (int)b,
                TypeCode.UInt32 => (float)a % (uint)b,
                TypeCode.Int64 => (float)a % (long)b,
                TypeCode.UInt64 => (float)a % (ulong)b,
                TypeCode.Single => (float)a % (float)b,
                TypeCode.Double => (float)a % (double)b,
                TypeCode.Decimal => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'float' and 'decimal'"),
                _ => throw new InvalidOperationException(
                    $"Operator '%' not implemented for operands of types 'long' and {typeCodeB}")
            },
            TypeCode.Double => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'double' and 'bool'"),
                TypeCode.Byte => (double)a % (byte)b,
                TypeCode.SByte => (double)a % (sbyte)b,
                TypeCode.Int16 => (double)a % (short)b,
                TypeCode.UInt16 => (double)a % (ushort)b,
                TypeCode.Int32 => (double)a % (int)b,
                TypeCode.UInt32 => (double)a % (uint)b,
                TypeCode.Int64 => (double)a % (long)b,
                TypeCode.UInt64 => (double)a % (ulong)b,
                TypeCode.Single => (double)a % (float)b,
                TypeCode.Double => (double)a % (double)b,
                TypeCode.Decimal => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'double' and 'decimal'"),
                _ => throw new InvalidOperationException(
                    $"Operator '%' not implemented for operands of types 'double' and {typeCodeB}")
            },
            TypeCode.Decimal => typeCodeB switch
            {
                TypeCode.Boolean => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'decimal' and 'bool'"),
                TypeCode.Byte => (decimal)a % (byte)b,
                TypeCode.SByte => (decimal)a % (sbyte)b,
                TypeCode.Int16 => (decimal)a % (short)b,
                TypeCode.UInt16 => (decimal)a % (ushort)b,
                TypeCode.Int32 => (decimal)a % (int)b,
                TypeCode.UInt32 => (decimal)a % (uint)b,
                TypeCode.Int64 => (decimal)a % (long)b,
                TypeCode.UInt64 => (decimal)a % (ulong)b,
                TypeCode.Single => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'decimal' and 'float'"),
                TypeCode.Double => throw new InvalidOperationException(
                    "Operator '%' can't be applied to operands of types 'decimal' and 'decimal'"),
                TypeCode.Decimal => (decimal)a % (decimal)b,
                _ => throw new InvalidOperationException(
                    $"Operator '%' not implemented for operands of types 'decimal' and {typeCodeB}")
            },
            _ => throw new InvalidOperationException(
                $"Operator '+' not implemented for operands of types {typeCodeA} and {typeCodeB}")
        };
    }

    public static object Max(object a, object b)
    {
        return Max(a, b, CultureInfo.CurrentCulture);
    }

    public static object Max(object a, object b, CultureInfo cultureInfo)
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

        TypeCode typeCodeA = Type.GetTypeCode(a.GetType());
        TypeCode typeCodeB = Type.GetTypeCode(b.GetType());

        return typeCodeA switch
        {
            TypeCode.Byte => Math.Max((byte)a, Convert.ToByte(b)),
            TypeCode.SByte => Math.Max((sbyte)a, Convert.ToSByte(b)),
            TypeCode.Int16 => Math.Max((short)a, Convert.ToInt16(b)),
            TypeCode.UInt16 => Math.Max((ushort)a, Convert.ToUInt16(b)),
            TypeCode.Int32 => Math.Max((int)a, Convert.ToInt32(b)),
            TypeCode.UInt32 => Math.Max((uint)a, Convert.ToUInt32(b)),
            TypeCode.Int64 => Math.Max((long)a, Convert.ToInt64(b)),
            TypeCode.UInt64 => Math.Max((ulong)a, Convert.ToUInt64(b)),
            TypeCode.Single => Math.Max((float)a, Convert.ToSingle(b)),
            TypeCode.Double => Math.Max((double)a, Convert.ToDouble(b)),
            TypeCode.Decimal => Math.Max((decimal)a, Convert.ToDecimal(b)),
            _ => throw new InvalidOperationException(
                $"Max not implemented for parameters of {typeCodeA} and {typeCodeB}")
        };
    }

    public static object Min(object a, object b)
    {
        return Min(a, b, CultureInfo.CurrentCulture);
    }

    public static object Min(object a, object b, CultureInfo cultureInfo)
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

        var typeCodeA = Type.GetTypeCode(a.GetType());
        var typeCodeB = Type.GetTypeCode(b.GetType());

        return typeCodeA switch
        {
            TypeCode.Byte => Math.Min((byte)a, Convert.ToByte(b)),
            TypeCode.SByte => Math.Min((sbyte)a, Convert.ToSByte(b)),
            TypeCode.Int16 => Math.Min((short)a, Convert.ToInt16(b)),
            TypeCode.UInt16 => Math.Min((ushort)a, Convert.ToUInt16(b)),
            TypeCode.Int32 => Math.Min((int)a, Convert.ToInt32(b)),
            TypeCode.UInt32 => Math.Min((uint)a, Convert.ToUInt32(b)),
            TypeCode.Int64 => Math.Min((long)a, Convert.ToInt64(b)),
            TypeCode.UInt64 => Math.Min((ulong)a, Convert.ToUInt64(b)),
            TypeCode.Single => Math.Min((float)a, Convert.ToSingle(b)),
            TypeCode.Double => Math.Min((double)a, Convert.ToDouble(b)),
            TypeCode.Decimal => Math.Min((decimal)a, Convert.ToDecimal(b)),
            _ => throw new InvalidOperationException(
                $"Max not implemented for parameters of {typeCodeA} and {typeCodeB}")
        };
    }
}