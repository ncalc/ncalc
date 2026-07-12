namespace NCalc.Parser;

public enum DefaultNumberType
{
    Double,

    /// <summary>
    /// Uses <see cref="decimal"/> as the default parsed and coerced number type.
    /// </summary>
    Decimal,

    Int32,

    /// <summary>
    /// Uses <see cref="long"/> as the default parsed and coerced integral number type.
    /// </summary>
    Int64
}
