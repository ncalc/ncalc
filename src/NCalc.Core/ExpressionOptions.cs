namespace NCalc;

/// <summary>
/// Options used for both parsing and evaluation of an expression.
/// </summary>
[Flags]
public enum ExpressionOptions
{
    /// <summary>
    /// Specifies that no options are set.
    /// </summary>
    None = 1 << 0,

    /// <summary>
    /// Specifies case-insensitive matching for built-in functions
    /// </summary>
    IgnoreCaseAtBuiltInFunctions = 1 << 1,

    /// <summary>
    /// No-cache mode. Ignores any pre-compiled expression in the cache.
    /// </summary>
    NoCache = 1 << 2,

    /// <summary>
    /// Treats parameters as arrays and returns a set of results.
    /// </summary>
    IterateParameters = 1 << 3,

    /// <summary>
    /// When using Round(), if a number is halfway between two others, it is rounded toward the nearest number that is away from zero.
    /// </summary>
    RoundAwayFromZero = 1 << 4,

    /// <summary>
    /// Specifies the use of CaseInsensitiveComparer for comparisons.
    /// </summary>
    CaseInsensitiveStringComparer = 1 << 5,

    /// <summary>
    /// Uses decimals instead of doubles as default floating point data type.
    /// </summary>
    DecimalAsDefault = 1 << 6,

    /// <summary>
    /// Defines a "null" parameter and allows comparison of values to null.
    /// </summary>
    AllowNullParameter = 1 << 7,

    /// <summary>
    /// Use ordinal culture on string compare
    /// </summary>
    OrdinalStringComparer = 1 << 8,

    /// <summary>
    /// Allow calculation with <see cref="bool"/> values
    /// </summary>
    AllowBooleanCalculation = 1 << 9,

    /// <summary>
    /// Check for arithmetic binary operation overflow
    /// </summary>
    OverflowProtection = 1 << 10,

    /// <summary>
    /// Concat values as strings instead of arithmetic addition
    /// </summary>
    StringConcat = 1 << 11,

    /// <summary>
    /// Parse single quoted strings as <see cref="char"/>
    /// </summary>
    AllowCharValues = 1 << 12
}