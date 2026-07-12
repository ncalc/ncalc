namespace NCalc.Parser;

public enum IntegerNumberType
{
    /// <summary>
    /// Parses integer literals as <see cref="int"/> when possible, otherwise as <see cref="long"/>.
    /// </summary>
    Auto,

    Int32,

    Int64
}
