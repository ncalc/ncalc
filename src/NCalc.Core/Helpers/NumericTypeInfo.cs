namespace NCalc.Helpers;

internal readonly struct NumericTypeInfo
{
    public TypeCode TypeCode { get; }

    public int BitSize { get; }

    public bool IsFloatingPoint { get; }

    public bool IsUnsigned { get; }

    public TypeCode ExpandedTypeCode { get; }

    // ReSharper disable once ConvertToPrimaryConstructor
    public NumericTypeInfo(TypeCode typeCode, int bitSize, bool isFloatingPoint, bool isUnsigned, TypeCode expandedTypeCode)
    {
        TypeCode = typeCode;
        BitSize = bitSize;
        IsFloatingPoint = isFloatingPoint;
        IsUnsigned = isUnsigned;
        ExpandedTypeCode = expandedTypeCode;
    }
}